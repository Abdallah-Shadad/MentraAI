using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.DTOs.Requests;
using MentraAI.API.Modules.AIGateway.InternalModels;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.CareerTracks.DTOs.Requests;
using MentraAI.API.Modules.CareerTracks.DTOs.Responses;
using MentraAI.API.Modules.CareerTracks.Mappings;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;
using MentraAI.API.Modules.Users.Services;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace MentraAI.API.Modules.CareerTracks.Services;

public class CareerTrackService : ICareerTrackService
{
    private readonly ICareerTrackRepository _repo;
    private readonly IMapper _mapper;
    private readonly IAIGatewayService _aiGateway;  // NEW — for track recommender
    private readonly IUserService _userService; // NEW — for onboarding gate + skills fallback
    private readonly ILogger<CareerTrackService> _logger;

    public CareerTrackService(
        ICareerTrackRepository repo,
        IMapper mapper,
        IAIGatewayService aiGateway,
        IUserService userService,
        ILogger<CareerTrackService> logger)
    {
        _repo = repo;
        _mapper = mapper;
        _aiGateway = aiGateway;
        _userService = userService;
        _logger = logger;
    }

    // === GET ALL TRACKS ===
    public async Task<CareerTracksListResponse> GetAllTracksAsync()
    {
        var tracks = await _repo.GetAllActiveTracksAsync();
        return new CareerTracksListResponse
        {
            Tracks = _mapper.Map<List<CareerTrackResponse>>(tracks)
        };
    }

    // === GET PREDICTION ===
    public async Task<PredictionResponse> GetPredictionAsync(string userId)
    {
        // 1. Gate: Verify onboarding status from UserService
        var isOnboarded = await _userService.GetIsOnboardedAsync(userId);
        if (!isOnboarded)
        {
            throw new AppException(ErrorCodes.NOT_ONBOARDED,
                "Complete onboarding before viewing your prediction.", 422);
        }

        // 2. Fetch the latest stored AI prediction
        var prediction = await _repo.GetLatestPredictionByUserIdAsync(userId);
        if (prediction is null)
        {
            // If the user has completed onboarding but hasn't run the AI recommendation yet,
            // return a friendly default indicating we are ready for recommendation.
            return new PredictionResponse
            {
                PrimaryRole = new RoleItem { Name = "Ready for Recommendation", Confidence = 0 },
                TopRoles = new List<RoleItem>(),
                PredictedAt = DateTime.UtcNow
            };
        }

        return PredictionMapper.ToPredictionResponse(prediction);
    }

    // === SELECT TRACK ===
    public async Task<SelectTrackResponse> SelectTrackAsync(
        string userId, SelectTrackRequest request, CancellationToken ct = default)
    {
        // 1. Gate: Verify onboarding status from UserService
        var isOnboarded = await _userService.GetIsOnboardedAsync(userId);
        if (!isOnboarded)
            throw new AppException(ErrorCodes.NOT_ONBOARDED, "Complete onboarding before selecting a track.", 422);

        // 2. Validate career track existence
        var careerTrack = await _repo.GetTrackByIdAsync(request.CareerTrackId);
        if (careerTrack is null)
            throw new AppException(ErrorCodes.TRACK_NOT_FOUND, "Career track not found.", 404);

        // 3. Fetch the latest AI prediction to determine the selection context
        var prediction = await _repo.GetLatestPredictionByUserIdAsync(userId);

        // 4. Logic: Determine SelectionType
        // Use 'Contains' to check if the track name exists within the prediction string 
        // (This handles cases where the AI returns full sentences like "Backend Engineering is a great fit...")
        var selectionType = "MANUAL";
        if (prediction != null && !string.IsNullOrEmpty(prediction.PrimaryRoleName))
        {
            if (prediction.PrimaryRoleName.Contains(careerTrack.Name, StringComparison.OrdinalIgnoreCase))
            {
                selectionType = "AI";
            }
        }

        // 5. Persist the new track selection
        var newTrack = new UserTrack
        {
            UserId = userId,
            CareerTrackId = careerTrack.Id,
            SelectionType = selectionType,
            IsActive = true,
            SelectedAt = DateTime.UtcNow
        };

        // Transactional replacement: Deactivates old tracks and inserts the new one
        var saved = await _repo.ReplaceActiveTrackAsync(userId, newTrack);

        // Ensure the response includes the populated CareerTrack object
        saved.CareerTrack = careerTrack;

        return _mapper.Map<SelectTrackResponse>(saved);
    }

    // === GET MY TRACK ===
    public async Task<MyTrackResponse> GetMyTrackAsync(string userId)
    {
        var userTrack = await _repo.GetActiveTrackByUserIdAsync(userId);
        if (userTrack is null)
            throw new AppException(ErrorCodes.NO_ACTIVE_TRACK,
                "No career track selected.", 422);

        var hasRoadmap = await _repo.HasActiveRoadmapAsync(userTrack.Id);
        var response = _mapper.Map<MyTrackResponse>(userTrack);
        response.HasRoadmap = hasRoadmap;
        return response;
    }

    // =====================================================================
    // GET TRACK RECOMMENDATIONS — Profile fetched internally from UserProfiles
    // =====================================================================
    public async Task<TrackRecommendationResponse> GetRecommendationsAsync(
        string userId, CancellationToken ct = default)
    {
        // 1. Gate: must be onboarded
        var isOnboarded = await _userService.GetIsOnboardedAsync(userId);
        if (!isOnboarded)
            throw new AppException(ErrorCodes.NOT_ONBOARDED,
                "Complete onboarding before getting track recommendations.", 422);

        // 2. Fetch the user's stored profile — single source of truth
        var profile = await _userService.GetProfileAsync(userId);

        // 3. Map stored profile → AI request profile
        //    All fields are optional in the AI contract; null means the AI
        //    will calculate a lower profile_completeness score automatically.
        var currentSkills = profile.CurrentSkills?.Count > 0 ? profile.CurrentSkills : null;
        var futureSkills  = profile.FutureSkills?.Count  > 0 ? profile.FutureSkills  : null;

        var aiProfile = new TrackRecommendProfile
        {
            Age           = profile.Age,
            EdLevel       = profile.EdLevel,
            YearsCode     = profile.YearsCode,
            WorkExp       = profile.WorkExp,
            Employment    = profile.Employment,
            RemoteWork    = profile.RemoteWork,
            Industry      = profile.Industry,
            OrgSize       = profile.OrgSize,
            AISelect      = profile.AISelect,
            CurrentSkills = currentSkills,
            FutureSkills  = futureSkills
        };

        // 4. Call AI
        var result = await _aiGateway.GetTrackRecommendationsAsync(userId, aiProfile, ct);

        // 4. Persistence: Save this recommendation as a Prediction in the database
        // This allows you to "remember" what the AI suggested for future use.
        try
        {
            var prediction = new PredictionResult
            {
                PrimaryRoleName = result.PrimaryRecommendation,
                PrimaryConfidence = (decimal)(result.RecommendedTracks.FirstOrDefault()?.FitScore ?? 0) / 100m,
                TopRolesJson = JsonSerializer.Serialize(result.RecommendedTracks.Select(t => new
                {
                    name = t.TrackName,
                    confidence = t.FitScore
                }))
            };
            await _repo.SavePredictionAsync(userId, prediction);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the request to the user
            // Because saving to MLPredictions is an auxiliary feature.
            _logger.LogWarning(ex, "Failed to save AI prediction for user {UserId}", userId);
        }

        // 5. Map and Return
        return new TrackRecommendationResponse
        {
            UserSummary = result.UserSummary,
            PrimaryRecommendation = result.PrimaryRecommendation,
            ProfileCompleteness = result.ProfileCompleteness,
            MissingInfoSuggestions = result.MissingInfoSuggestions ?? new List<string>(),
            RecommendedTracks = result.RecommendedTracks
                .Select(t => new TrackMatchResponse
                {
                    TrackName = t.TrackName,
                    FitScore = t.FitScore,
                    Reasoning = t.Reasoning,
                    SkillOverlap = t.SkillOverlap,
                    SkillsToLearn = t.SkillsToLearn,
                    EstimatedTransitionWeeks = t.EstimatedTransitionWeeks
                })
                .ToList()
        };
    }
}