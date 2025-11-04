using Microsoft.AspNetCore.Components;
using DashboardBlazor.Services;
using DashboardBlazor.Models;

namespace DashboardBlazor.Pages
{
    public class LogsPageBase : ComponentBase
    {
        [Inject] public MessageLogService LogService { get; set; } = null!;

        protected List<MessageLog>? Messages { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await Refresh();
        }

        protected async Task Refresh()
        {
            Messages = await LogService.GetRecentAsync(100);
        }
    }
}
