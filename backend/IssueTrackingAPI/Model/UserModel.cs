using System.ComponentModel.DataAnnotations;
using System.Net.Sockets;

namespace IssueTrackingAPI.Model;

public class UserModel
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public required string Name { get; set; }

    [Required, EmailAddress, MaxLength(150)]
    public required string Email { get; set; }

    [Required]
    public required string PasswordHash { get; set; } // hashed password only

    [Required]
    public required string Role { get; set; } // ["User", "Representative", "Admin"]

    public DateTime CreatedAt { get; set; } = IssueTrackingAPI.Context.TimeHelper.NowIst();

    // Navigation properties
    public ICollection<TicketModel> CreatedTickets { get; set; } = new List<TicketModel>();
    public ICollection<TicketModel> AssignedTickets { get; set; } = new List<TicketModel>();
}
