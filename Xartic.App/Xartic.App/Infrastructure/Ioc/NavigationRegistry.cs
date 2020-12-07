using Xamarin.Forms.Internals;
using Xartic.App.Abstractions;
using Xartic.App.Infrastructure.MVVM;

namespace Xartic.App.Infrastructure.Ioc
{
    public sealed class NavigationRegistry : INavigationRegistry
    {
        private readonly IContainerRegistry _containerRegistry;

        [Preserve]
        public NavigationRegistry(IContainerRegistry containerRegistry)
        {
            _containerRegistry = containerRegistry;
        }

        public INavigationRegistry RegisterForNavigation<TView, TViewModel>(RegisterOptions? options = null)
            where TView : class
            where TViewModel : class
        {
            ViewModelLocator.MapViewModel<TView, TViewModel>();

            var registerOptions = options ?? new RegisterOptions();

            if(registerOptions.ViewModelAsSingleton)
                _containerRegistry.RegisterSingleton<TViewModel>();
            else
                _containerRegistry.RegisterScoped<TViewModel>();

            if (registerOptions.ViewAsSingleton)
                _containerRegistry.RegisterSingleton<TView>();
            else
                _containerRegistry.RegisterScoped<TView>();

            return this;
        }
    }

    public struct RegisterOptions
    {
        public bool ViewModelAsSingleton { get; }

        public bool ViewAsSingleton { get; }

    }
}
