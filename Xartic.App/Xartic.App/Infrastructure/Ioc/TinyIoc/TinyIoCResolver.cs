using System;
using TinyIoC;
using Xartic.App.Abstractions;

namespace Xartic.App.Infrastructure.Ioc.TinyIoc
{
    public sealed class TinyIoCResolver : IServiceResolver
    {
        private readonly TinyIoCContainer _container;

        public TinyIoCResolver(TinyIoCContainer container)
        {
            _container = container;
        }

        public TInterface Resolve<TInterface>() =>
            (TInterface)_container.Resolve(typeof(TInterface));

        public object Resolve(Type typeToResolve) => 
            _container.Resolve(typeToResolve);

        public void Dispose() => _container.Dispose();
    }
}
