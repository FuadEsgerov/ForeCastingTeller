using System;

namespace ForecastingTeller.API.Models
{
    public enum ForecastType
    {
        Daily,
        Weekly,
        Monthly,
        Yearly,
        Custom
    }

    public class Forecast
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string Title { get; set; }
        public ForecastType Type { get; set; }
        public string Category { get; set; } // Love, Career, Health, etc.
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ForecastDate { get; set; } // Date the forecast is for
        public DateTime ExpiryDate { get; set; } // When the forecast expires
        public bool IsFavorite { get; set; } = false;
        public double Accuracy { get; set; } // User-rated accuracy (0-100%)
    }
}