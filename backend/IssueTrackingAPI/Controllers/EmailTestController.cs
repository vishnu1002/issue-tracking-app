using Microsoft.AspNetCore.Mvc;
using IssueTrackingAPI.Services;
using IssueTrackingAPI.Model;
using Microsoft.AspNetCore.Authorization;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/test")]
[Authorize]
public class EmailTestController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly ILogger<EmailTestController> _logger;

    public EmailTestController(IEmailService emailService, ILogger<EmailTestController> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    [HttpPost("email")]
    public async Task<IActionResult> TestEmailNotification()
    {
        try
        {
            // Create test data
            var testTicket = new TicketModel
            {
                Id = 999,
                Title = "Test Ticket for Email Notification",
                Description = "This is a test ticket to verify email notification functionality.",
                Priority = "High",
                Type = "Software",
                Status = "Closed",
                CreatedAt = DateTime.Now.AddHours(-2),
                UpdatedAt = DateTime.Now,
                ResolutionNotes = "This ticket has been resolved successfully.",
                Comment = "All issues have been addressed."
            };

            var testCreator = new UserModel
            {
                Id = 1,
                Name = "Test User",
                Email = "testuser@example.com",
                PasswordHash = "test-hash",
                Role = "User"
            };

            var testRep = new UserModel
            {
                Id = 2,
                Name = "Test Representative",
                Email = "testrep@example.com",
                PasswordHash = "test-hash",
                Role = "Representative"
            };

            // Send test email
            await _emailService.SendTicketClosedNotificationAsync(testTicket, testCreator, testRep);

            return Ok(new
            {
                message = "Test email notification triggered successfully!",
                note = "In development mode, the email content is logged to console and saved to logs/email-notifications.log"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send test email notification");
            return StatusCode(500, new
            {
                message = "Failed to send test email notification",
                error = ex.Message
            });
        }
    }
}
