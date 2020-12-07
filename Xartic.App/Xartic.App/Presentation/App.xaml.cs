using Microsoft.Extensions.Logging;
using Xamarin.Forms;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Infrastructure.Extensions;

namespace Xartic.App.Presentation
{
    public partial class App : Application
    {
        private readonly ILogger _logger;
        private readonly INavigationService _navigationService;

        public App(
            ILogger logger,
            INavigationService navigationService)
        {
            _logger = logger;
            _navigationService = navigationService;

            InitializeComponent();
            _navigationService.NavigateTo("MainPage").SafeFireAndForget();
        }

        protected override void OnStart()
        {
            _logger.LogInformation("Application started");
        }

        protected override void OnSleep()
        {
            _logger.LogInformation("Application sleeped");
        }

        protected override void OnResume()
        {
            _logger.LogInformation("Application resumed");
        }
    }
}
