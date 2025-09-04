using System.ComponentModel.DataAnnotations;

namespace IssueTrackingAPI.DTO.UserDTO;

public class UserUpdate_DTO
{
    [Required]
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; }

    [Required]
    public string Role { get; set; } // allow updating role, but **not** password here
}
