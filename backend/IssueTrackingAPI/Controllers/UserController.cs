using BCrypt.Net;
using IssueTrackingAPI.DTO.UserDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.UserRepo.UserRepo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Security.Claims;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/user")]
public class UserController : ControllerBase
{
    private readonly IUserRepo _userRepo;

    public UserController(IUserRepo userRepo)
    {
        _userRepo = userRepo;
    }

    // 
    // Get All Users
    // GET: api/users
    //
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserRead_DTO>>> GetAllUsers()
    {
        var users = await _userRepo.GetAllUsers();
        var userDtos = users.Select(u => new UserRead_DTO
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        });

        return Ok(userDtos);
    }

    // 
    // Get Representatives
    // GET: api/users/representatives
    //
    [Authorize]
    [HttpGet("representatives")]
    public async Task<ActionResult<IEnumerable<UserRead_DTO>>> GetRepresentatives()
    {
        var users = await _userRepo.GetAllUsers();
        var representatives = users.Where(u => u.Role == "Rep").Select(u => new UserRead_DTO
        {
            Id = u.Id,
            Name = u.Name,
            Email = u.Email,
            Role = u.Role,
            CreatedAt = u.CreatedAt
        });

        return Ok(representatives);
    }

    //
    // Get User By Id
    // GET: api/users/{id}
    //
    [Authorize]
    [HttpGet("{id}")]
    public async Task<ActionResult<UserRead_DTO>> GetUserById(int id)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        var user = await _userRepo.GetUserById(id);
        if (user == null) return NotFound(new { message = "User not found" });

        var userDto = new UserRead_DTO
        {
            Id = user.Id,
            Name = user.Name,
            Role = user.Role,
            CreatedAt = user.CreatedAt,
            Email = currentRole == Roles.Admin || currentUserId == id ? user.Email : null // hide email for other users
        };

        // Users can only see themselves
        if (currentRole != Roles.Admin && currentUserId != id)
            return Forbid();

        return Ok(userDto);
    }

    //
    // Add User
    // POST: api/users
    //
    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<UserRead_DTO>> CreateUser([FromBody] UserCreate_DTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = new UserModel
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Password Hashing using BCrypt
            Role = dto.Role,
            CreatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepo.AddUser(user);

        var createdDto = new UserRead_DTO
        {
            Id = createdUser.Id,
            Name = createdUser.Name,
            Email = createdUser.Email,
            Role = createdUser.Role,
            CreatedAt = createdUser.CreatedAt
        };

        return CreatedAtAction(nameof(GetUserById), new { id = createdDto.Id }, createdDto);
    }

    //
    // Update User
    // PUT: api/users/{id}
    //
    [Authorize]
    [HttpPut("{id}")]
    public async Task<ActionResult<UserRead_DTO>> UpdateUser(int id, [FromBody] UserUpdate_DTO dto)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        // User can only update themselves
        if (currentRole != "Admin" && currentUserId != id)
            return Forbid();

        var existingUser = await _userRepo.GetUserById(id);
        if (existingUser == null) return NotFound(new { message = "User not found" });

        existingUser.Name = dto.Name;
        existingUser.Email = dto.Email;

        if (currentRole == "Admin")
            existingUser.Role = dto.Role; // only admin can change roles

        var updatedUser = await _userRepo.UpdateUser(existingUser);

        return Ok(new UserRead_DTO
        {
            Id = updatedUser.Id,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Role = updatedUser.Role,
            CreatedAt = updatedUser.CreatedAt
        });
    }

    //
    // Update Password
    // PUT: api/users/{id}/password
    //
    [Authorize]
    [HttpPut("{id}/password")]
    public async Task<IActionResult> UpdatePassword(int id, [FromBody] UpdatePasswordRequest request)
    {
        var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
        var currentRole = User.FindFirst(ClaimTypes.Role).Value;

        // User can only update their own password
        if (currentRole != "Admin" && currentUserId != id)
            return Forbid();

        var user = await _userRepo.GetUserById(id);
        if (user == null) return NotFound(new { message = "User not found" });

        // Verify current password
        bool isValidPassword = false;
        try
        {
            isValidPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        }
        catch
        {
            // Fallback: compare plaintext (dummy data case)
            isValidPassword = request.CurrentPassword == user.PasswordHash;
        }

        if (!isValidPassword)
            return BadRequest(new { message = "Current password is incorrect" });

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await _userRepo.UpdateUser(user);

        return Ok(new { message = "Password updated successfully" });
    }

    //
    // Delete User
    // DELETE: api/users/{id}
    //
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        // First check if user exists
        var user = await _userRepo.GetUserById(id);
        if (user == null) return NotFound(new { message = "User not found" });

        // Check if user has any tickets
        if (user.CreatedTickets.Any() || user.AssignedTickets.Any())
        {
            return BadRequest(new { 
                message = "Cannot delete user. User has associated tickets. Please reassign or delete the tickets first.",
                hasCreatedTickets = user.CreatedTickets.Any(),
                hasAssignedTickets = user.AssignedTickets.Any(),
                createdTicketsCount = user.CreatedTickets.Count,
                assignedTicketsCount = user.AssignedTickets.Count
            });
        }

        var deleted = await _userRepo.DeleteUser(id);
        if (!deleted) return StatusCode(500, new { message = "Failed to delete user" });

        return Ok(new { message = "User deleted successfully" });
    }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Rep = "Rep";
}

public class UpdatePasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}
