using System.ComponentModel.DataAnnotations;

namespace IssueTrackingAPI.DTO.UserDTO;

public class UserCreate_DTO
{
    [Required, MaxLength(100)]
    public string Name { get; set; }

    [Required, EmailAddress, MaxLength(150)]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; } // plain password, will be hashed inside repo/service

    [Required]
    public string Role { get; set; } // "User", "Representative", "Admin"
}
