using IssueTrackingAPI.DTO.DashboardDTO;

namespace IssueTrackingAPI.Repository.DashboardRepo.DashboardRepo;

public interface IDashboardRepo
{
    // Dashboard Statistics
    Task<DashboardStats_DTO> GetDashboardStatsAsync(string? fromDate = null, string? toDate = null);
    Task<List<TicketTrend_DTO>> GetTicketTrendsAsync(int days = 30);
    Task<List<RepresentativePerformance_DTO>> GetRepresentativePerformanceAsync();
    
    // Individual Statistics
    Task<int> GetTotalTicketsCountAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTicketsCountByStatusAsync(string status, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTicketsCountByPriorityAsync(string priority, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetUsersCountByRoleAsync(string role);
    Task<int> GetRecentTicketsCountAsync(int days = 7);
    Task<double> GetAverageResolutionTimeAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    // Overloaded methods for basic statistics without date filtering
    Task<int> GetTotalTicketsCountAsync();
    Task<int> GetTicketsCountByStatusAsync(string status);
    Task<int> GetTicketsCountByPriorityAsync(string priority);
    Task<double> GetAverageResolutionTimeAsync();
}
