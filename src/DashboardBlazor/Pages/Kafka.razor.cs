using Microsoft.AspNetCore.Components;
using DashboardBlazor.Services;

namespace DashboardBlazor.Pages
{
    public class KafkaPageBase : ComponentBase
    {
        [Inject] public KafkaService KafkaService { get; set; } = null!;

        protected IEnumerable<KafkaTopicInfo>? Topics { get; set; }

        protected override Task OnInitializedAsync()
        {
            Refresh();
            return Task.CompletedTask;
        }

        protected void Refresh()
        {
            Topics = KafkaService.GetTopics();
        }
    }
}
