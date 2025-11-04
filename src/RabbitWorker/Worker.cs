using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SupportEngine.Models;
using Confluent.Kafka;

namespace RabbitWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Run the consumer loop on a background task so the method returns promptly.
        return Task.Run(() => ConsumerLoopAsync(stoppingToken), stoppingToken);
    }

    private async Task ConsumerLoopAsync(CancellationToken stoppingToken)
    {
        var rabbitHost = _configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost";
        var rabbitPort = _configuration.GetValue<int?>("RabbitMQ:Port") ?? 5672;
        var queueName = _configuration.GetValue<string>("RabbitMQ:Queue") ?? "questions";
        var dlqQueue = _configuration.GetValue<string>("RabbitMQ:DLQ") ?? "questions-dlq";
        var username = _configuration.GetValue<string>("RabbitMQ:Username") ?? "user";
        var password = _configuration.GetValue<string>("RabbitMQ:Password") ?? "password";

        var kafkaBootstrap = _configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
        var kafkaTopic = _configuration.GetValue<string>("Kafka:Topic") ?? "processed-questions";

        var factory = new ConnectionFactory { HostName = rabbitHost, Port = rabbitPort, UserName = username, Password = password };

        // Create RabbitMQ connection and channel once
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
        channel.QueueDeclare(queue: dlqQueue, durable: false, exclusive: false, autoDelete: false, arguments: null);

        // Kafka producer config
        var producerConfig = new ProducerConfig { BootstrapServers = kafkaBootstrap };
        using var producer = new ProducerBuilder<string, string>(producerConfig).Build();

        _logger.LogInformation("RabbitWorker is polling queue '{Queue}' and forwarding to Kafka '{Topic}'", queueName, kafkaTopic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = channel.BasicGet(queueName, autoAck: false);
                if (result == null)
                {
                    await Task.Delay(500, stoppingToken);
                    continue;
                }

                var body = result.Body.ToArray();
                var json = Encoding.UTF8.GetString(body);
                var props = result.BasicProperties;
                var correlationId = props?.MessageId ?? Guid.NewGuid().ToString();

                Serilog.Log.Information("[RabbitWorker] Polled message {CorrelationId} deliveryTag={Tag}", correlationId, result.DeliveryTag);

                // Retry logic
                const int maxAttempts = 3;
                int currentRetry = 0;
                try
                {
                    if (props?.Headers != null && props.Headers.TryGetValue("x-retry", out var headerVal) && headerVal is byte[] bbuf)
                    {
                        var s = Encoding.UTF8.GetString(bbuf);
                        int.TryParse(s, out currentRetry);
                    }
                }
                catch { }

                bool success = false;
                Exception? lastEx = null;

                for (int attempt = currentRetry + 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        var kheaders = new Confluent.Kafka.Headers();
                        kheaders.Add("correlation-id", Encoding.UTF8.GetBytes(correlationId));

                        var dr = await producer.ProduceAsync(kafkaTopic, new Message<string, string>
                        {
                            Key = correlationId,
                            Value = json,
                            Headers = kheaders
                        });

                        Serilog.Log.Information("Published to Kafka topic {Topic} partition {Partition} offset {Offset} correlationId={CorrelationId}", dr.Topic, dr.Partition, dr.Offset, correlationId);
                        success = true;
                        break;
                    }
                    catch (Exception ex)
                    {
                        lastEx = ex;
                        Serilog.Log.Warning(ex, "Attempt {Attempt} failed for message {CorrelationId}", attempt, correlationId);
                        await Task.Delay(TimeSpan.FromSeconds(1 * attempt), stoppingToken);
                    }
                }

                if (success)
                {
                    channel.BasicAck(result.DeliveryTag, multiple: false);
                }
                else
                {
                    Serilog.Log.Error(lastEx, "Message {CorrelationId} failed after retries; sending to DLQ", correlationId);
                    var dprops = channel.CreateBasicProperties();
                    dprops.MessageId = correlationId;
                    var headersDict = new System.Collections.Generic.Dictionary<string, object>();
                    headersDict["x-retry"] = Encoding.UTF8.GetBytes(maxAttempts.ToString());
                    dprops.Headers = headersDict;
                    channel.BasicPublish(exchange: "", routingKey: dlqQueue, basicProperties: dprops, body: body);
                    channel.BasicAck(result.DeliveryTag, multiple: false);
                }
            }
        }
        catch (OperationCanceledException) { }
    }
}
