using Xartic.App.Infrastructure.Ioc;

namespace Xartic.App.Abstractions
{
    public interface INavigationRegistry
    {
        INavigationRegistry RegisterForNavigation<TView, TViewModel>(RegisterOptions? options = null)
            where TView : class
            where TViewModel : class;
    }
}
