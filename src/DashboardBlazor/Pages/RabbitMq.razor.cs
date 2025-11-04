using Microsoft.AspNetCore.Components;
using DashboardBlazor.Services;

namespace DashboardBlazor.Pages
{
    public class RabbitMqPageBase : ComponentBase
    {
        [Inject] public RabbitMqService Rabbit { get; set; } = null!;

        protected IEnumerable<RabbitQueueInfo>? Queues { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Refresh();
        }

        protected async Task Refresh()
        {
            Queues = await Rabbit.GetQueuesAsync();
        }
    }
}
