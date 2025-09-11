using IssueTrackingAPI.Model;

namespace IssueTrackingAPI.Services;

public interface IEmailService
{
    Task SendTicketClosedNotificationAsync(TicketModel ticket, UserModel ticketCreator, UserModel assignedRep);
}
