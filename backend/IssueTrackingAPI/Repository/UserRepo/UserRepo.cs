using IssueTrackingAPI.Context;
using IssueTrackingAPI.Model;
using Microsoft.EntityFrameworkCore;

namespace IssueTrackingAPI.Repository.UserRepo.UserRepo;

public class UserRepo : IUserRepo
{
    private readonly AppDBContext _context;

    public UserRepo(AppDBContext context)
    {
        _context = context;
    }

    // Get All Users 
    public async Task<IEnumerable<UserModel>> GetAllUsers()
    {
        return await _context.Users_Table.ToListAsync();
    }

    // Get User By Id
    public async Task<UserModel?> GetUserById(int id)
    {
        return await _context.Users_Table
            .Include(u => u.CreatedTickets)
            .Include(u => u.AssignedTickets)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    // Get User By Email
    public async Task<UserModel?> GetUserByEmail(string email)
    {
        return await _context.Users_Table.FirstOrDefaultAsync(u => u.Email == email);
    }

    // Add User
    public async Task<UserModel> AddUser(UserModel user)
    {
        _context.Users_Table.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    // Update User
    public async Task<UserModel?> UpdateUser(UserModel user)
    {
        var existingUser = await _context.Users_Table.FindAsync(user.Id);
        if (existingUser == null) return null;

        existingUser.Name = user.Name;
        existingUser.Email = user.Email;
        existingUser.PasswordHash = user.PasswordHash;
        existingUser.Role = user.Role;

        await _context.SaveChangesAsync();
        return existingUser;
    }

    // Delete User
    public async Task<bool> DeleteUser(int id)
    {
        var user = await _context.Users_Table.FindAsync(id);
        if (user == null) return false;

        _context.Users_Table.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }
}
