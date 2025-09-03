using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace IssueTrackingAPI.Model;

public class UserModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; }

    [Required]
    public string PasswordHash { get; set; } // hashed password only

    [Required]
    public string Role { get; set; } // ["User", "Representative", "Admin"]

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<TicketModel> CreatedTickets { get; set; }
    public ICollection<TicketModel> AssignedTickets { get; set; }
}
