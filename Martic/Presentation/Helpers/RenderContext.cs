using SkiaSharp;

namespace Martic.Presentation.Helpers
{
    public struct RenderContext
    {
        public SKImageInfo Info { get; }

        public SKCanvas Canvas { get; }

        public SKPaint Paint { get; }

        public RenderContext(SKImageInfo info, SKCanvas canvas, SKPaint paint)
        {
            Info = info;
            Canvas = canvas;
            Paint = paint;
        }
    }
}
