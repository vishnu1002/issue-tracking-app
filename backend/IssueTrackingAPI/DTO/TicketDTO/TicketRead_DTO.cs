namespace IssueTrackingAPI.DTO.TicketDTO;

public class TicketRead_DTO
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string Priority { get; set; }
    public string Type { get; set; }
    public string Status { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserEmail { get; set; }
    public int? AssignedToUserId { get; set; }
    public string? AssignedToUserEmail { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public TimeSpan? ResolutionTime { get; set; }
    public string? ResolutionNotes { get; set; }
}
