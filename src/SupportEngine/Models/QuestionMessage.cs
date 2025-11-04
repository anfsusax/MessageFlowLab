using System;

namespace SupportEngine.Models
{
    // Envelope for messages exchanged between workers.
    public class QuestionMessage
    {
        // Unique id to correlate logs and processing across components.
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();

        // Message body (the question text)
        public string Question { get; set; } = string.Empty;

        // Optional metadata (e.g., user id, timestamp)
        public string? User { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
