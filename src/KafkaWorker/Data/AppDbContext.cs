using Microsoft.EntityFrameworkCore;

namespace KafkaWorker.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<MessageLog> MessageLogs { get; set; } = null!;
    }
}
