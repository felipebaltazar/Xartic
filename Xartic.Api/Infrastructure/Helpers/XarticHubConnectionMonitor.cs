using Microsoft.AspNetCore.Connections.Features;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xartic.Api.Abstractions;
using Xartic.Api.Domain.Models;
using Xartic.Api.Extensions;
using Xartic.Api.Hub;
using Xartic.Api.Infrastructure.Exceptions;
using Xartic.Core;

namespace Xartic.Api.Infrastructure.Helpers
{
    public sealed class XarticHubConnectionMonitor : IConnectionMonitor<XarticHub>
    {
        #region Fields

        private readonly List<HubClient> _connections = new List<HubClient>();
        private readonly HashSet<string> _pendingConnections = new HashSet<string>();

        private readonly object _pendingConnectionsLock = new object();
        private readonly object _connectionsLock = new object();

        #endregion

        #region IConectionMonitor<XarticHub>

        /// <inheritdoc/>
        public async Task InitConnectionMonitoringAsync(HubCallerContext context, XarticHub hub)
        {
            var id = context.ConnectionId;
            var userName = context?.ToUserName();
            var roomName = context?.ToRoomName();

            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(roomName))
            {
                context.Abort();
                throw new ConnectionAbortedException(nameof(XarticHub), $"Conexão inválida para usuário '{userName}' e sala '{roomName}'");
            }

            await hub.Groups.AddToGroupAsync(id, roomName).ConfigureAwait(false);

            _connections.Add(new HubClient(id, userName, roomName));

            var feature = context.Features.Get<IConnectionHeartbeatFeature>();
            feature.OnHeartbeat(state =>
            {
                if (_pendingConnections.Contains(context.ConnectionId))
                {
                    context.Abort();
                    lock (_pendingConnectionsLock)
                    {
                        _pendingConnections.Remove(context.ConnectionId);
                    }
                }

            }, context.ConnectionId);

            var players = GetByGroup(roomName).Select(s => new Player(s));
            var status = RoomStatus.Build(roomName, players);
            await hub.OnRoomStatusChanged(status).ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public string GetId(string username) => GetConnectionId(username);

        /// <inheritdoc/>
        public IEnumerable<string> GetByGroup(string groupName) =>
            _connections.Where(c => c.RoomName == groupName).Select(c => c.Username);

        /// <inheritdoc/>
        public async Task OnDisconnectedAsync(XarticHub hub, Exception exception)
        {
            var id = hub.Context.ConnectionId;
            var roomName = hub.Context.ToRoomName();

            lock (_connectionsLock)
            {
                var connToRemove = _connections.FirstOrDefault(c => c.ConnectionId == id);
                if (connToRemove == null) return;

                _connections.Remove(connToRemove);
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(roomName))
                {
                    var players = GetByGroup(roomName).Select(s => new Player(s));
                    var status = RoomStatus.Build(roomName, players);

                    await hub.OnRoomStatusChanged(status).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                //TODO: Fazer logs
            }

        }

        /// <inheritdoc/>
        public bool CloseConnection(string userName)
        {
            var connection = _connections.FirstOrDefault(c => c.Username == userName);

            if (string.IsNullOrEmpty(connection?.ConnectionId)) return false;

            var connectionId = connection?.ConnectionId;
            if (!_pendingConnections.Contains(connectionId))
            {
                lock (_pendingConnectionsLock)
                {
                    _pendingConnections.Add(connectionId);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region Private Methods

        private string GetConnectionId(string username)
        {
            lock (_connectionsLock)
            {
                return _connections.FirstOrDefault(c => c.Username == username)?.ConnectionId;
            }
        }

        #endregion
    }
}
