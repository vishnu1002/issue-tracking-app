namespace IssueTrackingAPI.DTO.TicketDTO;

public class TicketSearch_DTO
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Priority { get; set; } // "Low", "Medium", "High"
    public string? Type { get; set; } // "Software", "Hardware"
    public string? Status { get; set; } // "Open", "Closed", "In Progress"
    public int? CreatedByUserId { get; set; }
    public int? AssignedToUserId { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortBy { get; set; } = "CreatedAt"; // "CreatedAt", "UpdatedAt", "Priority", "Status"
    public string? SortOrder { get; set; } = "desc"; // "asc", "desc"
}
