using SkiaSharp;
using System;
using Xartic.App.Abstractions;
using Xartic.App.Extensions;
using Xartic.App.Infrastructure.Helpers;
using Xartic.Core;

namespace Xartic.App.Presentation.Renderers
{
    public sealed class LineRenderer : IRenderer
    {
        private const float DRAW_AREA_WIDTH = 400f;
        private const float DRAW_AREA_HEIGHT = 338f;

        public Vector2 Start { get; }

        public Vector2 End { get; }

        public Color Color { get; }

        public float Radius { get; }

        public LineRenderer(Vector2 start, Vector2 end, Color color, float radius)
        {
            Start = start;
            End = end;
            Radius = radius;
            Color = color;
        }

        public void ProcessRenderer(RenderContext context)
        {
            var canvas = context.Canvas;
            var paint = context.Paint;

            var xRatio = context.Info.Width / DRAW_AREA_WIDTH;
            var yRatio = context.Info.Height / DRAW_AREA_HEIGHT;
            var scale = Math.Min(xRatio, yRatio);

            paint.Color = SKColor.Parse(Color.Hex);
            paint.StrokeWidth = Radius * 4f;
            paint.IsAntialias = true;
            paint.IsStroke = true;

            canvas.DrawLine(
                (Start.X * scale).ToSingle(),
                (Start.Y * scale).ToSingle(),
                (End.X * scale).ToSingle(),
                (End.Y * scale).ToSingle(),
                paint);
        }
    }
}
