using Microsoft.EntityFrameworkCore;
using DashboardBlazor.Models;

namespace DashboardBlazor.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MessageLog> MessageLogs { get; set; } = null!;
    }
}