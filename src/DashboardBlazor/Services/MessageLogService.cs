using DashboardBlazor.Data;
using DashboardBlazor.Models;
using Microsoft.EntityFrameworkCore;

namespace DashboardBlazor.Services
{
    public class MessageLogService
    {
        private readonly AppDbContext _db;
        public MessageLogService(AppDbContext db) => _db = db;

        public async Task<int> GetTotalAsync(CancellationToken ct = default)
        {
            return await _db.MessageLogs.CountAsync(ct);
        }

        public async Task<List<MessageLog>> GetRecentAsync(int take = 50, CancellationToken ct = default)
        {
            return await _db.MessageLogs.OrderByDescending(m => m.Id).Take(take).ToListAsync(ct);
        }
    }
}
