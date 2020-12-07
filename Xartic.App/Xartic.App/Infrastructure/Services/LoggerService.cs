using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using Xartic.App.Infrastructure.Extensions;

namespace Xartic.App.Infrastructure.Services
{
    public sealed class LoggerService : ILogger
    {
        private readonly LogLevel _currentLevel;

        public LoggerService()
        {
            if (Debugger.IsAttached)
                _currentLevel = LogLevel.Information;
            else
                _currentLevel = LogLevel.Error;

            TaskExtensions.SetDefaultExceptionHandling(DefaultExceptionHandler);
        }

        public IDisposable BeginScope<TState>(TState state) => new Disposer(state);

        public bool IsEnabled(LogLevel logLevel) => logLevel >= _currentLevel;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (logLevel < _currentLevel)
                return;

            var message = formatter?.Invoke(state, exception) ?? exception?.Message ?? state.ToString();
            Console.WriteLine($"[{logLevel}] Event:{eventId.Name} | Message: {message}");
        }

        private void DefaultExceptionHandler(Exception obj)
        {
            Log(LogLevel.Critical, new EventId(12345, "SafeFireForget exception"), obj, obj, (s, ex) => ex.Message);
        }

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
    }
}
