using Microsoft.Extensions.Logging;

namespace InstaPoker.Client.Subsystems;

public sealed class NullLogger : ILogger{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        // empty
    }
    public bool IsEnabled(LogLevel logLevel) {
        return false;
    }
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return null;
    }
}

public sealed class NullLogger<T> : ILogger<T> {
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter) {
        // empty
    }

    public bool IsEnabled(LogLevel logLevel) {
        return false;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull {
        return null;
    }
}