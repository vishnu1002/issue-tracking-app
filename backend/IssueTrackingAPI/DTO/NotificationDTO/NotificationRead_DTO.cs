namespace IssueTrackingAPI.DTO.NotificationDTO;

public class NotificationRead_DTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? TicketId { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? TicketTitle { get; set; }
}
