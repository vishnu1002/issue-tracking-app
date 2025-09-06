using IssueTrackingAPI.DTO.NotificationDTO;
using IssueTrackingAPI.Repository.NotificationRepo.NotificationRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationController : ControllerBase
{
    private readonly INotificationRepo _notificationRepo;

    public NotificationController(INotificationRepo notificationRepo)
    {
        _notificationRepo = notificationRepo;
    }

    //
    // Get User Notifications
    // GET: api/notifications
    //
    [HttpGet]
    public async Task<ActionResult<List<NotificationRead_DTO>>> GetUserNotifications(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var notifications = await _notificationRepo.GetNotificationsByUserId(currentUserId);
            
            var notificationDtos = notifications
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationRead_DTO
                {
                    Id = n.Id,
                    UserId = n.UserId,
                    TicketId = n.TicketId,
                    Type = n.Type,
                    Message = n.Message,
                    IsRead = n.IsRead,
                    CreatedAt = n.CreatedAt,
                    TicketTitle = n.Ticket?.Title
                });

            return Ok(notificationDtos);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving notifications", error = ex.Message });
        }
    }

    //
    // Get Unread Notification Count
    // GET: api/notifications/unread-count
    //
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadNotificationCount()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var count = await _notificationRepo.GetUnreadNotificationCountByUserId(currentUserId);
            return Ok(new { unreadCount = count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error retrieving unread count", error = ex.Message });
        }
    }

    //
    // Mark Notification as Read
    // PUT: api/notifications/{id}/read
    //
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkNotificationAsRead(int id)
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var success = await _notificationRepo.MarkNotificationAsRead(id, currentUserId);
            
            if (!success)
                return NotFound(new { message = "Notification not found" });
            return Ok(new { message = "Notification marked as read" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error marking notification as read", error = ex.Message });
        }
    }

    //
    // Mark All Notifications as Read
    // PUT: api/notifications/read-all
    //
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllNotificationsAsRead()
    {
        try
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            await _notificationRepo.MarkAllNotificationsAsReadByUserId(currentUserId);
            return Ok(new { message = "All notifications marked as read" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error marking all notifications as read", error = ex.Message });
        }
    }
}
