using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Data;
using MentraAI.API.Modules.CareerTracks.DTOs.Requests;
using MentraAI.API.Modules.CareerTracks.DTOs.Responses;
using MentraAI.API.Modules.CareerTracks.Mappings;
using MentraAI.API.Modules.CareerTracks.Models;
using MentraAI.API.Modules.CareerTracks.Repositories;

namespace MentraAI.API.Modules.CareerTracks.Services;

public class CareerTrackService : ICareerTrackService
{
    private readonly ICareerTrackRepository _repo;
    private readonly IMapper _mapper;
    private readonly AppDbContext _db;

    public CareerTrackService(
        ICareerTrackRepository repo,
        IMapper mapper,
        AppDbContext db)
    {
        _repo = repo;
        _mapper = mapper;
        _db = db;
    }

    // == GET ALL TRACKS ==
    public async Task<CareerTracksListResponse> GetAllTracksAsync()
    {
        var tracks = await _repo.GetAllActiveTracksAsync();

        return new CareerTracksListResponse
        {
            Tracks = _mapper.Map<List<CareerTrackResponse>>(tracks)
        };
    }

    // == GET PREDICTION ==
    public async Task<PredictionResponse> GetPredictionAsync(string userId)
    {
        var prediction = await _repo.GetLatestPredictionByUserIdAsync(userId);
        // No prediction = user has not completed onboarding
        if (prediction is null)
            throw new AppException(ErrorCodes.NOT_ONBOARDED,
                "Complete onboarding before viewing your prediction.", 422);

        return PredictionMapper.ToPredictionResponse(prediction);
    }

    // == SELECT TRACK ==
    public async Task<SelectTrackResponse> SelectTrackAsync(
        string userId, SelectTrackRequest request)
    {
        // gate on onboarding — query prediction, not UserProfile
        // avoids adding IUserService dependency to this module
        var prediction = await _repo.GetLatestPredictionByUserIdAsync(userId);
        if (prediction is null)
            throw new AppException(ErrorCodes.NOT_ONBOARDED,
                "Complete onboarding before selecting a career track.", 422);

        // verify track exists and is active
        var careerTrack = await _repo.GetTrackByIdAsync(request.CareerTrackId);
        if (careerTrack is null)
            throw new AppException(ErrorCodes.TRACK_NOT_FOUND,
                "Career track not found.", 404);

        // determine selectionType — backend decides
        var selectionType = string.Equals(
            careerTrack.Name,
            prediction.PrimaryRoleName,
            StringComparison.OrdinalIgnoreCase) ? "AI" : "MANUAL";

        // build entity
        var newTrack = new UserTrack
        {
            UserId = userId,
            CareerTrackId = careerTrack.Id,
            SelectionType = selectionType,
            IsActive = true,
            SelectedAt = DateTime.UtcNow
        };

        // deactivate old + insert new in one transaction
        // if anything fails, rollback — user's previous track remains active
        await using var tx = await _db.Database.BeginTransactionAsync();
        try
        {
            await _repo.DeactivateAllUserTracksAsync(userId);
            var saved = await _repo.InsertUserTrackAsync(newTrack);
            await tx.CommitAsync();

            // Attach CareerTrack for mapping (not loaded by InsertUserTrackAsync)
            saved.CareerTrack = careerTrack;

            return _mapper.Map<SelectTrackResponse>(saved);
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    // == GET MY TRACK ==
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
}