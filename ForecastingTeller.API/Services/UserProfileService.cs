using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Models;
using ForecastingTeller.API.Repositories;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Services
{
    public interface IUserProfileService
    {
        Task<UserProfileResponse> GetUserProfileAsync(Guid userId);
        Task<UserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request);
        Task<UserProfileResponse> UpdateUserPreferencesAsync(Guid userId, UserPreferencesRequest request);
    }

    public class UserProfileService : IUserProfileService
    {
        private readonly IUserRepository _userRepository;

        public UserProfileService(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<UserProfileResponse> GetUserProfileAsync(Guid userId)
        {
            // Get the user with their profile
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            return MapToUserProfileResponse(user);
        }

        public async Task<UserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateProfileRequest request)
        {
            // Get the user with their profile
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Update the profile
            if (user.Profile == null)
            {
                // Create profile if it doesn't exist
                user.Profile = new UserProfile
                {
                    UserId = userId
                };
            }

            // Update properties if provided
            if (!string.IsNullOrEmpty(request.FullName))
            {
                user.Profile.FullName = request.FullName;
            }

            if (request.Bio != null) // Allow clearing the bio by passing empty string
            {
                user.Profile.Bio = request.Bio;
            }

            if (request.DateOfBirth.HasValue)
            {
                user.Profile.DateOfBirth = request.DateOfBirth;
            }

            if (request.ProfileImageUrl != null) // Allow clearing the image by passing empty string
            {
                user.Profile.ProfileImageUrl = request.ProfileImageUrl;
            }

            if (!string.IsNullOrEmpty(request.ZodiacSign))
            {
                user.Profile.ZodiacSign = request.ZodiacSign;
            }

            // Update timestamp
            user.Profile.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _userRepository.UpdateUserProfileAsync(user.Profile);

            return MapToUserProfileResponse(user);
        }

        public async Task<UserProfileResponse> UpdateUserPreferencesAsync(Guid userId, UserPreferencesRequest request)
        {
            // Get the user with their profile
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Update the profile
            if (user.Profile == null)
            {
                // Create profile if it doesn't exist
                user.Profile = new UserProfile
                {
                    UserId = userId
                };
            }

            // Serialize preferences to JSON
            user.Profile.Preferences = JsonSerializer.Serialize(request);
            
            // Update timestamp
            user.Profile.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _userRepository.UpdateUserProfileAsync(user.Profile);

            return MapToUserProfileResponse(user);
        }

        private UserProfileResponse MapToUserProfileResponse(User user)
        {
            // Handle the case where the user doesn't have a profile yet
            if (user.Profile == null)
            {
                return new UserProfileResponse
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.CreatedAt
                };
            }

            return new UserProfileResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.Profile.FullName,
                Bio = user.Profile.Bio,
                DateOfBirth = user.Profile.DateOfBirth,
                ProfileImageUrl = user.Profile.ProfileImageUrl,
                ZodiacSign = user.Profile.ZodiacSign,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.Profile.UpdatedAt
            };
        }
    }
    }