using Microsoft.AspNetCore.Http.Connections.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using Xartic.App.Abstractions;
using Xartic.App.Abstractions.Services;
using Xartic.App.Infrastructure.Helpers.Settings;

namespace Xartic.App.Services
{
    public sealed class XarticService : IXarticService, ILoggerProvider
    {
        #region Fields

        private readonly object _connectionLock;

        private readonly ILogger _logger;
        private readonly ISettingsService _settingsService;
        private readonly ICollection<IDisposable> _eventsReference;

        private HubConnection _hubConnection;
        private string username;
        private string roomName;

        #endregion

        #region Constructors

        [Preserve]
        public XarticService(ILogger logger, ISettingsService settingsService)
        {
            _logger = logger;
            _settingsService = settingsService;
            _eventsReference = new List<IDisposable>(5);
            _connectionLock = new object();
        }

        #endregion

        #region IXarticService

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

                if (_hubConnection is null)
                {
                    var apiSettings = _settingsService.GetValue<ApiSettings>();
                    _hubConnection = new HubConnectionBuilder()
                                    .ConfigureLogging(OnLogReceived)
                                    .WithUrl($"{apiSettings.BaseUrl}/Xartic?username={username}&roomName={roomName}", BuildOptions)
                                    .WithAutomaticReconnect()
                                    .Build();

                    _hubConnection.Closed += OnDisconnected;
                }

                await _hubConnection.StartAsync().ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening signal connection");
            }
        }

        private void OnLogReceived(ILoggingBuilder loggingBuilder)
        {
            var logLevel = Debugger.IsAttached ? LogLevel.Trace : LogLevel.Error;
            loggingBuilder.SetMinimumLevel(logLevel);
            loggingBuilder.AddProvider(this);
        }

        public async Task CloseConnectionAsync()
        {
            try
            {
                if (_hubConnection != null)
                {
                    _hubConnection.Closed -= OnDisconnected;
                    await _hubConnection.StopAsync().ConfigureAwait(false);
                }


                foreach (var disposer in _eventsReference)
                {
                    disposer.Dispose();
                }

                _eventsReference?.Clear();
                if (_hubConnection != null)
                    await _hubConnection.DisposeAsync().ConfigureAwait(false);

                _hubConnection = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing signal connection");
            }
        }

        public async Task SendMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;

            await _hubConnection.SendAsync("Message", message).ConfigureAwait(false);
        }

        public async Task CheckRoomStatusAsync() =>
            await _hubConnection.SendAsync("CheckRoomStatus").ConfigureAwait(false);

        public void SubscribeFor(string commandName, Action action)
        {
            var disposer = _hubConnection.On(commandName, action);
            _eventsReference.Add(disposer);
        }

        public void SubscribeFor<T>(string eventName, Action<T> actionResult)
        {
            var disposer = _hubConnection.On<T>(eventName, actionResult);
            _eventsReference.Add(disposer);
        }

        public void SubscribeFor<T1, T2>(string eventName, Action<T1, T2> actionResult)
        {
            var disposer = _hubConnection.On<T1, T2>(eventName, actionResult);
            _eventsReference.Add(disposer);
        }

        #endregion


        #region ILoggerProvider

        public ILogger CreateLogger(string categoryName) => _logger;

        public void Dispose() { /*NoDispose*/ }

        #endregion

        #region Private Events

        private static void BuildOptions(HttpConnectionOptions options)
        {
            options.WebSocketConfiguration = socket =>
            {
                socket.RemoteCertificateValidationCallback = CertificateValidationCallback;
            };

            options.HttpMessageHandlerFactory = handler =>
            {
                return new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = CertificateValidationCallback
                };
            };
        }

        private async Task OnDisconnected(Exception exception)
        {
            if (exception is null)
                return;

            await OpenConnectionAsync(username, roomName).ConfigureAwait(false);
        }

        private static bool CertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        #endregion
    }
}
