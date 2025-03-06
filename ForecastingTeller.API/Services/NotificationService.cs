using ForecastingTeller.API.DTOs;
using ForecastingTeller.API.Models;
using ForecastingTeller.API.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ForecastingTeller.API.Services
{
    public interface INotificationService
    {
        Task<NotificationResponse> GetNotificationAsync(Guid notificationId);
        Task<NotificationListResponse> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 10);
        Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request);
        Task<bool> MarkNotificationReadAsync(Guid userId, Guid notificationId);
        Task<bool> MarkAllNotificationsReadAsync(Guid userId);
        Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId);
        
        // System notification methods
        Task<NotificationResponse> CreateSystemNotificationAsync(Guid userId, string title, string message);
        Task<NotificationResponse> CreateTarotReadingNotificationAsync(Guid userId, Guid readingId, string title);
        Task<NotificationResponse> CreateForecastNotificationAsync(Guid userId, Guid forecastId, string title);
    }

    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository ?? throw new ArgumentNullException(nameof(notificationRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public async Task<NotificationResponse> GetNotificationAsync(Guid notificationId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {notificationId} not found");
            }

            return MapToNotificationResponse(notification);
        }

        public async Task<NotificationListResponse> GetUserNotificationsAsync(Guid userId, int page = 1, int pageSize = 10)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            var notifications = await _notificationRepository.GetByUserIdAsync(userId, page, pageSize);
            var totalCount = await _notificationRepository.GetCountByUserIdAsync(userId);
            var unreadCount = await _notificationRepository.GetUnreadCountByUserIdAsync(userId);

            return new NotificationListResponse
            {
                Notifications = notifications.Select(MapToNotificationResponse),
                TotalCount = totalCount,
                UnreadCount = unreadCount
            };
        }

        public async Task<NotificationResponse> CreateNotificationAsync(CreateNotificationRequest request)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {request.UserId} not found");
            }

            // Create a new notification
            var notification = new Notification
            {
                UserId = request.UserId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                RelatedItemId = request.RelatedItemId
            };

            // Save the notification
            var createdNotification = await _notificationRepository.CreateAsync(notification);

            return MapToNotificationResponse(createdNotification);
        }

        public async Task<bool> MarkNotificationReadAsync(Guid userId, Guid notificationId)
        {
            // Get the notification
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {notificationId} not found");
            }

            // Verify the notification belongs to the user
            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to mark this notification as read");
            }

            // Mark as read
            return await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task<bool> MarkAllNotificationsReadAsync(Guid userId)
        {
            // Validate user exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found");
            }

            // Mark all notifications as read
            return await _notificationRepository.MarkAllAsReadAsync(userId);
        }

        public async Task<bool> DeleteNotificationAsync(Guid userId, Guid notificationId)
        {
            // Get the notification
            var notification = await _notificationRepository.GetByIdAsync(notificationId);
            if (notification == null)
            {
                throw new KeyNotFoundException($"Notification with ID {notificationId} not found");
            }

            // Verify the notification belongs to the user
            if (notification.UserId != userId)
            {
                throw new UnauthorizedAccessException("You do not have permission to delete this notification");
            }

            // Delete the notification
            return await _notificationRepository.DeleteAsync(notificationId);
        }

        public async Task<NotificationResponse> CreateSystemNotificationAsync(Guid userId, string title, string message)
        {
            // Create system notification
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = NotificationType.System
            };

            return await CreateNotificationAsync(request);
        }

        public async Task<NotificationResponse> CreateTarotReadingNotificationAsync(Guid userId, Guid readingId, string title)
        {
            // Create tarot reading notification
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Title = title,
                Message = "Your tarot reading is ready to view.",
                Type = NotificationType.TarotReading,
                RelatedItemId = readingId.ToString()
            };

            return await CreateNotificationAsync(request);
        }

        public async Task<NotificationResponse> CreateForecastNotificationAsync(Guid userId, Guid forecastId, string title)
        {
            // Create forecast notification
            var request = new CreateNotificationRequest
            {
                UserId = userId,
                Title = title,
                Message = "Your forecast is ready to view.",
                Type = NotificationType.Forecast,
                RelatedItemId = forecastId.ToString()
            };

            return await CreateNotificationAsync(request);
        }

        private NotificationResponse MapToNotificationResponse(Notification notification)
        {
            return new NotificationResponse
            {
                Id = notification.Id,
                Title = notification.Title,
                Message = notification.Message,
                Type = notification.Type,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt,
                ReadAt = notification.ReadAt,
                RelatedItemId = notification.RelatedItemId
            };
        }
    }
}