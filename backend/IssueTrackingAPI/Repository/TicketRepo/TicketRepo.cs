using IssueTrackingAPI.Context;
using IssueTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace IssueTrackingAPI.Repository.TicketRepo.TicketRepo;

public class TicketRepo : ITicketRepo
{
    private readonly AppDBContext _context;

    public TicketRepo(AppDBContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketModel>> GetAllTickets()
    {
        return await _context.Tickets_Table
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Attachments)
            .ToListAsync();
    }

    public async Task<TicketModel?> GetTicketById(int id)
    {
        return await _context.Tickets_Table
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Attachments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<TicketModel> AddTicket(TicketModel ticket)
    {
        _context.Tickets_Table.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<TicketModel?> UpdateTicket(TicketModel ticket)
    {
        var existing = await _context.Tickets_Table.FindAsync(ticket.Id);
        if (existing == null) return null;

        existing.Title = ticket.Title;
        existing.Description = ticket.Description;
        existing.Priority = ticket.Priority;
        existing.Type = ticket.Type;
        existing.Status = ticket.Status;
        existing.AssignedToUserId = ticket.AssignedToUserId;
        existing.Comment = ticket.Comment;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteTicket(int id)
    {
        var ticket = await _context.Tickets_Table.FindAsync(id);
        if (ticket == null) return false;

        _context.Tickets_Table.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TicketModel>> GetTicketsByCreator(int userId)
    {
        return await _context.Tickets_Table
            .Where(t => t.CreatedByUserId == userId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TicketModel>> GetTicketsByAssignee(int userId)
    {
        return await _context.Tickets_Table
            .Where(t => t.AssignedToUserId == userId)
            .ToListAsync();
    }
}
