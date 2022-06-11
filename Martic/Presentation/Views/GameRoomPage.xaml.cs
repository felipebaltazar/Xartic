using Martic.Abstractions;
using Martic.Infrastructure.Extensions;
using Martic.Presentation.Helpers;
using Martic.Presentation.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Martic.Presentation.Views;

public partial class GameRoomPage : ContentPage
{
    #region Bindable Properties

    public readonly BindableProperty RenderersProperty = BindableProperty.Create(
        propertyName: nameof(Renderers),
        returnType: typeof(ICollection<IRenderer>),
        declaringType: typeof(GameRoomPage),
        defaultValue: null,
        defaultBindingMode: BindingMode.OneTime);

    #endregion

    #region Fields

    private int lastIndex = -1;

    #endregion

    #region Properties

    public ICollection<IRenderer> Renderers
    {
        get => (ICollection<IRenderer>)GetValue(RenderersProperty);
        set => SetValue(RenderersProperty, value);
    }

    #endregion

    #region Constructors

    public GameRoomPage(GameRoomPageViewModel vm)
    {
        InitializeComponent();
        this.SetBinding(RenderersProperty, nameof(Renderers), BindingMode.OneTime);

        BindingContext = vm;
    }

    #endregion

    #region Overrides

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

    #endregion

    #region Private Methods

    private void OnRenderersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action == NotifyCollectionChangedAction.Reset)
            lastIndex = -1;

        canvasView.InvalidateSurface();
    }

    private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
    {
        canvasView.WidthRequest = canvasView.Height;
        var surface = e.Surface;
        var canvas = surface.Canvas;

        if (Renderers.IsNullOrEmpty())
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
    #endregion
}

