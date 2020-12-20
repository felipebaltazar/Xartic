using System;

namespace Xartic.App.Infrastructure.Helpers
{
    public sealed class NoDispose : IDisposable
    {
        public void Dispose() { }
    }
}