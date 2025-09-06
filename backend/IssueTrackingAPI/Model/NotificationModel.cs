using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IssueTrackingAPI.Model;

public class NotificationModel
{
    public int Id { get; set; }

    [ForeignKey("User")]
    public int UserId { get; set; }
    public UserModel User { get; set; }

    [ForeignKey("Ticket")]
    public int? TicketId { get; set; }
    public TicketModel? Ticket { get; set; }

    [Required, MaxLength(50)]
    public required string Type { get; set; } // "TicketCreated", "TicketUpdated", "TicketAssigned", "TicketResolved"

    [Required, MaxLength(500)]
    public required string Message { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
