using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace TodoApi.Logging;

public class RabbitMQLogger : ILogger, IDisposable
{
    private readonly string _name;
    private readonly RabbitMQLoggerConfiguration _config;
    private IConnection _connection;
    private readonly IChannel _channel;
    private BasicProperties _properties;

    public RabbitMQLogger(string name, RabbitMQLoggerConfiguration config)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        _config = config ?? throw new ArgumentNullException(nameof(config));

        ValidateConfiguration(_config);

        _properties = new BasicProperties
        {
            Persistent = _config.Durable,
            ContentType = "application/json"
        };

    
    }

    private static void ValidateConfiguration(RabbitMQLoggerConfiguration config)
    {
        if (string.IsNullOrEmpty(config.HostName))
            throw new ArgumentException("RabbitMQ HostName is required", nameof(config));
        if (config.Port <= 0)
            throw new ArgumentException("RabbitMQ Port must be greater than 0", nameof(config));
        if (string.IsNullOrEmpty(config.UserName))
            throw new ArgumentException("RabbitMQ UserName is required", nameof(config));
        if (string.IsNullOrEmpty(config.Password))
            throw new ArgumentException("RabbitMQ Password is required", nameof(config));
        if (string.IsNullOrEmpty(config.Exchange))
            throw new ArgumentException("RabbitMQ Exchange is required", nameof(config));
        if (string.IsNullOrEmpty(config.ExchangeType))
            throw new ArgumentException("RabbitMQ ExchangeType is required", nameof(config));
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        try
        {
            var logEntry = new
            {
                Timestamp = DateTime.UtcNow,
                LogLevel = logLevel.ToString(),
                Category = _name,
                EventId = eventId.Id,
                EventName = eventId.Name,
                Message = formatter(state, exception),
                Exception = exception?.ToString()
            };

        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error publishing log message to RabbitMQ: {ex.Message}");
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.CloseAsync().GetAwaiter().GetResult();
            _channel?.DisposeAsync().GetAwaiter().GetResult();
            _connection?.CloseAsync().GetAwaiter().GetResult();
            _connection?.DisposeAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error disposing RabbitMQ logger: {ex.Message}");
        }
        GC.SuppressFinalize(this);
    }
}