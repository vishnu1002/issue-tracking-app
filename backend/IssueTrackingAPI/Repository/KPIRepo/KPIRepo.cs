using IssueTrackingAPI.Context;
using IssueTrackingAPI.DTO.DashboardDTO;
using Microsoft.EntityFrameworkCore;

namespace IssueTrackingAPI.Repository.KPIRepo.KPIRepo;

public class KPIRepo : IKPIRepo
{
    private readonly AppDBContext _context;

    public KPIRepo(AppDBContext context)
    {
        _context = context;
    }

    // Get Representative KPI
    public async Task<RepresentativePerformance_DTO> GetRepresentativeKPIAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        try
        {
            var query = _context.Tickets_Table
                .Where(t => t.AssignedToUserId == representativeId);

            if (fromDate.HasValue)
                query = query.Where(t => t.CreatedAt >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(t => t.CreatedAt <= toDate.Value);

            var tickets = await query.ToListAsync();
            // For representative KPI, count tickets resolved as those Closed while assigned to the rep
            var resolvedTickets = tickets
                .Where(t => t.Status != null && t.Status.ToLower().Replace(" ", "") == "closed")
                .ToList();

            var representative = await _context.Users_Table
                .FirstOrDefaultAsync(u => u.Id == representativeId);

            var resolutionRate = tickets.Any() 
                ? (double)resolvedTickets.Count / tickets.Count * 100 
                : 0;

            var avgResolutionTime = resolvedTickets.Any()
                ? resolvedTickets.Select(t =>
                {
                    if (t.ResolutionTime.HasValue) return t.ResolutionTime.Value.TotalHours;
                    var end = t.ResolvedAt ?? t.UpdatedAt;
                    var duration = end - t.CreatedAt;
                    if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;
                    return duration.TotalHours;
                }).Average()
                : 0;

            return new RepresentativePerformance_DTO
            {
                RepresentativeId = representativeId,
                RepresentativeName = representative?.Name ?? "Unknown",
                RepresentativeEmail = representative?.Email ?? string.Empty,
                TicketsAssigned = tickets.Count,
                TicketsResolved = resolvedTickets.Count,
                TicketsClosed = resolvedTickets.Count,
                ResolutionRate = resolutionRate,
                AverageResolutionTime = avgResolutionTime
            };
        }
        catch
        {
            return new RepresentativePerformance_DTO
            {
                RepresentativeId = representativeId,
                RepresentativeName = "Unknown",
                RepresentativeEmail = string.Empty,
                TicketsAssigned = 0,
                TicketsResolved = 0,
                TicketsClosed = 0,
                ResolutionRate = 0,
                AverageResolutionTime = 0
            };
        }
    }

    // Get All Representatives KPI
    public async Task<List<RepresentativePerformance_DTO>> GetAllRepresentativesKPIAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var representatives = await _context.Users_Table
            .Where(u => u.Role == "Rep")
            .ToListAsync();

        var performanceList = new List<RepresentativePerformance_DTO>();

        foreach (var rep in representatives)
        {
            var kpi = await GetRepresentativeKPIAsync(rep.Id, fromDate, toDate);
            performanceList.Add(kpi);
        }

        return performanceList.OrderByDescending(p => p.ResolutionRate).ToList();
    }

    // Get Average Resolution Time
    public async Task<double> GetAverageResolutionTimeAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table
            .Where(t => t.Status != null && t.Status.ToLower().Replace(" ", "") == "closed");

        if (fromDate.HasValue)
            query = query.Where(t => t.ResolvedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.ResolvedAt <= toDate.Value);

        var resolvedTickets = await query.ToListAsync();

        if (!resolvedTickets.Any()) return 0;

        var hours = resolvedTickets
            .Select(t =>
            {
                if (t.ResolutionTime.HasValue) return t.ResolutionTime.Value.TotalHours;
                var end = t.ResolvedAt ?? t.UpdatedAt;
                var duration = end - t.CreatedAt;
                if (duration < TimeSpan.Zero) duration = TimeSpan.Zero;
                return duration.TotalHours;
            })
            .Average();

        return hours;
    }

    // Get Total Tickets Resolved
    public async Task<int> GetTotalTicketsResolvedAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        // Total tickets resolved = tickets marked Solved or Resolved
        var query = _context.Tickets_Table
            .Where(t => t.Status != null && (t.Status.ToLower().Replace(" ", "") == "solved" || t.Status.ToLower().Replace(" ", "") == "resolved"));

        if (fromDate.HasValue)
            query = query.Where(t => (t.ResolvedAt ?? t.UpdatedAt) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => (t.ResolvedAt ?? t.UpdatedAt) <= toDate.Value);

        return await query.CountAsync();
    }

    // Get Tickets Assigned To Representative
    public async Task<int> GetTicketsAssignedToRepresentativeAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table
            .Where(t => t.AssignedToUserId == representativeId);

        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);

        return await query.CountAsync();
    }

    // Get Tickets Resolved By Representative
    public async Task<int> GetTicketsResolvedByRepresentativeAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table
            .Where(t => t.AssignedToUserId == representativeId && t.Status != null && t.Status.ToLower().Replace(" ", "") == "closed");

        if (fromDate.HasValue)
            query = query.Where(t => (t.ResolvedAt ?? t.UpdatedAt) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(t => (t.ResolvedAt ?? t.UpdatedAt) <= toDate.Value);

        return await query.CountAsync();
    }

    // Get Resolution Rate By Representative
    public async Task<double> GetResolutionRateByRepresentativeAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var assigned = await GetTicketsAssignedToRepresentativeAsync(representativeId, fromDate, toDate);
        var resolved = await GetTicketsResolvedByRepresentativeAsync(representativeId, fromDate, toDate);

        return assigned > 0 ? (double)resolved / assigned * 100 : 0;
    }
}
