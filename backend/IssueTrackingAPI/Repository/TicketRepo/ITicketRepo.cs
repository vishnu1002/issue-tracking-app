using IssueTrackingAPI.Model;

namespace IssueTrackingAPI.Repository.TicketRepo.TicketRepo;

public interface ITicketRepo
{
    Task<IEnumerable<TicketModel>> GetAllTickets();
    Task<TicketModel?> GetTicketById(int id);
    Task<TicketModel> AddTicket(TicketModel ticket);
    Task<TicketModel?> UpdateTicket(TicketModel ticket);
    Task<bool> DeleteTicket(int id);
}