using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTrackingAPI.Model;

public class AttachmentModel
{
    public int Id { get; set; }

    [Required]
    public required string FileName { get; set; }

    [Required]
    public required string StoredFileName { get; set; }

    [Required]
    public required string ContentType { get; set; }

    public long FileSizeBytes { get; set; }

    [Required]
    public required string FilePath { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int UploadedByUserId { get; set; }

    [ForeignKey("Ticket")]
    public int TicketId { get; set; }
    public TicketModel Ticket { get; set; }
}


