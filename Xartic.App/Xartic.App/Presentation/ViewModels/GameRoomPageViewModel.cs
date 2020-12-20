using Xartic.App.Abstractions;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Domain.Models;
using Xartic.App.Infrastructure.Extensions;
using Xartic.App.Infrastructure.Helpers;
using Xamarin.Forms.Internals;
using XFColor = Xamarin.Forms.Color;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Xamarin.Forms;
using Xartic.Core;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace Xartic.App.Presentation.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class GameRoomPageViewModel : BaseViewModel, INavigatedAware, IDisposable, IApplicationLifeCycleAware
    {
        #region Fields

        private readonly IXarticService _xarticService;
        private readonly ILogger _logger;

        private CancellationTokenSource cancellationTokenSource;
        private DrawCommand lastCommand;

        private string username;
        private string roomName;
        private string message;
        private bool hasWinner;

        #endregion

        #region Properties

        public ObservableRangeCollection<Player> Players { get; }

        public ObservableRangeCollection<IRenderer> Renderers { get; }

        public ObservableRangeCollection<ChatMessage> Messages { get; }

        public ICommand MessageSendCommand { get; }

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        public string RoomName
        {
            get => roomName;
            set => SetProperty(ref roomName, value);
        }

        public bool HasWinner
        {
            get => hasWinner;
            set => SetProperty(ref hasWinner, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        #endregion

        #region Constructors

        public GameRoomPageViewModel(
            IXarticService xarticService,
            ILogger logger)
        {
            _logger = logger;
            _xarticService = xarticService;

            Players = new ObservableRangeCollection<Player>();
            Renderers = new ObservableRangeCollection<IRenderer>();
            Messages = new ObservableRangeCollection<ChatMessage>();
            MessageSendCommand = new Command(MessageSendCommandExecute);
        }

        #endregion

        #region INavigatedAware

        public async Task OnNavigatedAsync(IDictionary<string, StringValues> parameters)
        {
            try
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();
                var token = cancellationTokenSource.Token;

                await ExecuteBusyTask(OpenSignalRConnectionAsync, token).ConfigureAwait(false);

                await _xarticService.CheckRoomStatusAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cant start Xartic Service");
            }
        }

        #endregion

        #region IApplicationLifeCycleAware

        public async Task OnSleepAsync(CancellationToken token)
        {
            try
            {
                await ExecuteBusyTask(_xarticService.CloseConnectionAsync, token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation($"{nameof(OnSleepAsync)} task canceled");
            }
        }

        public async Task OnResumeAsync(CancellationToken token)
        {
            try
            {
                await ExecuteBusyTask(OpenSignalRConnectionAsync, token).ConfigureAwait(false);
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation($"{nameof(OnResumeAsync)} task canceled");
            }
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            DisposeAsync().SafeFireAndForget();

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        private Task DisposeAsync() =>
            _xarticService.CloseConnectionAsync();

        #endregion

        #region Private Methods

        private async Task OpenSignalRConnectionAsync()
        {
            await _xarticService.OpenConnectionAsync(Username, RoomName).ConfigureAwait(false);

            if (_xarticService.IsConnected)
            {
                _xarticService.SubscribeFor<RoomStatus>("OnRoomStatusChanged", OnRoomStatusChanged);
                _xarticService.SubscribeFor<string>("ResponseResult", OnResponseResultReceived);
                _xarticService.SubscribeFor<string, string>("Message", OnChatMessageReceived);
                _xarticService.SubscribeFor<DrawCommand>("Draw", OnDrawCommandReceived);
                _xarticService.SubscribeFor("Clear", OnClearCanvasReceived);
            }
        }

        private void MessageSendCommandExecute()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;

            ExecuteBusyTask(SendMessageAsync, CancellationToken.None).SafeFireAndForget();
        }

        private async Task SendMessageAsync()
        {
            await _xarticService.SendMessage(Message).ConfigureAwait(false);
            Message = string.Empty;
        }

        private void OnChatMessageReceived(string username, string message)
        {
            var newMessage = new ChatMessage(username, message, XFColor.Gray);
            Messages.Add(newMessage);
        }

        private void OnResponseResultReceived(string responseResult)
        {
            var won = responseResult.IndexOf("acertou", StringComparison.OrdinalIgnoreCase) >= 0;
            var newServerMessage =
                new ChatMessage(string.Empty, responseResult, won ? XFColor.LimeGreen : XFColor.DarkGray);

            Messages.Add(newServerMessage);
        }

        private void OnRoomStatusChanged(RoomStatus status)
        {
            Players.Clear();
            Players.AddRange(status.Players);

            HasWinner = status.HasWinner();

            if (!status.CurrentDraw.IsNullOrEmpty())
            {
                foreach (var command in status.CurrentDraw)
                    OnDrawCommandReceived(command);
            }
        }

        private void OnClearCanvasReceived()
        {
            Renderers.Clear();
            lastCommand = null;
        }

        private void OnDrawCommandReceived(DrawCommand command)
        {
            if (command.IsMouseDown && lastCommand != null)
                Renderers.Add(command.ToRenderer(lastCommand));

            lastCommand = command;
        }

        #endregion
    }
}
