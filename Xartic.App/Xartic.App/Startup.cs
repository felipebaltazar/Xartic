using Microsoft.Extensions.Logging;
using Xamarin.Forms;
using Xartic.App.Abstractions;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Abstractions.Services;
using Xartic.App.Infrastructure.Helpers;
using Xartic.App.Infrastructure.Ioc;
using Xartic.App.Infrastructure.Ioc.TinyIoc;
using Xartic.App.Infrastructure.Services;
using Xartic.App.Presentation.ViewModels;
using Xartic.App.Presentation.Views;
using Xartic.App.Services;
using XarticApp = Xartic.App.Presentation.App;

namespace Xartic.App
{
    public sealed class Startup
    {
        #region Fields

        private readonly IPlatformInitializer _platformInitializer;
        private readonly IContainerRegistry _containerRegistry;

        #endregion

        #region Properties

        public static Startup Instance { get; private set; }

        #endregion

        #region Constructors

        public Startup(IPlatformInitializer platformInitializer)
        {
            _platformInitializer = platformInitializer;
            _containerRegistry = new TinyIoCContainerRegistry();

            InitializeInternal();
            Instance = this;
        }

        #endregion

        #region Public Methods

        public Application ResolveApplication()
        {
            return _containerRegistry.GetResolver()
                                     .Resolve<XarticApp>();
        }

        #endregion

        #region Private Methods

        private void InitializeInternal()
        {
            var resolver = _containerRegistry.GetResolver();

            RegisterRequiredTypes(_containerRegistry);
            _platformInitializer?.RegisterTypes(_containerRegistry);

            RegisterNavigation(resolver.Resolve<INavigationRegistry>());
        }

        private void RegisterNavigation(INavigationRegistry navigationRegistry)
        {
            navigationRegistry.RegisterForNavigation<MainPage, MainPageViewModel>()
                              .RegisterForNavigation<GameRoomPage, GameRoomPageViewModel>();
        }

        private void RegisterRequiredTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry
                .RegisterSingleton<IApplicationProvider, ApplicationProvider>()
                .RegisterSingleton<INavigationRegistry, NavigationRegistry>()
                .RegisterSingleton<INavigationService, NavigationService>()
                .RegisterSingleton<ISettingsService, SettingsService>()
                .RegisterSingleton<IXarticService, XarticService>()
                .RegisterSingleton<ILogger, LoggerService>()
                .RegisterSingleton<XarticApp>();
        }

        #endregion
    }
}
