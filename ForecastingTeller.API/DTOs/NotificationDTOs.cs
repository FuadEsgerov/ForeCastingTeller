using System.ComponentModel.DataAnnotations;

namespace ForecastingTeller.API.DTOs
{
    public enum NotificationType
    {
        System,
        TarotReading,
        Forecast,
        UserActivity
    }

    public class NotificationResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReadAt { get; set; }
        public string RelatedItemId { get; set; }
    }

    public class CreateNotificationRequest
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Message { get; set; }

        [Required]
        public NotificationType Type { get; set; }

        public string RelatedItemId { get; set; }
    }

    public class MarkNotificationReadRequest
    {
        [Required]
        public Guid NotificationId { get; set; }
    }

    public class NotificationListResponse
    {
        public IEnumerable<NotificationResponse> Notifications { get; set; }
        public int TotalCount { get; set; }
        public int UnreadCount { get; set; }
    }
}
