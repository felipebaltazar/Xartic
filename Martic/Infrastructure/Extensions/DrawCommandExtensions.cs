using Martic.Abstractions;
using Martic.Presentation.Renderers;
using Xartic.Core;

namespace Martic.Infrastructure.Extensions
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
