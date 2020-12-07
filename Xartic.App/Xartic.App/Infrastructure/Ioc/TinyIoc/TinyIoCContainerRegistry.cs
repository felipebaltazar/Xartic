using System;
using TinyIoC;
using Xamarin.Forms.Internals;
using Xartic.App.Abstractions;

namespace Xartic.App.Infrastructure.Ioc.TinyIoc
{
    public sealed class TinyIoCContainerRegistry : IContainerRegistry
    {
        private readonly TinyIoCContainer _container;
        private readonly IServiceResolver _resolver;

        [Preserve]
        public TinyIoCContainerRegistry()
        {
            _container = new TinyIoCContainer();
            _resolver = new TinyIoCResolver(_container);

            _container.Register<IContainerRegistry>(this);
            _container.Register<IServiceResolver>(_resolver);
        }

        public IContainerRegistry RegisterScoped<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _container.Register<TInterface, TImplementation>()
                      .AsMultiInstance();

            return this;
        }

        public IContainerRegistry RegisterScoped<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _container.Register(typeof(TInterface), (c, p) => factory())
                      .AsMultiInstance();

            return this;
        }

        public IContainerRegistry RegisterScoped<TInterface, TImplementation>(Func<IServiceResolver, TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _container.Register(typeof(TInterface), (c, p) => factory(_resolver))
                      .AsMultiInstance();

            return this;
        }

        public IContainerRegistry RegisterScoped<TImplementation>() where TImplementation : class
        {
            _container.Register(typeof(TImplementation))
                      .AsMultiInstance();

            return this;
        }

        public IContainerRegistry RegisterSingleton<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _container.Register<TInterface, TImplementation>()
                      .AsSingleton();

            return this;
        }

        public IContainerRegistry RegisterSingleton<TInterface, TImplementation>(Func<TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _container.Register(typeof(TInterface), (c, p) => factory())
                      .AsSingleton();

            return this;
        }

        public IContainerRegistry RegisterSingleton<TInterface, TImplementation>(Func<IServiceResolver, TImplementation> factory)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            _container.Register(typeof(TInterface), (c, p) => factory(_resolver))
                      .AsSingleton();

            return this;
        }

        public IContainerRegistry RegisterSingleton<TImplementation>() where TImplementation : class
        {
            _container.Register(typeof(TImplementation))
                      .AsSingleton();

            return this;
        }

        public IServiceResolver GetResolver() => _resolver;

    }
}
