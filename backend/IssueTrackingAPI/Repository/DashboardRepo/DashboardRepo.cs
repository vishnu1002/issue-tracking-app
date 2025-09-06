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
    public async Task<DashboardStats_DTO> GetDashboardStatsAsync()
    {
        var stats = new DashboardStats_DTO();

        // Ticket statistics
        stats.TotalTickets = await GetTotalTicketsCountAsync();
        stats.OpenTickets = await GetTicketsCountByStatusAsync("Open");
        stats.ClosedTickets = await GetTicketsCountByStatusAsync("Closed");
        stats.InProgressTickets = await GetTicketsCountByStatusAsync("In Progress");
        stats.HighPriorityTickets = await GetTicketsCountByPriorityAsync("High");

        // User statistics
        stats.TotalUsers = await GetTotalUsersCountAsync();
        stats.TotalRepresentatives = await GetUsersCountByRoleAsync("Rep");
        stats.TotalAdmins = await GetUsersCountByRoleAsync("Admin");

        // Recent tickets (last 7 days)
        stats.RecentTickets = await GetRecentTicketsCountAsync(7);

        // Average resolution time (for closed tickets)
        stats.AverageResolutionTime = await GetAverageResolutionTimeAsync();

        // Get trends and performance data
        stats.TicketTrends = await GetTicketTrendsAsync(30);
        stats.TopPerformers = await GetRepresentativePerformanceAsync();

        return stats;
    }

    // Get Ticket Trends
    public async Task<List<TicketTrend_DTO>> GetTicketTrendsAsync(int days = 30)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        var trends = new List<TicketTrend_DTO>();
        
        for (int i = 0; i < days; i++)
        {
            var date = startDate.AddDays(i);
            var nextDate = date.AddDays(1);
            
            var created = await _context.Tickets_Table
                .CountAsync(t => t.CreatedAt >= date && t.CreatedAt < nextDate);
                
            var resolved = await _context.Tickets_Table
                .CountAsync(t => t.Status == "Closed" && 
                               t.UpdatedAt >= date && 
                               t.UpdatedAt < nextDate);
            
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
                ? resolvedTickets.Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours)
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
    public async Task<int> GetTotalTicketsCountAsync()
    {
        return await _context.Tickets_Table.CountAsync();
    }

    // Get Tickets Count By Status
    public async Task<int> GetTicketsCountByStatusAsync(string status)
    {
        return await _context.Tickets_Table.CountAsync(t => t.Status == status);
    }

    // Get Tickets Count By Priority
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
        var dateFrom = DateTime.UtcNow.AddDays(-days);
        return await _context.Tickets_Table.CountAsync(t => t.CreatedAt >= dateFrom);
    }

    // Get Average Resolution Time
    public async Task<double> GetAverageResolutionTimeAsync()
    {
        var closedTickets = await _context.Tickets_Table
            .Where(t => t.Status == "Closed")
            .ToListAsync();

        if (!closedTickets.Any()) return 0;

        return closedTickets
            .Where(t => t.UpdatedAt > t.CreatedAt)
            .Average(t => (t.UpdatedAt - t.CreatedAt).TotalHours);
    }
}
