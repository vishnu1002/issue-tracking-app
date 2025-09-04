namespace IssueTrackingAPI.DTO.UserDTO;

public class UserRead_DTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime CreatedAt { get; set; }
}
