using IssueTrackingAPI.DTO.NotificationDTO;
using IssueTrackingAPI.Hubs;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.NotificationRepo.NotificationRepo;
using IssueTrackingAPI.Repository.UserRepo.UserRepo;
using Microsoft.AspNetCore.SignalR;

namespace IssueTrackingAPI.Services;

public interface INotificationSignalRService
{
    Task SendNotificationAsync(int userId, int? ticketId, string type, string message);
    Task SendNotificationToAdminsAsync(int? ticketId, string type, string message);
}

public class NotificationSignalRService : INotificationSignalRService
{
    private readonly INotificationRepo _notificationRepo;
    private readonly IUserRepo _userRepo;
    private readonly IHubContext<NotificationHub> _hubContext;

    public NotificationSignalRService(
        INotificationRepo notificationRepo, 
        IUserRepo userRepo,
        IHubContext<NotificationHub> hubContext)
    {
        _notificationRepo = notificationRepo;
        _userRepo = userRepo;
        _hubContext = hubContext;
    }

    public async Task SendNotificationAsync(int userId, int? ticketId, string type, string message)
    {
        try
        {
            var notification = new NotificationModel
            {
                UserId = userId,
                TicketId = ticketId,
                Type = type,
                Message = message
            };

            await _notificationRepo.AddNotification(notification);

            // Send real-time notification via SignalR
            var notificationDto = new NotificationRead_DTO
            {
                Id = notification.Id,
                UserId = notification.UserId,
                TicketId = notification.TicketId,
                Type = notification.Type,
                Message = notification.Message,
                IsRead = notification.IsRead,
                CreatedAt = notification.CreatedAt
            };

            await _hubContext.Clients.Group($"user_{userId}").SendAsync("ReceiveNotification", notificationDto);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the operation
            Console.WriteLine($"Error sending notification: {ex.Message}");
        }
    }

    public async Task SendNotificationToAdminsAsync(int? ticketId, string type, string message)
    {
        try
        {
            var adminUsers = await _userRepo.GetAllUsers();
            var admins = adminUsers.Where(u => u.Role == "Admin").ToList();

            foreach (var admin in admins)
            {
                await SendNotificationAsync(admin.Id, ticketId, type, message);
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the operation
            Console.WriteLine($"Error sending notifications to admins: {ex.Message}");
        }
    }
}
