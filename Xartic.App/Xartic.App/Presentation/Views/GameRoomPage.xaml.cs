
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xartic.App.Abstractions;
using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Presentation.Views
{
    public partial class GameRoomPage : ContentPage
    {
        public readonly BindableProperty RenderersProperty = BindableProperty.Create(
            propertyName: nameof(Renderers),
            returnType: typeof(ICollection<IRenderer>),
            declaringType: typeof(GameRoomPage),
            defaultValue: null,
            defaultBindingMode: BindingMode.OneTime);

        private int lastIndex = -1;

        public ICollection<IRenderer> Renderers
        {
            get => (ICollection<IRenderer>)GetValue(RenderersProperty);
            set => SetValue(RenderersProperty, value);
        }

        [Preserve]
        public GameRoomPage()
        {
            InitializeComponent();
            this.SetBinding(RenderersProperty, nameof(Renderers), BindingMode.OneTime);
        }

        protected override void OnPropertyChanging([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanging(propertyName);

            if (nameof(Renderers).Equals(propertyName, StringComparison.Ordinal))
            {
                if (Renderers is INotifyCollectionChanged observable)
                    observable.CollectionChanged -= OnRenderersCollectionChanged;
            }
        }

        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (nameof(Renderers).Equals(propertyName, StringComparison.Ordinal))
            {
                if (Renderers is INotifyCollectionChanged observable)
                    observable.CollectionChanged += OnRenderersCollectionChanged;

                canvasView.InvalidateSurface();
            }
        }

        private void OnRenderersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
                lastIndex = -1;

            canvasView.InvalidateSurface();
        }

        private void OnPaintSurface(object sender, SkiaSharp.Views.Forms.SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;

            if (!Renderers.Any())
            {
                canvas.Clear();
                return;
            }

            using (var paint = new SKPaint())
            {
                var rendererContext = new RenderContext(e.Info, canvas, paint);

                for (int i = lastIndex + 1; i < Renderers.Count; i++)
                {
                    var renderer = Renderers.ElementAtOrDefault(i);
                    if (renderer is null)
                        continue;

                    renderer.ProcessRenderer(rendererContext);
                }
            }
        }
    }
}