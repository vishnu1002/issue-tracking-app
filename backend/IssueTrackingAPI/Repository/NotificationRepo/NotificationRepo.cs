using IssueTrackingAPI.Context;
using IssueTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace IssueTrackingAPI.Repository.NotificationRepo.NotificationRepo;

public class NotificationRepo : INotificationRepo
{
    private readonly AppDBContext _context;

    public NotificationRepo(AppDBContext context)
    {
        _context = context;
    }

    // Get All Notifications
    public async Task<IEnumerable<NotificationModel>> GetAllNotifications()
    {
        return await _context.Notifications_Table
            .Include(n => n.User)
            .Include(n => n.Ticket)
            .ToListAsync();
    }

    // Get Notification By Id
    public async Task<NotificationModel?> GetNotificationById(int id)
    {
        return await _context.Notifications_Table
            .Include(n => n.User)
            .Include(n => n.Ticket)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    // Get Notifications By User Id
    public async Task<IEnumerable<NotificationModel>> GetNotificationsByUserId(int userId)
    {
        return await _context.Notifications_Table
            .Where(n => n.UserId == userId)
            .Include(n => n.Ticket)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    // Get Unread Notifications By User Id
    public async Task<IEnumerable<NotificationModel>> GetUnreadNotificationsByUserId(int userId)
    {
        return await _context.Notifications_Table
            .Where(n => n.UserId == userId && !n.IsRead)
            .Include(n => n.Ticket)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    // Add Notification
    public async Task<NotificationModel> AddNotification(NotificationModel notification)
    {
        _context.Notifications_Table.Add(notification);
        await _context.SaveChangesAsync();
        return notification;
    }

    // Update Notification
    public async Task<NotificationModel?> UpdateNotification(NotificationModel notification)
    {
        var existing = await _context.Notifications_Table.FindAsync(notification.Id);
        if (existing == null) return null;

        existing.UserId = notification.UserId;
        existing.TicketId = notification.TicketId;
        existing.Type = notification.Type;
        existing.Message = notification.Message;
        existing.IsRead = notification.IsRead;

        await _context.SaveChangesAsync();
        return existing;
    }

    // Delete Notification
    public async Task<bool> DeleteNotification(int id)
    {
        var notification = await _context.Notifications_Table.FindAsync(id);
        if (notification == null) return false;

        _context.Notifications_Table.Remove(notification);
        await _context.SaveChangesAsync();
        return true;
    }

    // Get Unread Notification Count By User Id
    public async Task<int> GetUnreadNotificationCountByUserId(int userId)
    {
        return await _context.Notifications_Table
            .CountAsync(n => n.UserId == userId && !n.IsRead);
    }

    // Mark Notification As Read
    public async Task<bool> MarkNotificationAsRead(int notificationId, int userId)
    {
        var notification = await _context.Notifications_Table
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (notification == null) return false;

        notification.IsRead = true;
        await _context.SaveChangesAsync();
        return true;
    }

    // Mark All Notifications As Read By User Id
    public async Task<bool> MarkAllNotificationsAsReadByUserId(int userId)
    {
        var notifications = await _context.Notifications_Table
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}
