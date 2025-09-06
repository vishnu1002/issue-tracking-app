using IssueTrackingAPI.Context;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.DTO.TicketDTO;
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
        existing.ResolutionNotes = ticket.ResolutionNotes;
        existing.UpdatedAt = DateTime.UtcNow;

        // Handle KPI tracking
        if (ticket.Status == "Closed" && existing.Status != "Closed")
        {
            existing.ResolvedAt = DateTime.UtcNow;
            existing.ResolutionTime = existing.ResolvedAt.Value - existing.CreatedAt;
        }
        else if (ticket.Status != "Closed" && existing.Status == "Closed")
        {
            // Ticket was reopened
            existing.ResolvedAt = null;
            existing.ResolutionTime = null;
        }

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

    public async Task<IEnumerable<TicketModel>> GetUnassignedTickets()
    {
        return await _context.Tickets_Table
            .Where(t => t.AssignedToUserId == null)
            .ToListAsync();
    }

    public async Task<(IEnumerable<TicketModel> tickets, int totalCount)> SearchTickets(TicketSearch_DTO searchDto)
    {
        var query = _context.Tickets_Table.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(searchDto.Title))
            query = query.Where(t => t.Title.Contains(searchDto.Title));

        if (!string.IsNullOrEmpty(searchDto.Description))
            query = query.Where(t => t.Description.Contains(searchDto.Description));

        if (!string.IsNullOrEmpty(searchDto.Priority))
            query = query.Where(t => t.Priority == searchDto.Priority);

        if (!string.IsNullOrEmpty(searchDto.Type))
            query = query.Where(t => t.Type == searchDto.Type);

        if (!string.IsNullOrEmpty(searchDto.Status))
            query = query.Where(t => t.Status == searchDto.Status);

        if (searchDto.CreatedByUserId.HasValue)
            query = query.Where(t => t.CreatedByUserId == searchDto.CreatedByUserId.Value);

        if (searchDto.AssignedToUserId.HasValue)
            query = query.Where(t => t.AssignedToUserId == searchDto.AssignedToUserId.Value);

        if (searchDto.CreatedFrom.HasValue)
            query = query.Where(t => t.CreatedAt >= searchDto.CreatedFrom.Value);

        if (searchDto.CreatedTo.HasValue)
            query = query.Where(t => t.CreatedAt <= searchDto.CreatedTo.Value);

        if (searchDto.UpdatedFrom.HasValue)
            query = query.Where(t => t.UpdatedAt >= searchDto.UpdatedFrom.Value);

        if (searchDto.UpdatedTo.HasValue)
            query = query.Where(t => t.UpdatedAt <= searchDto.UpdatedTo.Value);

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = searchDto.SortBy?.ToLower() switch
        {
            "updatedat" => searchDto.SortOrder == "asc" ? query.OrderBy(t => t.UpdatedAt) : query.OrderByDescending(t => t.UpdatedAt),
            "priority" => searchDto.SortOrder == "asc" ? query.OrderBy(t => t.Priority) : query.OrderByDescending(t => t.Priority),
            "status" => searchDto.SortOrder == "asc" ? query.OrderBy(t => t.Status) : query.OrderByDescending(t => t.Status),
            _ => searchDto.SortOrder == "asc" ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt)
        };

        // Apply pagination
        var tickets = await query
            .Skip((searchDto.PageNumber - 1) * searchDto.PageSize)
            .Take(searchDto.PageSize)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Attachments)
            .ToListAsync();

        return (tickets, totalCount);
    }
}
