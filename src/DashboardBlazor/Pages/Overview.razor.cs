using Microsoft.AspNetCore.Components;
using DashboardBlazor.Services;

namespace DashboardBlazor.Pages
{
    public partial class OverviewBase : ComponentBase
    {
        [Inject] public MessageLogService LogService { get; set; } = null!;
        [Inject] public RabbitMqService Rabbit { get; set; } = null!;
        [Inject] public KafkaService Kafka { get; set; } = null!;

        protected int TotalMessages { get; set; }
        protected int QueueCount { get; set; }
        protected int TopicCount { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Load();
        }

        protected async Task Load()
        {
            TotalMessages = await LogService.GetTotalAsync();
            var queues = await Rabbit.GetQueuesAsync();
            QueueCount = queues.Count();
            var topics = Kafka.GetTopics();
            TopicCount = topics.Count();
            StateHasChanged();
        }
    }
}
