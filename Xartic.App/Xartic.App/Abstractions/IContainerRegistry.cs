using System;

namespace Xartic.App.Abstractions
{
    public interface IContainerRegistry
    {
        IContainerRegistry RegisterScoped<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        IContainerRegistry RegisterScoped<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface;

        IContainerRegistry RegisterScoped<TInterface, TImplementation>(Func<IServiceResolver, TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface;

        IContainerRegistry RegisterScoped<TViewModel>()
            where TViewModel : class;

        IContainerRegistry RegisterSingleton<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface;

        IContainerRegistry RegisterSingleton<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface;

        IContainerRegistry RegisterSingleton<TInterface, TImplementation>(Func<IServiceResolver, TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface;

        IContainerRegistry RegisterSingleton<TImplementation>()
            where TImplementation : class;

        IServiceResolver GetResolver();
    }
}
