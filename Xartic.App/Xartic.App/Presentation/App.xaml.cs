using Microsoft.Extensions.Logging;
using System.Threading;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Infrastructure.Extensions;
using Xartic.App.Presentation.Extensions;

namespace Xartic.App.Presentation
{
    [Preserve(AllMembers = true)]
    public partial class App : Application
    {
        private readonly ILogger _logger;
        private readonly INavigationService _navigationService;

        private CancellationTokenSource cancellationTokenSource;

        public App(
            ILogger logger,
            INavigationService navigationService)
        {
            _logger = logger;
            _navigationService = navigationService;

            InitializeComponent();
            _navigationService.NavigateTo("MainPage").SafeFireAndForget();
        }

        protected override void OnStart() =>
            _logger.LogInformation("Application started");

        protected override void OnSleep()
        {
            MainPage?.GetCurrentPage()
                    ?.InvokeViewAndViewModelActionAsync<IApplicationLifeCycleAware>(v => v.OnSleepAsync(SetupToken()))
                    ?.SafeFireAndForget();

            _logger.LogInformation("Application sleeped");
        }

        protected override void OnResume()
        {
            MainPage?.GetCurrentPage()
                    ?.InvokeViewAndViewModelActionAsync<IApplicationLifeCycleAware>(v => v.OnResumeAsync(SetupToken()))
                    ?.SafeFireAndForget();

            _logger.LogInformation("Application resumed");
        }

        private CancellationToken SetupToken()
        {
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = new CancellationTokenSource();

            return cancellationTokenSource.Token;
        }
    }
}
