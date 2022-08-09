using AsyncAwaitBestPractices;
using Microsoft.Extensions.Logging;
using System.Windows.Input;

namespace Martic.Presentation.ViewModels
{
    public sealed class MainPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly ILogger _logger;

        private CancellationTokenSource cancellationTokenSource;
        private string username;

        #endregion

        #region Properties

        public ICommand StartGameCommand =>
            new Command(StartGameCommandExecute);

        public string Username
        {
            get => username;
            set => SetProperty(ref username, value);
        }

        #endregion

        #region Constructors

        public MainPageViewModel(
            ILogger logger)
        {
            _logger = logger;
        }

        #endregion

        #region Private Methods

        private void StartGameCommandExecute(object _)
        {
            if (string.IsNullOrWhiteSpace(Username))
                return;

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            ExecuteBusyTask(NavigateToGameRoom, token).SafeFireAndForget();
        }

        private Task NavigateToGameRoom()
        {
            _logger.LogInformation("Starting game session");
            return Shell.Current.GoToAsync($"GameRoomPage?Username={Username}&RoomName=Animais");
        }

        #endregion
    }
}
