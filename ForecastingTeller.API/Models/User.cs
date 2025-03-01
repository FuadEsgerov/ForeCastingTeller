using System;
using System.Collections.Generic;

namespace ForecastingTeller.API.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastLoginAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string EmailVerificationToken { get; set; }
        public string PasswordResetToken { get; set; }
        public DateTime? PasswordResetExpiry { get; set; }

        public UserProfile Profile { get; set; }
        public List<Notification> Notifications { get; set; } = new List<Notification>();
        public List<TarotReading> TarotReadings { get; set; } = new List<TarotReading>();
        public List<Forecast> Forecasts { get; set; } = new List<Forecast>();
    }
}