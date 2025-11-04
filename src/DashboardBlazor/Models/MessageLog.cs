using System;

namespace DashboardBlazor.Models
{
    public class MessageLog
    {
        public int Id { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public string? User { get; set; }
        public string Question { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Topic { get; set; } = string.Empty;
        public int Partition { get; set; }
        public long Offset { get; set; }
        public string RawPayload { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}