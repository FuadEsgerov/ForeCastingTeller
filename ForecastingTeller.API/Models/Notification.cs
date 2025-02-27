using System;

namespace ForecastingTeller.API.Models
{
    public enum NotificationType
    {
        System,
        TarotReading,
        Forecast,
        UserActivity
    }

    public class Notification
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public string RelatedItemId { get; set; } // ID of related item (tarot reading, forecast, etc.)
    }
}