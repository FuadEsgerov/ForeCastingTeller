using System;

namespace ForecastingTeller.API.Models
{
    public class UserProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string FullName { get; set; } = string.Empty;  
        public string Bio { get; set; } = string.Empty;  
        public DateTime? DateOfBirth { get; set; }
        public string ProfileImageUrl { get; set; } = string.Empty;  
        public string ZodiacSign { get; set; } = string.Empty;  
        public string Preferences { get; set; }  = "{}";  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}