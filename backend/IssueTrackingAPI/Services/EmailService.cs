using IssueTrackingAPI.Model;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace IssueTrackingAPI.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _isDevelopment;

    public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger, IConfiguration configuration)
    {
        _emailSettings = emailSettings.Value;
        _logger = logger;
        _configuration = configuration;
        _isDevelopment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") == "Development";
    }

    public async Task SendTicketClosedNotificationAsync(TicketModel ticket, UserModel ticketCreator, UserModel assignedRep)
    {
        try
        {
            // Check if email sending is enabled
            var emailEnabled = _configuration.GetValue<bool>("Email:SendOnClosed", true);
            if (!emailEnabled)
            {
                _logger.LogInformation($"Email sending is disabled. Skipping notification for ticket #{ticket.Id}");
                return;
            }

            // In development, log the email content unless forced to send
            var forceSendInDev = _configuration.GetValue<bool>("Email:ForceSendInDevelopment", false);
            if (_isDevelopment && !forceSendInDev)
            {
                await LogEmailForDevelopment(ticket, ticketCreator, assignedRep);
                return;
            }

            var message = new MimeMessage();
            // Use system address as technical sender, Rep as display sender and Reply-To
            message.From.Add(new MailboxAddress(assignedRep.Name, _emailSettings.FromEmail));
            message.ReplyTo.Add(new MailboxAddress(assignedRep.Name, assignedRep.Email));
            message.To.Add(new MailboxAddress(ticketCreator.Name, ticketCreator.Email));
            message.Subject = $"Ticket #{ticket.Id} - {ticket.Title} has been closed";

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = GenerateTicketClosedEmailHtml(ticket, ticketCreator, assignedRep);
            bodyBuilder.TextBody = GenerateTicketClosedEmailText(ticket, ticketCreator, assignedRep);

            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            // Use STARTTLS when available (port 587) for better security
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(_emailSettings.SmtpUsername, _emailSettings.SmtpPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation($"Ticket closure notification sent successfully to {ticketCreator.Email} for ticket #{ticket.Id}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send ticket closure notification to {ticketCreator.Email} for ticket #{ticket.Id}");
            throw;
        }
    }

    private async Task LogEmailForDevelopment(TicketModel ticket, UserModel ticketCreator, UserModel assignedRep)
    {
        _logger.LogInformation("=== EMAIL NOTIFICATION (DEVELOPMENT MODE) ===");
        _logger.LogInformation($"To: {ticketCreator.Name} <{ticketCreator.Email}>");
        _logger.LogInformation($"Subject: Ticket #{ticket.Id} - {ticket.Title} has been closed");
        _logger.LogInformation($"From: {_emailSettings.FromName} <{_emailSettings.FromEmail}>");
        _logger.LogInformation("--- Email Content ---");
        
        var textContent = GenerateTicketClosedEmailText(ticket, ticketCreator, assignedRep);
        _logger.LogInformation(textContent);
        
        _logger.LogInformation("=== END EMAIL NOTIFICATION ===");
        
        // Also save to a file for easy viewing
        var emailLogPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "email-notifications.log");
        Directory.CreateDirectory(Path.GetDirectoryName(emailLogPath)!);
        
        var logEntry = $@"
[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] EMAIL NOTIFICATION
To: {ticketCreator.Name} <{ticketCreator.Email}>
Subject: Ticket #{ticket.Id} - {ticket.Title} has been closed
From: {_emailSettings.FromName} <{_emailSettings.FromEmail}>

{textContent}

{new string('=', 80)}

";
        
        await File.AppendAllTextAsync(emailLogPath, logEntry);
        _logger.LogInformation($"Email content saved to: {emailLogPath}");
    }

    private string GenerateTicketClosedEmailHtml(TicketModel ticket, UserModel ticketCreator, UserModel assignedRep)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Ticket Closed Notification</title>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #28a745; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }}
        .ticket-info {{ background-color: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #28a745; }}
        .status-badge {{ background-color: #28a745; color: white; padding: 5px 10px; border-radius: 15px; font-size: 12px; font-weight: bold; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Ticket Closed Successfully</h1>
        </div>
        <div class='content'>
            <p>Hello <strong>{ticketCreator.Name}</strong>,</p>
            
            <p>Great news! Your ticket has been successfully closed by our support team.</p>
            
            <div class='ticket-info'>
                <h3>Ticket Details</h3>
                <p><strong>Ticket ID:</strong> #{ticket.Id}</p>
                <p><strong>Title:</strong> {ticket.Title}</p>
                <p><strong>Status:</strong> <span class='status-badge'>CLOSED</span></p>
                <p><strong>Priority:</strong> {ticket.Priority}</p>
                <p><strong>Type:</strong> {ticket.Type}</p>
                <p><strong>Created:</strong> {ticket.CreatedAt:MMM dd, yyyy 'at' h:mm tt}</p>
                <p><strong>Closed:</strong> {ticket.UpdatedAt:MMM dd, yyyy 'at' h:mm tt}</p>
                <p><strong>Assigned Representative:</strong> {assignedRep.Name}</p>
            </div>

            {(string.IsNullOrEmpty(ticket.ResolutionNotes) ? "" : $@"
            <div class='ticket-info'>
                <h3>Resolution Notes</h3>
                <p>{ticket.ResolutionNotes}</p>
            </div>")}

            {(string.IsNullOrEmpty(ticket.Comment) ? "" : $@"
            <div class='ticket-info'>
                <h3>Final Comment</h3>
                <p>{ticket.Comment}</p>
            </div>")}

            <p>Thank you for using our issue tracking system. If you have any questions or need further assistance, please don't hesitate to contact us.</p>
            
            <div class='footer'>
                <p>This is an automated notification from the Issue Tracking System.</p>
                <p>Please do not reply to this email.</p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

    private string GenerateTicketClosedEmailText(TicketModel ticket, UserModel ticketCreator, UserModel assignedRep)
    {
        return $@"
TICKET CLOSED NOTIFICATION
==========================

Hello {ticketCreator.Name},

Great news! Your ticket has been successfully closed by our support team.

TICKET DETAILS:
- Ticket ID: #{ticket.Id}
- Title: {ticket.Title}
- Status: CLOSED
- Priority: {ticket.Priority}
- Type: {ticket.Type}
- Created: {ticket.CreatedAt:MMM dd, yyyy 'at' h:mm tt}
- Closed: {ticket.UpdatedAt:MMM dd, yyyy 'at' h:mm tt}
- Assigned Representative: {assignedRep.Name}

{(string.IsNullOrEmpty(ticket.ResolutionNotes) ? "" : $@"
RESOLUTION NOTES:
{ticket.ResolutionNotes}

")}{(string.IsNullOrEmpty(ticket.Comment) ? "" : $@"
FINAL COMMENT:
{ticket.Comment}

")}Thank you for using our issue tracking system. If you have any questions or need further assistance, please don't hesitate to contact us.

---
This is an automated notification from the Issue Tracking System.
Please do not reply to this email.";
    }
}
