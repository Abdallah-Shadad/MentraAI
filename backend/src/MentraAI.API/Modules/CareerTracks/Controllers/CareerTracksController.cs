using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.CareerTracks.DTOs.Requests;
using MentraAI.API.Modules.CareerTracks.DTOs.Responses;
using MentraAI.API.Modules.CareerTracks.Services;

namespace MentraAI.API.Modules.CareerTracks.Controllers;

[ApiController]
[Route("api/v1/career-tracks")]
[Authorize]
public class CareerTracksController : ControllerBase
{
    private readonly ICareerTrackService _service;
    private readonly IValidator<SelectTrackRequest> _validator;

    public CareerTracksController(
        ICareerTrackService service,
        IValidator<SelectTrackRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;


    // == Returns all active tracks. Frontend uses this to render the selection UI ==
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<CareerTracksListResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllTracksAsync();
        return Ok(ApiResponse<CareerTracksListResponse>.Ok(result));
    }

    // == Returns the stored ML prediction so frontend can highlight recommended track ==
    [HttpGet("prediction")]
    [ProducesResponseType(typeof(ApiResponse<PredictionResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 422)]
    public async Task<IActionResult> GetPrediction()
    {
        var result = await _service.GetPredictionAsync(GetUserId());
        return Ok(ApiResponse<PredictionResponse>.Ok(result));
    }

    // == User confirms their track. Previous track deactivated atomically ==
    [HttpPost("select")]
    [ProducesResponseType(typeof(ApiResponse<SelectTrackResponse>), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 422)]
    public async Task<IActionResult> SelectTrack([FromBody] SelectTrackRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new
            {
                success = false,
                error = new
                {
                    code = ErrorCodes.VALIDATION_ERROR,
                    message = "Validation failed.",
                    statusCode = 400,
                    errors
                }
            });
        }

        var result = await _service.SelectTrackAsync(GetUserId(), request);
        return StatusCode(201, ApiResponse<SelectTrackResponse>.Ok(result));
    }

    // == Returns active track with hasRoadmap flag==
    // Frontend uses hasRoadmap to show "Generate Roadmap" or "View Roadmap"
    [HttpGet("my-track")]
    [ProducesResponseType(typeof(ApiResponse<MyTrackResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 422)]
    public async Task<IActionResult> GetMyTrack()
    {
        var result = await _service.GetMyTrackAsync(GetUserId());
        return Ok(ApiResponse<MyTrackResponse>.Ok(result));
    }
}