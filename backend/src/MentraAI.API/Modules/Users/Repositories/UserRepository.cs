using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.Auth.Models;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.Users.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

    public async Task<UserProfile?> GetProfileByUserIdAsync(string userId) =>
        await _db.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

    public async Task UpdateProfileAsync(UserProfile profile)
    {
        _db.UserProfiles.Update(profile);
        await _db.SaveChangesAsync();
    }
}