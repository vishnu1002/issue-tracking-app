namespace IssueTrackingAPI.DTO.AttachmentDTO;

public class AttachmentRead_DTO
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string FileName { get; set; }
    public string FileUrl { get; set; }
    public DateTime UploadedAt { get; set; }
}
