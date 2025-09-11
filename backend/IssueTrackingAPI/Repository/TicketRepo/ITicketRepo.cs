using IssueTrackingAPI.Model;
using IssueTrackingAPI.DTO.TicketDTO;

namespace IssueTrackingAPI.Repository.TicketRepo.TicketRepo;

public interface ITicketRepo
{
    Task<IEnumerable<TicketModel>> GetAllTickets();
    Task<TicketModel?> GetTicketById(int id);
    Task<TicketModel?> GetTicketByIdWithUsers(int id);
    Task<TicketModel> AddTicket(TicketModel ticket);
    Task<TicketModel?> UpdateTicket(TicketModel ticket);
    Task<bool> DeleteTicket(int id);

    Task<IEnumerable<TicketModel>> GetTicketsByCreator(int userId);
    Task<IEnumerable<TicketModel>> GetTicketsByAssignee(int userId);
    Task<IEnumerable<TicketModel>> GetUnassignedTickets();
    Task<(IEnumerable<TicketModel> tickets, int totalCount)> SearchTickets(TicketSearch_DTO searchDto);

    // Attachments
    Task<AttachmentModel?> AddAttachment(AttachmentModel attachment);
    Task<IEnumerable<AttachmentModel>> GetAttachmentsByTicket(int ticketId);
    Task<AttachmentModel?> GetAttachmentById(int attachmentId);
    Task<bool> DeleteAttachment(int attachmentId);
}

