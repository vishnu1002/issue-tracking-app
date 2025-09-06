using IssueTrackingAPI.DTO.DashboardDTO;

namespace IssueTrackingAPI.Repository.DashboardRepo.DashboardRepo;

public interface IDashboardRepo
{
    // Dashboard Statistics
    Task<DashboardStats_DTO> GetDashboardStatsAsync();
    Task<List<TicketTrend_DTO>> GetTicketTrendsAsync(int days = 30);
    Task<List<RepresentativePerformance_DTO>> GetRepresentativePerformanceAsync();
    
    // Individual Statistics
    Task<int> GetTotalTicketsCountAsync();
    Task<int> GetTicketsCountByStatusAsync(string status);
    Task<int> GetTicketsCountByPriorityAsync(string priority);
    Task<int> GetTotalUsersCountAsync();
    Task<int> GetUsersCountByRoleAsync(string role);
    Task<int> GetRecentTicketsCountAsync(int days = 7);
    Task<double> GetAverageResolutionTimeAsync();
}
