using Microsoft.AspNetCore.SignalR;
using SignalRSwaggerGen.Attributes;
using SignalRSwaggerGen.Enums;
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
    /// <summary>
    /// Game hub connection
    /// </summary>
    [SignalRHub("XarticHub", AutoDiscover.MethodsAndParams)]
    public class XarticHub : SignalHub
    {
        #region Fields

        private static readonly IGameController _gameController = new GameController();

        private readonly IConnectionMonitor<XarticHub> _connectionMonitor;

        #endregion

        #region Constructors

        /// <summary>
        /// Builds a new instance from Xartic Hub
        /// </summary>
        /// <param name="connectionMonitor">Instance for monitoring active connections</param>
        public XarticHub(IConnectionMonitor<XarticHub> connectionMonitor) : base()
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
        [SignalRMethod(nameof(StartGame), summary: "Initialize the game room")]
        public Task StartGame()
        {
            var roomName = Context.ToRoomName();
            var currentWord = _gameController.StartGame(roomName, Context.ConnectionId);

            return Clients.Client(Context.ConnectionId).SendCoreAsync("OnGameWordChanged", new object[] { currentWord });
        }

        /// <summary>
        /// Clear the draw on canvas
        /// </summary>
        /// <returns></returns>
        [HubMethodName(nameof(Clear))]
        [SignalRMethod(nameof(Clear), summary: "Clear the current draw for all players")]
        public Task Clear()
        {
            _gameController.OnClearReceived();
            return Clients.Group(Context.ToRoomName()).SendCoreAsync(nameof(Clear), Array.Empty<object>());
        }

        /// <summary>
        /// Request for room status
        /// </summary>
        /// <returns>Room status</returns>
        [HubMethodName(nameof(CheckRoomStatus))]
        [SignalRMethod(nameof(CheckRoomStatus), summary: "Request for room current status")]
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
        [SignalRMethod(nameof(Draw), summary: "Dispatch current draw information for all users")]
        public Task Draw([SignalRParam("Current draw action", typeof(DrawCommand))] DrawCommand drawCommand)
        {
            _gameController.OnDrawReceived(drawCommand);
            return Clients.Group(Context.ToRoomName()).SendCoreAsync(nameof(Draw), new object[] { drawCommand });
        }

        /// <summary>
        /// Message on room chat
        /// </summary>
        /// <param name="message">Message text</param>
        /// <returns></returns>
        [HubMethodName(nameof(Message))]
        [SignalRMethod(nameof(Message), summary: "Sends message to room chat")]
        public async Task Message([SignalRParam("Message that will be displayied to chat or validated as game response", typeof(string))] string message)
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
        [SignalRMethod(nameof(ResponseResult), summary: "Send response result")]
        public Task ResponseResult([SignalRParam("User to send this response result", typeof(string))] string username, [SignalRParam("Current game response result", typeof(string))] string result)
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
        [SignalRMethod(nameof(OnRoomStatusChanged), summary: "Dispatch event when room status changed")]
        public Task OnRoomStatusChanged([SignalRParam("The current room status", typeof(RoomStatus))] RoomStatus status) =>
            Clients.Group(status.RoomName).SendCoreAsync(nameof(OnRoomStatusChanged), new object[] { status });

        #endregion

        #region Overrides

        /// <inheritdoc/>
        [SignalRHidden]
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

        /// <inheritdoc/>
        [SignalRHidden]
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
