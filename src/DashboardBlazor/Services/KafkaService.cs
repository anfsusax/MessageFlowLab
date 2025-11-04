using Confluent.Kafka.Admin;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace DashboardBlazor.Services
{
    public record KafkaTopicInfo(string Name, int PartitionCount);

    public class KafkaService
    {
        private readonly string _bootstrap;

        public KafkaService(IConfiguration configuration)
        {
            _bootstrap = configuration.GetValue<string>("Kafka:BootstrapServers") ?? "localhost:9092";
        }

        public IEnumerable<KafkaTopicInfo> GetTopics()
        {
            try
            {
                var conf = new AdminClientConfig { BootstrapServers = _bootstrap };
                using var admin = new AdminClientBuilder(conf).Build();
                var meta = admin.GetMetadata(TimeSpan.FromSeconds(5));
                return meta.Topics.Select(t => new KafkaTopicInfo(t.Topic, t.Partitions.Count)).ToList();
            }
            catch
            {
                return Enumerable.Empty<KafkaTopicInfo>();
            }
        }
    }
}
