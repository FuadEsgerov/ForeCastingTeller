using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Models;
using ForecastingTeller.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Services
{
    public interface IForecastingService
    {
        Task<ForecastResponse> GetForecastAsync(Guid forecastId);
        Task<ForecastListResponse> GetUserForecastsAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<ForecastListResponse> GetUserForecastsByCategoryAsync(Guid userId, string category, int page = 1, int pageSize = 10);
        Task<ForecastResponse> RequestForecastAsync(Guid userId, RequestForecastRequest request);
        Task<ForecastResponse> UpdateForecastAsync(Guid userId, Guid forecastId, UpdateForecastRequest request);
        Task<bool> DeleteForecastAsync(Guid userId, Guid forecastId);
    }

    public class ForecastingService : IForecastingService
    {
        private readonly IForecastRepository _forecastRepository;
        private readonly IUserRepository _userRepository;
        
        // This would typically be injected as a service that interfaces with an AI model
        // For this implementation, we'll use simpler methods for generating forecasts
        private readonly Random _random = new Random();

        public ForecastingService(
            IForecastRepository forecastRepository,
            IUserRepository userRepository)
        {
            _forecastRepository = forecastRepository ?? throw new ArgumentNullException(nameof(forecastRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<ForecastResponse> GetForecastAsync(Guid forecastId)
        {
            var forecast = await _forecastRepository.GetByIdAsync(forecastId);
            if (forecast == null)
            {
                throw new KeyNotFoundException($"Forecast with ID {forecastId} not found");
            }

            return MapToForecastResponse(forecast);
        }

        public async Task<ForecastListResponse> GetUserForecastsAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var forecasts = await _forecastRepository.GetByUserIdAsync(userId, page, pageSize);
            var totalCount = await _forecastRepository.GetCountByUserIdAsync(userId);

            return new ForecastListResponse
            {
                Forecasts = forecasts.Select(MapToForecastResponse),
                TotalCount = totalCount
            };
        }

        public async Task<ForecastListResponse> GetUserForecastsByCategoryAsync(Guid userId, string category, int page = 1, int pageSize = 10)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var forecasts = await _forecastRepository.GetByUserIdAndCategoryAsync(userId, category, page, pageSize);
            var totalCount = await _forecastRepository.GetCountByUserIdAndCategoryAsync(userId, category);

            return new ForecastListResponse
            {
                Forecasts = forecasts.Select(MapToForecastResponse),
                TotalCount = totalCount
            };
        }

        public async Task<ForecastResponse> RequestForecastAsync(Guid userId, RequestForecastRequest request)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Generate forecast content using AI (simulated for this example)
            string forecastContent = GenerateForecastContent(user, request);
            
            // Calculate expiry date based on forecast type
            DateTime expiryDate = CalculateExpiryDate(request.Type, request.ForecastDate);

            // Create a new forecast
            var forecast = new Forecast
            {
                UserId = userId,
                Title = request.Title,
                Type = request.Type,
                Category = request.Category,
                Content = forecastContent,
                ForecastDate = request.ForecastDate,
                ExpiryDate = expiryDate,
                IsFavorite = false,
                Accuracy = 0 // Will be rated by user later
            };

            // Save the forecast
            var createdForecast = await _forecastRepository.CreateAsync(forecast);

            return MapToForecastResponse(createdForecast);
        }

        public async Task<ForecastResponse> UpdateForecastAsync(Guid userId, Guid forecastId, UpdateForecastRequest request)
        {
            // Get the forecast
            var forecast = await _forecastRepository.GetByIdAsync(forecastId);
            if (forecast == null)
            {
                throw new KeyNotFoundException($"Forecast with ID {forecastId} not found");
            }

            // Verify the forecast belongs to the user
            if (forecast.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to update this forecast");
            }

            // Update the forecast
            if (request.IsFavorite)
            {
                forecast.IsFavorite = request.IsFavorite;
            }
            
            if (request.Accuracy.HasValue)
            {
                // Ensure accuracy is within valid range
                forecast.Accuracy = Math.Clamp(request.Accuracy.Value, 0, 100);
            }
            
            // Save the changes
            var updatedForecast = await _forecastRepository.UpdateAsync(forecast);

            return MapToForecastResponse(updatedForecast);
        }

        public async Task<bool> DeleteForecastAsync(Guid userId, Guid forecastId)
        {
            // Get the forecast
            var forecast = await _forecastRepository.GetByIdAsync(forecastId);
            if (forecast == null)
            {
                throw new KeyNotFoundException($"Forecast with ID {forecastId} not found");
            }

            // Verify the forecast belongs to the user
            if (forecast.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this forecast");
            }

            // Delete the forecast
            return await _forecastRepository.DeleteAsync(forecastId);
        }

        private string GenerateForecastContent(User user, RequestForecastRequest request)
        {
            // In a real application, this would call an AI service for personalized forecasts
            // Here we're just simulating with templated content
            var templates = new Dictionary<string, List<string>>
            {
                { "Love", new List<string> {
                    "Your romantic life is about to enter an exciting phase. New connections may form, and existing relationships will deepen. Be open to vulnerability and honest communication.",
                    "This is a period of reflection in your love life. Take time to understand your own needs before seeking fulfillment from others. Self-love leads to healthier relationships.",
                    "Romance is highlighted in your forecast. Whether single or attached, your charisma is at a peak. Use this energy wisely and be clear about your intentions."
                }},
                { "Career", new List<string> {
                    "Professional growth is on the horizon. Your hard work is being noticed, and new opportunities will present themselves. Stay prepared and continue to develop your skills.",
                    "A period of workplace challenges may test your resilience. View these as opportunities to prove your capabilities. A mentor figure will provide valuable guidance.",
                    "Your creative energy at work is peaking. This is an excellent time to propose new ideas and take initiative on projects. Your unique perspective will be valued."
                }},
                { "Health", new List<string> {
                    "Your physical energy is strong, but balance is key. Incorporate both activity and rest into your routine. Pay attention to nutritional needs to maintain this positive phase.",
                    "Mental wellness deserves your focus now. Practices like meditation or journaling will prove especially beneficial. Don't hesitate to seek support if needed.",
                    "A holistic approach to health will serve you best. Consider how emotional, physical, and spiritual aspects interconnect. Small, consistent habits will yield significant benefits."
                }},
                { "Finance", new List<string> {
                    "Financial stability is within reach. Review your budget and identify areas for adjustment. A disciplined approach now will create security for the future.",
                    "Unexpected financial opportunities may arise. Evaluate them carefully rather than making impulsive decisions. Consulting with a trusted advisor would be beneficial.",
                    "This is a good time to reassess your financial goals. Your intuition about investments is particularly strong now, but still do your research before committing resources."
                }}
            };

            // Get templates for the requested category, or use general templates if category not found
            List<string> categoryTemplates;
            if (!templates.TryGetValue(request.Category, out categoryTemplates))
            {
                categoryTemplates = new List<string> {
                    "The coming period brings positive energy and new opportunities. Stay open to unexpected developments and trust your intuition when making decisions.",
                    "A time of reflection and growth awaits you. You may need to reassess priorities and make adjustments to align with your true values and goals.",
                    "Changes in your environment will create new possibilities. Remain adaptable and view challenges as stepping stones rather than obstacles."
                };
            }

            // Select a random template from the appropriate category
            string baseTemplate = categoryTemplates[_random.Next(categoryTemplates.Count)];
            
            // Add type-specific content
            string typeSpecificContent = GetTypeSpecificContent(request.Type);
            
            // Personalize with user info if available
            string personalizedContent = "";
            if (user.Profile != null && !string.IsNullOrEmpty(user.Profile.ZodiacSign))
            {
                personalizedContent = $"\n\nAs a {user.Profile.ZodiacSign}, you should pay special attention to how the planetary alignments of {GetRandomPlanet()} and {GetRandomPlanet()} influence your decisions during this period.";
            }

            return $"{baseTemplate}\n\n{typeSpecificContent}{personalizedContent}";
        }

        private string GetTypeSpecificContent(ForecastType type)
        {
            switch (type)
            {
                case ForecastType.Daily:
                    return "Focus on being present today. Even small actions can have significant impacts.";
                case ForecastType.Weekly:
                    return "Your week ahead contains multiple opportunities for growth. Tuesday and Thursday are particularly favorable for important conversations.";
                case ForecastType.Monthly:
                    return "This month represents a cycle of completion and new beginnings. The third week is especially powerful for manifesting your intentions.";
                case ForecastType.Yearly:
                    return "The coming year contains four distinct phases of development. Patience during challenges in the second quarter will lead to significant breakthroughs later.";
                case ForecastType.Custom:
                    return "This specialized forecast period highlights your unique path. Trust the timing of events, even when progress seems slow.";
                default:
                    return "";
            }
        }

        private string GetRandomPlanet()
        {
            string[] planets = { "Mercury", "Venus", "Mars", "Jupiter", "Saturn", "Neptune", "Uranus", "Pluto" };
            return planets[_random.Next(planets.Length)];
        }

        private DateTime CalculateExpiryDate(ForecastType type, DateTime forecastDate)
        {
            return type switch
            {
                ForecastType.Daily => forecastDate.AddDays(1),
                ForecastType.Weekly => forecastDate.AddDays(7),
                ForecastType.Monthly => forecastDate.AddMonths(1),
                ForecastType.Yearly => forecastDate.AddYears(1),
                ForecastType.Custom => forecastDate.AddMonths(3), // Default 3 months for custom forecasts
                _ => forecastDate.AddDays(7) // Default 1 week
            };
        }

        private ForecastResponse MapToForecastResponse(Forecast forecast)
        {
            return new ForecastResponse
            {
                Id = forecast.Id,
                Title = forecast.Title,
                Type = forecast.Type,
                Category = forecast.Category,
                Content = forecast.Content,
                CreatedAt = forecast.CreatedAt,
                ForecastDate = forecast.ForecastDate,
                ExpiryDate = forecast.ExpiryDate,
                IsFavorite = forecast.IsFavorite,
                Accuracy = forecast.Accuracy
            };
        }
    }
}