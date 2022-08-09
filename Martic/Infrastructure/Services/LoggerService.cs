using AsyncAwaitBestPractices;
using Martic.Abstractions;
using Martic.Abstractions.Services;
using Martic.Infrastructure.Helpers.Settings;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Martic.Infrastructure.Services
{
    public sealed class LoggerService : ILogger
    {
        #region Fields

        private readonly LogLevel _currentLevel;

        #endregion

        #region Constructors

        public LoggerService(ILazyDependency<ISettingsService> settingsService)
        {
            if (!Debugger.IsAttached)
            {
                _currentLevel = LogLevel.Error;

                var appcenterSettings = settingsService.Value.GetValue<AppCenterSettings>();
                var androidKey = appcenterSettings.AndroidKey;
                var iosKey = appcenterSettings.IosKey;

                AppCenter.Start(
                    $"android={androidKey};" +
                    $"ios={iosKey}",
                  typeof(Analytics), typeof(Crashes));
            }
            else
            {
                _currentLevel = LogLevel.Debug;
            }

            SafeFireAndForgetExtensions.SetDefaultExceptionHandling(DefaultExceptionHandler);
        }

        #endregion

        #region ILogger

        public IDisposable BeginScope<TState>(TState state) =>
            new Disposer(state);

        public bool IsEnabled(LogLevel logLevel) =>
            logLevel >= _currentLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < _currentLevel)
                return;

            var message = formatter?.Invoke(state, exception) ?? exception?.Message ?? state.ToString();
            var logMessage = $"[{logLevel}] Event:{eventId.Name} | Message: {message}";

            if (Debugger.IsAttached)
            {
                Console.WriteLine(logMessage);
            }
            else
            {
                if (_currentLevel > LogLevel.Warning && exception != null)
                    Crashes.TrackError(exception);
                else
                    Analytics.TrackEvent(logMessage);
            }
        }

        #endregion

        #region Private Methods

        private void DefaultExceptionHandler(Exception obj) =>
            Log(LogLevel.Critical, new EventId(12345, "SafeFireForget exception"), obj, obj, (s, ex) => ex.Message);

        #endregion

        #region Help Classes

        public class Disposer : IDisposable
        {
            private readonly WeakReference<IDisposable> reference;

            public Disposer(object reference)
            {
                if (reference is IDisposable disposable)
                    this.reference = new WeakReference<IDisposable>(disposable);
            }

            public void Dispose()
            {
                if (reference is null)
                    return;

                if (reference.TryGetTarget(out var disposable))
                    disposable.Dispose();
            }
        }

        #endregion
    }
}
