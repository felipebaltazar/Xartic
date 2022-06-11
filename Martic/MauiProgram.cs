using Martic.Abstractions;
using Martic.Abstractions.Services;
using Martic.Infrastructure.Helpers;
using Martic.Infrastructure.Services;
using Martic.Presentation.ViewModels;
using Martic.Presentation.Views;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Martic;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Font Awesome 5 Free-Regular-400.otf", "FontAwesomeRegular");
                fonts.AddFont("Font Awesome 5 Free-Solid-900.otf", "FontAwesomeBold");
            });

        builder.Services.AddSingleton<ISettingsService, SettingsService>();
        builder.Services.AddSingleton<ILazyDependency<ISettingsService>, LazyDependency<ISettingsService>>();

        builder.Services.AddSingleton<IXarticService, XarticService>();
        builder.Services.AddSingleton<ILogger, LoggerService>();


        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<GameRoomPage>();

        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<GameRoomPageViewModel>();

        return builder.Build();
    }
}
