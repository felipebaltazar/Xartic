using AsyncAwaitBestPractices;
using Martic.Abstractions;
using Martic.Presentation.Extensions;
using Martic.Presentation.Views;
using Microsoft.Extensions.Logging;

namespace Martic;

public partial class App : Application
{
    private readonly ILogger _logger;
    private CancellationTokenSource cancellationTokenSource;

    public App(ILogger logger)
	{
        _logger = logger;
        InitializeComponent();

		MainPage = new AppShell();

        Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
        Routing.RegisterRoute(nameof(GameRoomPage), typeof(GameRoomPage));
    }

    protected override void OnStart() =>
            _logger.LogInformation("Application started");

    protected override void OnSleep()
    {
        MainPage?.GetCurrentPage()
                ?.InvokeViewAndViewModelActionAsync<IApplicationLifeCycleAware>(v => v.OnSleepAsync(SetupToken()))
                ?.SafeFireAndForget();

        _logger.LogInformation("Application sleeped");
    }

    protected override void OnResume()
    {
        MainPage?.GetCurrentPage()
                ?.InvokeViewAndViewModelActionAsync<IApplicationLifeCycleAware>(v => v.OnResumeAsync(SetupToken()))
                ?.SafeFireAndForget();

        _logger.LogInformation("Application resumed");
    }

    private CancellationToken SetupToken()
    {
        cancellationTokenSource?.Cancel();
        cancellationTokenSource?.Dispose();
        cancellationTokenSource = new CancellationTokenSource();

        return cancellationTokenSource.Token;
    }
}
