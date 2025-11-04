using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using KafkaWorker.Data;
using SupportEngine.Models;
using Serilog;
using System.Text.Json;

namespace KafkaWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IConfiguration _configuration;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var kafkaBootstrap = _configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
        var topic = _configuration.GetValue<string>("Kafka:Topic") ?? "processed-questions";
        var groupId = _configuration.GetValue<string>("Kafka:GroupId") ?? "kafkaworker-group";

        var config = new ConsumerConfig
        {
            BootstrapServers = kafkaBootstrap,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        // ensure DB is created
        using (var scope = _serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        using var consumer = new ConsumerBuilder<string, string>(config).Build();
        consumer.Subscribe(topic);

        Log.Information("KafkaWorker subscribed to topic {Topic}", topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var cr = consumer.Consume(stoppingToken);
                    if (cr == null) continue;

                    Log.Information("Consumed message from Kafka topic {Topic} partition {Partition} offset {Offset}", cr.Topic, cr.Partition, cr.Offset);

                    // Try parse payload as QuestionMessage
                    QuestionMessage? qm = null;
                    try
                    {
                        qm = JsonSerializer.Deserialize<QuestionMessage>(cr.Message.Value);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Failed to deserialize message payload, storing raw");
                    }

                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var logEntry = new MessageLog
                    {
                        CorrelationId = qm?.CorrelationId ?? cr.Message.Key ?? Guid.NewGuid().ToString(),
                        User = qm?.User,
                        Question = qm?.Question ?? cr.Message.Value,
                        CreatedAt = qm?.CreatedAt ?? DateTime.UtcNow,
                        ReceivedAt = DateTime.UtcNow,
                        Topic = cr.Topic,
                        Partition = cr.Partition.Value,
                        Offset = cr.Offset.Value,
                        RawPayload = cr.Message.Value ?? string.Empty,
                        Status = "Processed"
                    };

                    db.MessageLogs.Add(logEntry);
                    await db.SaveChangesAsync(stoppingToken);

                    consumer.Commit(cr);
                }
                catch (OperationCanceledException) { break; }
                catch (ConsumeException cex)
                {
                    Log.Error(cex, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while consuming and persisting message");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }
}
