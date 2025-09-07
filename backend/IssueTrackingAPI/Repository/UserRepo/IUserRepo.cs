using IssueTrackingAPI.Model;

namespace IssueTrackingAPI.Repository.UserRepo.UserRepo;

public interface IUserRepo
{
    // Async Methods
    Task<IEnumerable<UserModel>> GetAllUsers(); // Iterating over collection
    Task<UserModel?> GetUserById(int id);
    Task<UserModel?> GetUserByEmail(string email);
    Task<UserModel> AddUser(UserModel user);
    Task<UserModel?> UpdateUser(UserModel user);
    Task<bool> DeleteUser(int id);
    Task<bool> DeleteUserWithReassignment(int id, int? reassignToUserId = null);
}
