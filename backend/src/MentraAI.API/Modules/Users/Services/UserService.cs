using System.Text.Json;
using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Repositories;

namespace MentraAI.API.Modules.Users.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _repo;
    private readonly IMapper _mapper;

    public UserService(IUserRepository repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    // =====================================================================
    // GET PROFILE
    // =====================================================================
    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var user = await _repo.GetByIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User not found.", 404);

        var profile = await _repo.GetProfileByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User profile not found.", 404);

        return _mapper.Map<UserProfileResponse>((user, profile));
    }

    // =====================================================================
    // UPDATE PROFILE (frontend PUT /users/me)
    // =====================================================================
    public async Task<UserProfileResponse> UpdateProfileAsync(
        string userId, UpdateProfileRequest request)
    {
        var user = await _repo.GetByIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User not found.", 404);

        var profile = await _repo.GetProfileByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User profile not found.", 404);

        if (request.Background is not null) profile.Background = request.Background;
        if (request.CareerGoals is not null) profile.CareerGoals = request.CareerGoals;
        if (request.WeeklyHours is not null) profile.WeeklyHours = request.WeeklyHours;

        if (request.CurrentSkills is not null)
            profile.CurrentSkillsJson = JsonSerializer.Serialize(request.CurrentSkills);

        if (request.Interests is not null)
            profile.InterestsJson = JsonSerializer.Serialize(request.Interests);

        profile.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateProfileAsync(profile);

        return _mapper.Map<UserProfileResponse>((user, profile));
    }

    // =====================================================================
    // ONBOARDING HELPERS
    // =====================================================================

    public async Task<bool> GetIsOnboardedAsync(string userId)
    {
        var profile = await _repo.GetProfileByUserIdAsync(userId);
        return profile?.IsOnboarded ?? false;
    }

    public async Task UpdateProfileFromAnswersAsync(string userId, ProfileUpdateData data)
    {
        var profile = await _repo.GetProfileByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "Profile not found.", 404);

        // Only overwrite fields that were actually submitted
        if (data.Background is not null) profile.Background = data.Background;
        if (data.WeeklyHours is not null) profile.WeeklyHours = data.WeeklyHours;
        if (data.CurrentSkillsJson is not null) profile.CurrentSkillsJson = data.CurrentSkillsJson;
        if (data.InterestsJson is not null) profile.InterestsJson = data.InterestsJson;
        if (data.CareerGoals is not null) profile.CareerGoals = data.CareerGoals;

        profile.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateProfileAsync(profile);
    }

    public async Task SetOnboardedAsync(string userId)
    {
        var profile = await _repo.GetProfileByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "Profile not found.", 404);

        profile.IsOnboarded = true;
        profile.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateProfileAsync(profile);
    }
}