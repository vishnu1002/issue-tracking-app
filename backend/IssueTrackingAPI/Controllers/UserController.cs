using IssueTrackingAPI.DTO.UserDTO;
using IssueTrackingAPI.Model;
using IssueTrackingAPI.Repository.UserRepo.UserRepo;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace IssueTrackingAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
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
    [HttpGet("{id}")]
    public async Task<ActionResult<UserRead_DTO>> GetUserById(int id)
    {
        var user = await _userRepo.GetUserById(id);
        if (user == null) return NotFound(new { message = "User not found" });

        var userDto = new UserRead_DTO
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };

        return Ok(userDto);
    }

    //
    // Add User
    // POST: api/users
    //
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
    [HttpPut("{id}")]
    public async Task<ActionResult<UserRead_DTO>> UpdateUser(int id, [FromBody] UserUpdate_DTO dto)
    {
        if (id != dto.Id) return BadRequest(new { message = "User ID mismatch" });

        var existingUser = await _userRepo.GetUserById(id);
        if (existingUser == null) return NotFound(new { message = "User not found" });

        existingUser.Name = dto.Name;
        existingUser.Email = dto.Email;
        existingUser.Role = dto.Role;

        var updatedUser = await _userRepo.UpdateUser(existingUser);

        var updatedDto = new UserRead_DTO
        {
            Id = updatedUser.Id,
            Name = updatedUser.Name,
            Email = updatedUser.Email,
            Role = updatedUser.Role,
            CreatedAt = updatedUser.CreatedAt
        };

        return Ok(updatedDto);
    }

    //
    // Delete User
    // DELETE: api/users/{id}
    //
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var deleted = await _userRepo.DeleteUser(id);
        if (!deleted) return NotFound(new { message = "User not found" });

        return NoContent();
    }
}
