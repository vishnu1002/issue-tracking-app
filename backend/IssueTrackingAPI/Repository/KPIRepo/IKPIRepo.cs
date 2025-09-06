using IssueTrackingAPI.DTO.DashboardDTO;

namespace IssueTrackingAPI.Repository.KPIRepo.KPIRepo;

public interface IKPIRepo
{
    // Representative KPI Methods
    Task<RepresentativePerformance_DTO> GetRepresentativeKPIAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<List<RepresentativePerformance_DTO>> GetAllRepresentativesKPIAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    // Resolution Time Methods
    Task<double> GetAverageResolutionTimeAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTotalTicketsResolvedAsync(DateTime? fromDate = null, DateTime? toDate = null);
    
    // Individual KPI Methods
    Task<int> GetTicketsAssignedToRepresentativeAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<int> GetTicketsResolvedByRepresentativeAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null);
    Task<double> GetResolutionRateByRepresentativeAsync(int representativeId, DateTime? fromDate = null, DateTime? toDate = null);
}
