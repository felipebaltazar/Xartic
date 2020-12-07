using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xartic.App.Abstractions;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Domain.Models;
using Xartic.App.Infrastructure.Extensions;
using Xartic.App.Infrastructure.Helpers;
using Xartic.Core;
using XFColor = Xamarin.Forms.Color;

namespace Xartic.App.Presentation.ViewModels
{
    public sealed class GameRoomPageViewModel : BaseViewModel, INavigatedAware
    {
        private readonly IXarticService _xarticService;
        private readonly ILogger _logger;

        private CancellationTokenSource cancellationTokenSource;
        private DrawCommand lastCommand;

        private string username;
        private string roomName;
        private string message;
        private bool hasWinner;

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

        private async Task OpenSignalRConnectionAsync()
        {
            await _xarticService.OpenConnectionAsync(Username, RoomName).ConfigureAwait(false);

            _xarticService.SubscribeFor<RoomStatus>("OnRoomStatusChanged", OnRoomStatusChanged);
            _xarticService.SubscribeFor<string>("ResponseResult", OnResponseResultReceived);
            _xarticService.SubscribeFor<string, string>("Message", OnChatMessageReceived);
            _xarticService.SubscribeFor<DrawCommand>("Draw", OnDrawCommandReceived);
            _xarticService.SubscribeFor("Clear", OnClearCanvasReceived);
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
        }

        private void OnClearCanvasReceived(string username)
        {
            Renderers.Clear();
            lastCommand = null;
        }

        private void OnDrawCommandReceived(DrawCommand command)
        {
            if(command.IsMouseDown && lastCommand != null)
                Renderers.Add(command.ToRenderer(lastCommand));

            lastCommand = command;
        }
    }
}
