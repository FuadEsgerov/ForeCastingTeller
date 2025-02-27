
using System.ComponentModel.DataAnnotations;

namespace ForecastingTeller.API.DTOs
{
    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string Bio { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ProfileImageUrl { get; set; }
        public string ZodiacSign { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateProfileRequest
    {
        [StringLength(100)]
        public string FullName { get; set; }

        [StringLength(500)]
        public string Bio { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public string ProfileImageUrl { get; set; }

        [StringLength(20)]
        public string ZodiacSign { get; set; }
    }

    public class UserPreferencesRequest
    {
        public bool EnableEmailNotifications { get; set; }
        public bool EnablePushNotifications { get; set; }
        public bool DarkModeEnabled { get; set; }
        public string[] FavoriteCategories { get; set; }
        public string LanguagePreference { get; set; }
    }
}