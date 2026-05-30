using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Models;
using MentraAI.API.Modules.Users.Repositories;
using System.Text.Json;

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

        if (request.Age is not null) profile.Age = request.Age;
        if (request.EdLevel is not null) profile.EdLevel = request.EdLevel;
        if (request.YearsCode.HasValue) profile.YearsCode = request.YearsCode;
        if (request.WorkExp.HasValue) profile.WorkExp = request.WorkExp;
        if (request.Employment is not null) profile.Employment = request.Employment;
        if (request.RemoteWork is not null) profile.RemoteWork = request.RemoteWork;
        if (request.Industry is not null) profile.Industry = request.Industry;
        if (request.OrgSize is not null) profile.OrgSize = request.OrgSize;
        if (request.AISelect is not null) profile.AISelect = request.AISelect;

        if (request.CurrentSkills is not null)
            profile.CurrentSkillsJson = JsonSerializer.Serialize(request.CurrentSkills);

        if (request.FutureSkills is not null)
            profile.FutureSkillsJson = JsonSerializer.Serialize(request.FutureSkills);

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
        if (data.Age is not null) profile.Age = data.Age;
        if (data.EdLevel is not null) profile.EdLevel = data.EdLevel;
        if (data.YearsCode.HasValue) profile.YearsCode = data.YearsCode;
        if (data.WorkExp.HasValue) profile.WorkExp = data.WorkExp;
        if (data.Employment is not null) profile.Employment = data.Employment;
        if (data.RemoteWork is not null) profile.RemoteWork = data.RemoteWork;
        if (data.Industry is not null) profile.Industry = data.Industry;
        if (data.OrgSize is not null) profile.OrgSize = data.OrgSize;
        if (data.AISelect is not null) profile.AISelect = data.AISelect;

        if (data.CurrentSkillsJson is not null) profile.CurrentSkillsJson = data.CurrentSkillsJson;
        if (data.FutureSkillsJson is not null) profile.FutureSkillsJson = data.FutureSkillsJson;

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

    public async Task<UserProfile?> GetProfileEntityAsync(string userId)
    {
        return await _repo.GetProfileByUserIdAsync(userId);
    }
}