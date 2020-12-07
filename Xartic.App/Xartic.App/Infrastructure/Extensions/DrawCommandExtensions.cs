using Xartic.App.Abstractions;
using Xartic.App.Presentation.Renderers;
using Xartic.Core;

namespace Xartic.App.Infrastructure.Extensions
{
    public static class DrawCommandExtensions
    {
        public static IRenderer ToRenderer(this DrawCommand command, DrawCommand lastCommand)
        {
            return new LineRenderer(
                lastCommand.Position,
                command.Position,
                command.Color,
                command.Radius);
        }
    }
}
