using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xartic.Api.Abstractions;
using Xartic.Api.Domain;
using Xartic.Api.Domain.Models;
using Xartic.Api.Extensions;
using Xartic.Api.Infrastructure.Abstractions;
using Xartic.Api.Infrastructure.Exceptions;
using Xartic.Core;
using SignalHub = Microsoft.AspNetCore.SignalR.Hub;

namespace Xartic.Api.Hub
{
    public class XarticHub : SignalHub
    {
        #region Fields

        private static readonly IGameController _gameController = new GameController();

        private readonly IConnectionMonitor<XarticHub> _connectionMonitor;

        #endregion

        #region Constructors

        public XarticHub(IConnectionMonitor<XarticHub> connectionMonitor)
        {
            _connectionMonitor = connectionMonitor;
        }

        #endregion

        #region Hub Methods

        /// <summary>
        /// Initialize the game
        /// </summary>
        /// <returns></returns>
        [HubMethodName(nameof(StartGame))]
        public Task StartGame()
        {
            var roomName = Context.ToRoomName();
            var currentWord = _gameController.StartGame(roomName, Context.ConnectionId);

            return Clients.Client(Context.ConnectionId).SendCoreAsync("OnGameWordChanged", new object[] { currentWord });
        }

        /// <summary>
        /// Clear the draw on canvas
        /// </summary>
        /// <param name="roomName">Room that currently has executed this action</param>
        /// <returns></returns>
        [HubMethodName(nameof(Clear))]
        public Task Clear()
        {
            _gameController.OnClearReceived();
            return Clients.Group(Context.ToRoomName()).SendCoreAsync(nameof(Clear), Array.Empty<object>());
        }

        /// <summary>
        /// Request for room status
        /// </summary>
        /// <param name="username">User that requested the room status</param>
        /// <returns>Room status</returns>
        [HubMethodName(nameof(CheckRoomStatus))]
        public async Task CheckRoomStatus()
        {
            var roomName = Context.ToRoomName();
            var username = Context.ToUserName();
            var connectionId = Context.ConnectionId;
            var players = _connectionMonitor.GetByGroup(roomName).Select(s => new Player(s));
            var status = RoomStatus.Build(roomName, players, new Player(username));
            status.CurrentDraw = _gameController.GetCurrentDraw();

            await Clients.Client(connectionId).SendCoreAsync(nameof(OnRoomStatusChanged), new object[] { status }).ConfigureAwait(false);
        }

        /// <summary>
        /// Draw some points on canvas
        /// </summary>
        /// <param name="drawCommand">Draw command</param>
        /// <returns></returns>
        [HubMethodName(nameof(Draw))]
        public Task Draw(DrawCommand drawCommand)
        {
            _gameController.OnDrawReceived(drawCommand);
            return Clients.Group(Context.ToRoomName()).SendCoreAsync(nameof(Draw), new object[] { drawCommand });
        }

        /// <summary>
        /// Message on room chat
        /// </summary>
        /// <param name="username">user that have sent message</param>
        /// <param name="message">Message text</param>
        /// <returns></returns>
        [HubMethodName(nameof(Message))]
        public async Task Message(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            var result = _gameController.OnResponse(message);
            var roomName = Context.ToRoomName();
            var username = Context.ToUserName();

            //Envia mensagem de resposta à tentativa do usuário
            if (result is AllMatchResult)
            {
                await ResponseResult(username, "Você acertou!").ConfigureAwait(false);

                //Envia mensagem que o usuário acertou, para todo o chat
                var players = _connectionMonitor.GetByGroup(roomName).Select(s => new Player(s));
                var status = RoomStatus.Build(roomName, players, new Player(username));

                await OnRoomStatusChanged(status).ConfigureAwait(false);
                await Clients.Group(roomName).SendCoreAsync(nameof(ResponseResult), new object[] { $"{username} acertou!" }).ConfigureAwait(false);
                return;
            }
            else if (result is IsClosestResult)
            {
                await ResponseResult(username, $"{message} está perto!").ConfigureAwait(false);
            }

            //Envia mensagem de tentativa para todo o chat
            await Clients.Group(roomName).SendCoreAsync(nameof(Message), new object[] { username, message  }).ConfigureAwait(false);
        }

        /// <summary>
        /// Send response result
        /// </summary>
        /// <param name="username">User to send this response result</param>
        /// <param name="result">Result</param>
        /// <returns></returns>
        [HubMethodName(nameof(ResponseResult))]
        public Task ResponseResult(string username, string result)
        {
            var connectionId = _connectionMonitor.GetId(username);
            return Clients.Client(connectionId).SendCoreAsync(nameof(ResponseResult), new object[] { result });
        }

        /// <summary>
        /// Dispatch event when room status changed
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
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
                //TODO: logs
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
                //TODO: logs
            }
            finally
            {
                _gameController.OnPlayerDisconnected(Context.ConnectionId);
            }
        }

        #endregion
    }
}
