using IssueTrackingAPI.Model;

namespace IssueTrackingAPI.Repository.NotificationRepo.NotificationRepo;

public interface INotificationRepo
{
    // Async Methods
    Task<IEnumerable<NotificationModel>> GetAllNotifications();
    Task<NotificationModel?> GetNotificationById(int id);
    Task<IEnumerable<NotificationModel>> GetNotificationsByUserId(int userId);
    Task<IEnumerable<NotificationModel>> GetUnreadNotificationsByUserId(int userId);
    Task<NotificationModel> AddNotification(NotificationModel notification);
    Task<NotificationModel?> UpdateNotification(NotificationModel notification);
    Task<bool> DeleteNotification(int id);
    Task<int> GetUnreadNotificationCountByUserId(int userId);
    Task<bool> MarkNotificationAsRead(int notificationId, int userId);
    Task<bool> MarkAllNotificationsAsReadByUserId(int userId);
}
