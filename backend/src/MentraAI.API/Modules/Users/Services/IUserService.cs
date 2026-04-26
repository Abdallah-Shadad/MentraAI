using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.DTOs.Responses;

namespace MentraAI.API.Modules.Users.Services
{
    public interface IUserService
    {
        Task<UserProfileResponse> GetProfileAsync (string userId);
        Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);
    }
}
