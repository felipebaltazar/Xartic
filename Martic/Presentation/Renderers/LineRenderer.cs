using Martic.Abstractions;
using Martic.Infrastructure.Extensions;
using Martic.Presentation.Helpers;
using SkiaSharp;
using System;
using Xartic.Core;
using Color = Xartic.Core.Color;

namespace Martic.Presentation.Renderers
{
    public sealed class LineRenderer : IRenderer
    {
        #region Fields

        private const float DRAW_AREA_WIDTH = 400f;
        private const float DRAW_AREA_HEIGHT = 338f;

        #endregion

        #region Properties

        public Vector2 Start { get; }

        public Vector2 End { get; }

        public Color Color { get; }

        public float Radius { get; }

        #endregion

        #region Constructors

        public LineRenderer(Vector2 start, Vector2 end, Color color, float radius)
        {
            Start = start;
            End = end;
            Radius = radius;
            Color = color;
        }

        #endregion

        #region IRenderer

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

        #endregion
    }
}
