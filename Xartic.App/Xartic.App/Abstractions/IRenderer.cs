using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Abstractions
{
    public interface IRenderer
    {
        void ProcessRenderer(RenderContext context);
    }
}
