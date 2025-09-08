using IssueTrackingAPI.Context;
using IssueTrackingAPI.DTO.DashboardDTO;
using Microsoft.EntityFrameworkCore;

namespace IssueTrackingAPI.Repository.DashboardRepo.DashboardRepo;

public class DashboardRepo : IDashboardRepo
{
    private readonly AppDBContext _context;

    public DashboardRepo(AppDBContext context)
    {
        _context = context;
    }

    // Get Dashboard Stats
    public async Task<DashboardStats_DTO> GetDashboardStatsAsync(string? fromDate = null, string? toDate = null)
    {
        var stats = new DashboardStats_DTO();

        // Parse date parameters
        DateTime? fromDateTime = null;
        DateTime? toDateTime = null;
        
        if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var parsedFromDate))
        {
            fromDateTime = parsedFromDate;
        }
        
        if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var parsedToDate))
        {
            toDateTime = parsedToDate;
        }

        // Ticket statistics (show current state of all tickets, not filtered by creation date)
        stats.TotalTickets = await GetTotalTicketsCountAsync();
        stats.OpenTickets = await GetTicketsCountByStatusAsync("Open");
        stats.ClosedTickets = await GetTicketsCountByStatusAsync("Closed");
        stats.InProgressTickets = await GetTicketsCountByStatusAsync("In Progress");
        stats.HighPriorityTickets = await GetTicketsCountByPriorityAsync("High");

        // User statistics (these don't need date filtering)
        stats.TotalUsers = await GetTotalUsersCountAsync();
        stats.TotalRepresentatives = await GetUsersCountByRoleAsync("Rep");
        stats.TotalAdmins = await GetUsersCountByRoleAsync("Admin");

        // Recent tickets (last 7 days)
        stats.RecentTickets = await GetRecentTicketsCountAsync(7);

        // Average resolution time (for closed tickets)
        stats.AverageResolutionTime = await GetAverageResolutionTimeAsync(fromDateTime, toDateTime);

        // Get trends and performance data
        stats.TicketTrends = await GetTicketTrendsAsync(30);
        stats.TopPerformers = await GetRepresentativePerformanceAsync();

        return stats;
    }

    // Get Ticket Trends
    public async Task<List<TicketTrend_DTO>> GetTicketTrendsAsync(int days = 30)
    {
        var startDate = IssueTrackingAPI.Context.TimeHelper.NowIst().AddDays(-days);
        var trends = new List<TicketTrend_DTO>();
        
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var nextDate = date.AddDays(1);
            
            var created = await _context.Tickets_Table
                .CountAsync(t => t.CreatedAt >= date && t.CreatedAt < nextDate);
                
            var resolved = await _context.Tickets_Table
                .CountAsync(t => t.Status == "Closed" && 
                               t.UpdatedAt >= date.Date && 
                               t.UpdatedAt < nextDate.Date);
            
            trends.Add(new TicketTrend_DTO
            {
                Date = date,
                Created = created,
                Resolved = resolved
            });
        }
        
        return trends;
    }

    // Get Representative Performance
    public async Task<List<RepresentativePerformance_DTO>> GetRepresentativePerformanceAsync()
    {
        var representatives = await _context.Users_Table
            .Where(u => u.Role == "Rep")
            .ToListAsync();

        var performance = new List<RepresentativePerformance_DTO>();

        foreach (var rep in representatives)
        {
            var assignedTickets = await _context.Tickets_Table
                .Where(t => t.AssignedToUserId == rep.Id)
                .ToListAsync();

            var resolvedTickets = assignedTickets.Where(t => t.Status == "Closed").ToList();

            var resolutionRate = assignedTickets.Any() 
                ? (double)resolvedTickets.Count / assignedTickets.Count * 100 
                : 0;

            var avgResolutionTime = resolvedTickets.Any()
                ? resolvedTickets.Where(t => t.ResolutionTime.HasValue).Average(t => t.ResolutionTime!.Value.TotalHours)
                : 0;

            performance.Add(new RepresentativePerformance_DTO
            {
                RepresentativeId = rep.Id,
                RepresentativeName = rep.Name,
                TicketsAssigned = assignedTickets.Count,
                TicketsResolved = resolvedTickets.Count,
                ResolutionRate = resolutionRate,
                AverageResolutionTime = avgResolutionTime
            });
        }

        return performance.OrderByDescending(p => p.ResolutionRate).Take(10).ToList();
    }

    // Get Total Tickets Count
    public async Task<int> GetTotalTicketsCountAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table.AsQueryable();
        
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);
            
        return await query.CountAsync();
    }

    // Get Total Tickets Count (overload without date filtering)
    public async Task<int> GetTotalTicketsCountAsync()
    {
        return await _context.Tickets_Table.CountAsync();
    }

    // Get Tickets Count By Status
    public async Task<int> GetTicketsCountByStatusAsync(string status, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table.Where(t => t.Status == status);
        
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);
            
        return await query.CountAsync();
    }

    // Get Tickets Count By Status (overload without date filtering)
    public async Task<int> GetTicketsCountByStatusAsync(string status)
    {
        return await _context.Tickets_Table.CountAsync(t => t.Status == status);
    }

    // Get Tickets Count By Priority
    public async Task<int> GetTicketsCountByPriorityAsync(string priority, DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table.Where(t => t.Priority == priority);
        
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value);
            
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value);
            
        return await query.CountAsync();
    }

    // Get Tickets Count By Priority (overload without date filtering)
    public async Task<int> GetTicketsCountByPriorityAsync(string priority)
    {
        return await _context.Tickets_Table.CountAsync(t => t.Priority == priority);
    }

    // Get Total Users Count
    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users_Table.CountAsync();
    }

    // Get Users Count By Role
    public async Task<int> GetUsersCountByRoleAsync(string role)
    {
        return await _context.Users_Table.CountAsync(u => u.Role == role);
    }

    // Get Recent Tickets Count
    public async Task<int> GetRecentTicketsCountAsync(int days = 7)
    {
        var dateFrom = IssueTrackingAPI.Context.TimeHelper.NowIst().AddDays(-days).Date;
        return await _context.Tickets_Table.CountAsync(t => t.CreatedAt >= dateFrom);
    }

    // Get Average Resolution Time
    public async Task<double> GetAverageResolutionTimeAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        var query = _context.Tickets_Table.Where(t => t.Status == "Closed");
        
        if (fromDate.HasValue)
            query = query.Where(t => t.CreatedAt >= fromDate.Value.Date);
            
        if (toDate.HasValue)
            query = query.Where(t => t.CreatedAt <= toDate.Value.Date);
            
        var closedTickets = await query.ToListAsync();

        if (!closedTickets.Any()) return 0;

        return closedTickets
            .Where(t => t.ResolutionTime.HasValue)
            .Average(t => t.ResolutionTime!.Value.TotalHours);
    }

    // Get Average Resolution Time (overload without date filtering)
    public async Task<double> GetAverageResolutionTimeAsync()
    {
        var closedTickets = await _context.Tickets_Table
            .Where(t => t.Status == "Closed")
            .ToListAsync();

        if (!closedTickets.Any()) return 0;

        return closedTickets
            .Where(t => t.ResolutionTime.HasValue)
            .Average(t => t.ResolutionTime!.Value.TotalHours);
    }
}
