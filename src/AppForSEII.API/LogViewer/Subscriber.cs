using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogViewer;

public class Subscriber
{
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IChannel? _channel;

    public Subscriber(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task StartAsync()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.TryParse(_configuration["RabbitMQ:Port"], out int port) ? port : 5672,
            UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
            Password = _configuration["RabbitMQ:Password"] ?? "guest"
        };

        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();

        string exchangeName = _configuration["RabbitMQ:Exchange"] ?? "logs";

        // 1. Declarar el exchange (debe coincidir con la API)
        await _channel.ExchangeDeclareAsync(exchangeName, ExchangeType.Fanout, durable: true, autoDelete: false);

        // 2. Crear una cola temporal exclusiva para este visor
        var tempQueue = await _channel.QueueDeclareAsync();
        string queueName = tempQueue.QueueName;

        // 3. Vincular la cola temporal al exchange
        await _channel.QueueBindAsync(queue: queueName, exchange: exchangeName, routingKey: string.Empty);

        // 4. Quedarse a la escucha e imprimir el log
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"\n[NUEVO LOG]\n{message}");
            await Task.Yield();
        };

        await _channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
    }
}