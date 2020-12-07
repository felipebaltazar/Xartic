using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xartic.App.Abstractions;
using Xartic.Core;

namespace Xartic.App.Services
{
    public sealed class XarticService : IXarticService
    {
        private const int MAXATTEMPTS = 3;

        private readonly ILogger _logger;
        private readonly ICollection<IDisposable> _eventsReference;

        private HubConnection _hubConnection;
        private string username;
        private string roomName;
        private int attempt = 0;

        public XarticService(ILogger logger)
        {
            _logger = logger;
            _eventsReference = new List<IDisposable>(3);
        }

        public bool IsConnected =>
            _hubConnection?.State == HubConnectionState.Connected;

        public async Task OpenConnectionAsync(string username, string roomName)
        {
            try
            {
                this.username = username;
                this.roomName = roomName;

                if (IsConnected)
                    return;

                if(_hubConnection is null)
                {
                    _hubConnection = new HubConnectionBuilder()
                                    .WithUrl($"https://api-xartic.azurewebsites.net/Xartic?username={username}&roomName={roomName}", BuildOptions)
                                    .WithAutomaticReconnect()
                                    .Build();

                    _hubConnection.Closed += OnDisconnected;
                }

                await _hubConnection.StartAsync().ConfigureAwait(false);
            }
            catch (Exception) when (attempt < MAXATTEMPTS)
            {
                _logger.LogInformation($"Retrying XarticHub connection... Attempt: {attempt}");

                attempt++;
                await Task.Yield();
                await Task.Delay(600).ConfigureAwait(false);
                await OpenConnectionAsync(username, roomName).ConfigureAwait(false);
            }
        }

        public async Task CloseConnectionAsync()
        {
            await _hubConnection.StopAsync().ConfigureAwait(false);
            _hubConnection.Closed -= OnDisconnected;

            foreach (var disposer in _eventsReference)
            {
                disposer.Dispose();
            }

            await _hubConnection.DisposeAsync().ConfigureAwait(false);
            _hubConnection = null;
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            await _hubConnection.SendAsync("Message", username, message).ConfigureAwait(false);
        }

        public async Task CheckRoomStatusAsync()
        {
            await _hubConnection.SendAsync("CheckRoomStatus", username).ConfigureAwait(false);
        }

        public void SubscribeFor<T>(string eventName, Action<T> actionResult)
        {
            var disposer = ParseEvent(eventName, actionResult);
            _eventsReference.Add(disposer);
        }

        public void SubscribeFor<T1, T2>(string eventName, Action<T1, T2> actionResult)
        {
            var disposer = _hubConnection.On<T1, T2>(eventName, actionResult);
            _eventsReference.Add(disposer);
        }

        private IDisposable ParseEvent<T>(string eventName, Action<T> actionResult)
        {
            if(actionResult is Action<DrawCommand> drawAction)
            {
                return _hubConnection.On<string, double, double, int, bool>(eventName, (h, x, y, r, md) => DrawCommandResolver(h, x, y, r, md, drawAction));
            }
            else if (actionResult is Action<RoomStatus> roomStatusResult)
            {
                return _hubConnection.On<RoomStatus>(eventName, roomStatusResult);
            }

            return default(IDisposable);
        }

        private static void DrawCommandResolver(string h, double x, double y, int r, bool mD, Action<DrawCommand> drawAction)
        {
            var command = new DrawCommand()
            {
                IsMouseDown = mD,
                Color = new Color()
                {
                    Hex = h
                },
                Position = new Vector2() { X = x, Y = y },
                Radius = r
            };

            drawAction.Invoke(command);
        }

        public void SubscribeFor(string commandName, Action<string> action)
        {
           var disposer = _hubConnection.On(commandName, action);
            _eventsReference.Add(disposer);
        }

        private static void BuildOptions(HttpConnectionOptions options)
        {
            options.HttpMessageHandlerFactory = handler =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, certificate, chain, sslPolicyErrors) => true
                };
            };
        }

        private async Task OnDisconnected(Exception exception)
        {
            if (exception is null)
                return;

            await OpenConnectionAsync(username, roomName).ConfigureAwait(false);
        }
    }
}
