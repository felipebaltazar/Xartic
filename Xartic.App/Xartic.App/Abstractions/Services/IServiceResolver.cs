using System;

namespace Xartic.App.Abstractions
{
    public interface IServiceResolver : IDisposable
    {
        TInterface Resolve<TInterface>();

        object Resolve(Type typeToResolve);
    }
}
