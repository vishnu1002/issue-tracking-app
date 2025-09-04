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
        var currentUserId = int.Parse(User.FindFirst("id").Value);
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
    // Delete User
    // DELETE: api/users/{id}
    //
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _userRepo.DeleteUser(id);
        if (!deleted) return NotFound(new { message = "User not found" });

        return NoContent();
    }
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Rep = "Rep";
}
