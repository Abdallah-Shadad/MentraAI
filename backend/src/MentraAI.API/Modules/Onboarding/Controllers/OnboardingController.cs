using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.Onboarding.DTOs.Requests;
using MentraAI.API.Modules.Onboarding.DTOs.Responses;
using MentraAI.API.Modules.Onboarding.Services;

namespace MentraAI.API.Modules.Onboarding.Controllers;

[ApiController]
[Route("api/v1/onboarding")]
[Authorize]
public class OnboardingController : ControllerBase
{
    private readonly IOnboardingService _service;
    private readonly IValidator<SubmitAnswersRequest> _validator;

    public OnboardingController(
        IOnboardingService service,
        IValidator<SubmitAnswersRequest> validator)
    {
        _service = service;
        _validator = validator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // ==============================
    // GET /api/v1/onboarding/questions
    // Returns all active questions. Frontend uses this to render the form.
    // ==============================
    [HttpGet("questions")]
    [ProducesResponseType(typeof(ApiResponse<QuestionsListResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetQuestions()
    {
        var result = await _service.GetQuestionsAsync();
        return Ok(ApiResponse<QuestionsListResponse>.Ok(result));
    }

    // =================================
    // GET /api/v1/onboarding/status
    // Frontend polls this to know when to navigate away from onboarding.
    // ==================================
    [HttpGet("status")]
    [ProducesResponseType(typeof(ApiResponse<OnboardingStatusResponse>), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetStatus()
    {
        var result = await _service.GetStatusAsync(GetUserId());
        return Ok(ApiResponse<OnboardingStatusResponse>.Ok(result));
    }

    // ==================================
    // POST /api/v1/onboarding/answers
    // Saves answers, calls ML predict, sets IsOnboarded = true when complete.
    // Safe to call multiple times — answers are upserted.
    // If AI fails, answers are already saved and user can retry.
    // ==================================
    [HttpPost("answers")]
    [ProducesResponseType(typeof(ApiResponse<SubmitAnswersResponse>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 502)]
    [ProducesResponseType(typeof(object), 503)]
    public async Task<IActionResult> SubmitAnswers([FromBody] SubmitAnswersRequest request)
    {
        var validation = await _validator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

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

        var result = await _service.SubmitAnswersAsync(GetUserId(), request);
        return Ok(ApiResponse<SubmitAnswersResponse>.Ok(result));
    }
}