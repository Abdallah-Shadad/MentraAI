using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.Users.Repositories;

public interface IUserRepository
{
    Task<ApplicationUser?> GetByIdAsync(string userId);
    Task<UserProfile?> GetProfileByUserIdAsync(string userId);
    Task UpdateProfileAsync(UserProfile profile);
}