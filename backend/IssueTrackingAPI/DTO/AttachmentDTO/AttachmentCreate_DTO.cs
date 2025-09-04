using System.ComponentModel.DataAnnotations;

namespace IssueTrackingAPI.DTO.AttachmentDTO;

public class AttachmentCreate_DTO
{
    [Required]
    public int TicketId { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; }

    [Required]
    public string FileUrl { get; set; }
}
