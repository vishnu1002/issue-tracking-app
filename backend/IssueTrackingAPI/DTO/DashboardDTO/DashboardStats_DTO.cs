namespace IssueTrackingAPI.DTO.DashboardDTO;

public class DashboardStats_DTO
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int HighPriorityTickets { get; set; }
    public int TotalUsers { get; set; }
    public int TotalRepresentatives { get; set; }
    public int TotalAdmins { get; set; }
    public int RecentTickets { get; set; } // Last 7 days
    public double AverageResolutionTime { get; set; } // In hours
    public List<TicketTrend_DTO> TicketTrends { get; set; } = new();
    public List<RepresentativePerformance_DTO> TopPerformers { get; set; } = new();
}

public class TicketTrend_DTO
{
    public DateTime Date { get; set; }
    public int Created { get; set; }
    public int Resolved { get; set; }
}

public class RepresentativePerformance_DTO
{
    public int RepresentativeId { get; set; }
    public string RepresentativeName { get; set; } = string.Empty;
    public string RepresentativeEmail { get; set; } = string.Empty;
    public int TicketsAssigned { get; set; }
    public int TicketsResolved { get; set; }
    public int TicketsClosed { get; set; }
    public double ResolutionRate { get; set; }
    public double AverageResolutionTime { get; set; }
}
