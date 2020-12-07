using System;
using System.Collections.Generic;
using System.Text;

namespace Xartic.App.Abstractions
{
    public interface IPlatformInitializer
    {
        void RegisterTypes(IContainerRegistry containerRegistry);
    }
}
