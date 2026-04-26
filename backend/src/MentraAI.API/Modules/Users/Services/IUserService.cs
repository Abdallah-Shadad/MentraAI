using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Models;

namespace MentraAI.API.Modules.Users.Services;
public interface IUserService
{
    // == Frontend-facing ==
    Task<UserProfileResponse> GetProfileAsync(string userId);
    Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request);

    // == Called by Onboarding module only ==
    Task<bool> GetIsOnboardedAsync(string userId);
    Task UpdateProfileFromAnswersAsync(string userId, ProfileUpdateData data);
    Task SetOnboardedAsync(string userId);
    Task<UserProfile?> GetProfileEntityAsync(string userId);
}