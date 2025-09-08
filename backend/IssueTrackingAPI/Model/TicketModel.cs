using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net.Mail;

namespace IssueTrackingAPI.Model;

public class TicketModel
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public required string Title { get; set; }

    [Required]
    public required string Description { get; set; }

    [Required]
    public required string Priority { get; set; } // "Low", "Medium", "High"

    [Required]
    public required string Type { get; set; } // "Software", "Hardware"

    [Required]
    public required string Status { get; set; } = "Open"; // default "Open"

    [ForeignKey("CreatedByUser")]
    public int CreatedByUserId { get; set; }
    public UserModel CreatedByUser { get; set; }

    [ForeignKey("AssignedToUser")]
    public int? AssignedToUserId { get; set; } // nullable
    public UserModel AssignedToUser { get; set; }

    public string? Comment { get; set; } // Comment

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Ticket Created Date Time
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow; // Ticket Updated Date Time

    // KPI Fields
    public DateTime? ResolvedAt { get; set; } // When ticket was marked as resolved
    public TimeSpan? ResolutionTime { get; set; } // Calculated resolution time
    public string? ResolutionNotes { get; set; } // Notes about resolution

    // Navigation properties (attachments removed)
    public ICollection<AttachmentModel> Attachments { get; set; } = new List<AttachmentModel>();
}
