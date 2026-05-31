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

namespace MentraAI.API.Modules.CareerTracks.Services;

public class CareerTrackService : ICareerTrackService
{
    private readonly ICareerTrackRepository _repo;
    private readonly IMapper _mapper;
    private readonly IAIGatewayService _aiGateway;  // NEW — for track recommender
    private readonly IUserService _userService; // NEW — for onboarding gate + skills fallback

    public CareerTrackService(
        ICareerTrackRepository repo,
        IMapper mapper,
        IAIGatewayService aiGateway,
        IUserService userService)
    {
        _repo = repo;
        _mapper = mapper;
        _aiGateway = aiGateway;
        _userService = userService;
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
        var prediction = await _repo.GetLatestPredictionByUserIdAsync(userId);
        if (prediction is null)
            throw new AppException(ErrorCodes.NOT_ONBOARDED,
                "Complete onboarding before viewing your prediction.", 422);

        return PredictionMapper.ToPredictionResponse(prediction);
    }

    // === SELECT TRACK ===
    public async Task<SelectTrackResponse> SelectTrackAsync(
        string userId, SelectTrackRequest request, CancellationToken ct = default)
    {
        // 1. Gate: Check onboarding status from UserService (Reliable source)
        var isOnboarded = await _userService.GetIsOnboardedAsync(userId);
        if (!isOnboarded)
            throw new AppException(ErrorCodes.NOT_ONBOARDED, "Complete onboarding before selecting a track.", 422);

        var careerTrack = await _repo.GetTrackByIdAsync(request.CareerTrackId);
        if (careerTrack is null)
            throw new AppException(ErrorCodes.TRACK_NOT_FOUND, "Career track not found.", 404);

        // 2. Fetch prediction safely
        var prediction = await _repo.GetLatestPredictionByUserIdAsync(userId);

        // 3. Logic: If prediction exists and matches, mark as AI; otherwise MANUAL
        var selectionType = (prediction != null && !string.IsNullOrEmpty(prediction.PrimaryRoleName) &&
            string.Equals(careerTrack.Name, prediction.PrimaryRoleName, StringComparison.OrdinalIgnoreCase))
            ? "AI" : "MANUAL";

        var newTrack = new UserTrack
        {
            UserId = userId,
            CareerTrackId = careerTrack.Id,
            SelectionType = selectionType,
            IsActive = true,
            SelectedAt = DateTime.UtcNow
        };

        var saved = await _repo.ReplaceActiveTrackAsync(userId, newTrack);
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
    // GET TRACK RECOMMENDATIONS — Refactored for Persistence & Safety
    // =====================================================================
    public async Task<TrackRecommendationResponse> GetRecommendationsAsync(
        string userId, TrackRecommendRequest request, CancellationToken ct = default)
    {
        // 1. Gate: must be onboarded
        var isOnboarded = await _userService.GetIsOnboardedAsync(userId);
        if (!isOnboarded)
            throw new AppException(ErrorCodes.NOT_ONBOARDED,
                "Complete onboarding before getting track recommendations.", 422);

        // 2. Skill Fallback Logic (Good practice)
        List<string>? currentSkills = request.CurrentSkills;
        if (currentSkills is null || currentSkills.Count == 0)
        {
            var profile = await _userService.GetProfileAsync(userId);
            currentSkills = profile.CurrentSkills?.Count > 0 ? profile.CurrentSkills : null;
        }

        var aiProfile = new TrackRecommendProfile
        {
            Age = request.Age,
            EdLevel = request.EdLevel,
            YearsCode = request.YearsCode,
            WorkExp = request.WorkExp,
            Employment = request.Employment,
            RemoteWork = request.RemoteWork,
            Industry = request.Industry,
            OrgSize = request.OrgSize,
            AISelect = request.AISelect,
            CurrentSkills = currentSkills,
            FutureSkills = request.FutureSkills
        };

        // 3. Call AI
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
            Console.WriteLine($"Failed to save AI prediction: {ex.Message}");
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