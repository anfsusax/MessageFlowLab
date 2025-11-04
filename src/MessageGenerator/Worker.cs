using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using SupportEngine.Models;
 

namespace MessageGenerator;

/// <summary>
/// MessageGenerator worker: simula múltiplos usuários gerando perguntas
/// e publica mensagens na fila RabbitMQ. Cada mensagem usa um
/// CorrelationId para rastreamento.
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var rabbitHost = _configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        var rabbitPort = _configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
        var queueName = _configuration.GetValue<string>("RabbitMQ:Queue") ?? "questions";
            var username = _configuration.GetValue<string>("RabbitMQ:Username") ?? "user";
            var password = _configuration.GetValue<string>("RabbitMQ:Password") ?? "password";

            var factory = new ConnectionFactory { 
                HostName = rabbitHost, 
                Port = rabbitPort,
                UserName = username,
                Password = password
            };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Ensure the queue exists (durable=false for demo; in prod consider durable/persistent)
        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var rnd = new Random();

        // Sample questions to simulate random users
        var samples = new[] {
            "How do I configure dependency injection in .NET?",
            "What's the difference between Kafka and RabbitMQ?",
            "How to implement retry with Polly?",
            "How to structure messages with CorrelationId?",
            "Best practices for logging in distributed systems?"
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            var msg = new QuestionMessage
            {
                Question = samples[rnd.Next(samples.Length)],
                User = $"user-{rnd.Next(1, 20)}"
            };

            var json = JsonSerializer.Serialize(msg);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.MessageId = msg.CorrelationId;
            props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            props.Persistent = false;

            channel.BasicPublish(exchange: "",
                                 routingKey: queueName,
                                 basicProperties: props,
                                 body: body);

            _logger.LogInformation("Published question {CorrelationId} user={User}: {Question}", msg.CorrelationId, msg.User, msg.Question);

            // Wait a bit before publishing next message
            await Task.Delay(1000, stoppingToken);
        }
    }
}
