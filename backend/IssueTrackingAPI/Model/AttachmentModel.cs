using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Sockets;

namespace IssueTrackingAPI.Model;

public class AttachmentModel
{
    public int Id { get; set; }

    [ForeignKey("Ticket")]
    public int TicketId { get; set; }
    public TicketModel Ticket { get; set; }

    [Required, MaxLength(255)]
    public string FileName { get; set; }

    [Required]
    public string FileUrl { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
