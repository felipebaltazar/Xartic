using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Infrastructure.Extensions;

namespace Xartic.App.Presentation.ViewModels
{
    public sealed class MainPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly INavigationService _navigationService;

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

        [Preserve]
        public MainPageViewModel(
            ILogger logger,
            INavigationService navigationService)
        {
            _logger = logger;
            _navigationService = navigationService;
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
            return _navigationService.NavigateTo($"/GameRoomPage?Username={Username}&RoomName=Animais");
        }

        #endregion
    }
}
