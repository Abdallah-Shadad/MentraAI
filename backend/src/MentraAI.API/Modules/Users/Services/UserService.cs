using System.Text.Json;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.DTOs.Responses;
using MentraAI.API.Modules.Users.Repositories;
using AutoMapper;

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

    public async Task<UserProfileResponse> GetProfileAsync(string userId)
    {
        var user = await _repo.GetByIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User not found.", 404);

        var profile = await _repo.GetProfileByUserIdAsync(userId)
            ?? throw new AppException(ErrorCodes.NOT_FOUND, "User profile not found.", 404);

        return _mapper.Map<UserProfileResponse>((user, profile));
    }

    public async Task<UserProfileResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request)
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
}