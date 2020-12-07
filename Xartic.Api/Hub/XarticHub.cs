using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xartic.Api.Abstractions;
using Xartic.Api.Domain;
using Xartic.Api.Domain.Models;
using Xartic.Api.Extensions;
using Xartic.Api.Infrastructure.Exceptions;
using Xartic.Core;
using SignalHub = Microsoft.AspNetCore.SignalR.Hub;

namespace Xartic.Api.Hub
{
    public class XarticHub : SignalHub
    {
        #region Fields

        private static readonly GameController _gameController = new GameController();

        private readonly IConnectionMonitor<XarticHub> _connectionMonitor;

        #endregion

        #region Constructors

        public XarticHub(IConnectionMonitor<XarticHub> connectionMonitor)
        {
            _connectionMonitor = connectionMonitor;
        }

        #endregion

        #region Hub Methods

        [HubMethodName(nameof(Clear))]
        public Task Clear(string username) =>
            Clients.All.SendCoreAsync(nameof(Clear), new object[] { username });

        [HubMethodName(nameof(CheckRoomStatus))]
        public async Task CheckRoomStatus(string username) {
            var connectionId = _connectionMonitor.GetId(username);
            var roomName = Context.ToRoomName();
            var players = _connectionMonitor.GetByGroup(roomName).Select(s => new Player(s));
            var status = RoomStatus.Build(roomName, players, new Player(username));

            await Clients.Client(connectionId).SendCoreAsync(nameof(OnRoomStatusChanged), new object[] { status }).ConfigureAwait(false);
        }

        [HubMethodName(nameof(Draw))]
        public Task Draw(string username, DrawCommand drawCommand) =>
            Clients.All.SendCoreAsync(nameof(Draw), new object[] { drawCommand.Color.Hex, drawCommand.Position.X, drawCommand.Position.Y, drawCommand.Radius, drawCommand.IsMouseDown });

        [HubMethodName(nameof(Message))]
        public async Task Message(string username, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var result = _gameController.OnResponse(message);
            var roomName = Context.ToRoomName();

            //Envia mensagem de tentativa para todo o chat
            await Clients.Group(roomName).SendCoreAsync(nameof(Message), new object[] { username, message }).ConfigureAwait(false);

            //Envia mensagem de resposta à tentativa do usuário
            if (result is AllMatchResult)
            {
                await ResponseResult(username, "Você acertou!").ConfigureAwait(false);

                //Envia mensagem que o usuário acertou, para todo o chat
                var players = _connectionMonitor.GetByGroup(roomName).Select(s => new Player(s));
                var status = RoomStatus.Build(roomName, players, new Player(username));

                await OnRoomStatusChanged(status).ConfigureAwait(false);
                await Clients.Group(roomName).SendCoreAsync(nameof(ResponseResult), new object[] {$"{username} acertou!" }).ConfigureAwait(false);
            }
            else if (result is IsClosestResult)
            {
                await ResponseResult(username, $"{message} está perto!").ConfigureAwait(false);
            }
        }

        [HubMethodName(nameof(ResponseResult))]
        public Task ResponseResult(string username, string result)
        {
            var connectionId = _connectionMonitor.GetId(username);
            return Clients.Client(connectionId).SendCoreAsync(nameof(ResponseResult), new object[] { result });
        }

        [HubMethodName(nameof(OnRoomStatusChanged))]
        public Task OnRoomStatusChanged(RoomStatus status) =>
            Clients.Group(status.RoomName).SendCoreAsync(nameof(OnRoomStatusChanged), new object[] { status });

        #endregion

        #region Overrides

        public override async Task OnConnectedAsync()
        {
            try
            {
                await base.OnConnectedAsync().ConfigureAwait(false);
                await _connectionMonitor.InitConnectionMonitoringAsync(Context, this).ConfigureAwait(false);
            }
            catch (ConnectionAbortedException)
            {
                //TODO: Fazer logs
            }
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
                await _connectionMonitor.OnDisconnectedAsync(this, exception).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //TODO: Fazer logs
            }
        }

        #endregion
    }
}
