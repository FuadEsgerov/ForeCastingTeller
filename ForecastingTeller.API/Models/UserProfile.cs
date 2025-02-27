using System;

namespace ForecastingTeller.API.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string FullName { get; set; }
        public string Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ZodiacSign { get; set; }
        public string Preferences { get; set; } // JSON string to store user preferences
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}