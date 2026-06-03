using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;
using MentraAI.API.Modules.Quizzes.Services;

namespace MentraAI.API.Modules.Quizzes.Controllers;

[ApiController]
[Route("api/v1/quizzes")]
[Authorize]
public class QuizController : ControllerBase
{
    private readonly IQuizService _service;
    private readonly IValidator<GenerateQuizRequest> _generateValidator;
    private readonly IValidator<SubmitQuizRequest> _submitValidator;

    public QuizController(
        IQuizService service,
        IValidator<GenerateQuizRequest> generateValidator,
        IValidator<SubmitQuizRequest> submitValidator)
    {
        _service = service;
        _generateValidator = generateValidator;
        _submitValidator = submitValidator;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // =====================================================================
    // POST /api/v1/quizzes/generate
    // Generates a quiz for an active stage.
    // Returns 409 if a pending quiz already exists for this stage.
    // =====================================================================
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<QuizResponse>), 201)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 409)]
    [ProducesResponseType(typeof(object), 422)]
    [ProducesResponseType(typeof(object), 502)]
    public async Task<IActionResult> GenerateQuiz([FromBody] GenerateQuizRequest request)
    {
        var validation = await _generateValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new
            {
                success = false,
                error = new { code = ErrorCodes.VALIDATION_ERROR, message = "Validation failed.", statusCode = 400, errors }
            });
        }

        var result = await _service.GenerateQuizAsync(request.StageProgressId, GetUserId(), HttpContext.RequestAborted);
        return StatusCode(201, ApiResponse<QuizResponse>.Ok(result));
    }

    // =====================================================================
    // GET /api/v1/quizzes/{quizId}
    // Fetch a quiz the user already generated (for page reload / restore).
    // =====================================================================
    [HttpGet("{quizId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<QuizResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetQuiz(Guid quizId)
    {
        var result = await _service.GetQuizAsync(quizId, GetUserId());
        return Ok(ApiResponse<QuizResponse>.Ok(result));
    }

    // =====================================================================
    // POST /api/v1/quizzes/{quizId}/submit
    // Submit answers. Scores attempt. Unlocks next stage on pass.
    // Triggers adaptation on fail (try/catch — never fails the submission).
    // Always returns 200 — roadmapAdapted field signals adaptation outcome.
    // =====================================================================
    [HttpPost("{quizId:guid}/submit")]
    [ProducesResponseType(typeof(ApiResponse<QuizSubmitResponse>), 200)]
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    [ProducesResponseType(typeof(object), 409)]
    public async Task<IActionResult> SubmitQuiz(Guid quizId, [FromBody] SubmitQuizRequest request)
    {
        var validation = await _submitValidator.ValidateAsync(request);
        if (!validation.IsValid)
        {
            var errors = validation.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            return BadRequest(new
            {
                success = false,
                error = new { code = ErrorCodes.VALIDATION_ERROR, message = "Validation failed.", statusCode = 400, errors }
            });
        }

        var result = await _service.SubmitQuizAsync(quizId, request, GetUserId());
        return Ok(ApiResponse<QuizSubmitResponse>.Ok(result));
    }

    // =====================================================================
    // GET /api/v1/quizzes/history?stageProgressId={id}
    // All quiz attempts for a stage — ordered by AttemptNumber ASC.
    // =====================================================================
    [HttpGet("history")]
    [ProducesResponseType(typeof(ApiResponse<QuizHistoryResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> GetHistory([FromQuery] Guid stageProgressId)
    {
        var result = await _service.GetHistoryAsync(stageProgressId, GetUserId());
        return Ok(ApiResponse<QuizHistoryResponse>.Ok(result));
    }


    // =====================================================================
    // GET /api/v1/quizzes/{quizId}/questions/{questionId}/hint?hintIndex={index}
    // Get a hint for a quiz question. hintIndex starts at 0 for the first hint.
    // Returns 404 if no more hints are available for this question.
    // =====================================================================
    [HttpGet("{quizId:guid}/questions/{questionId}/hint")]
    public async Task<IActionResult> GetQuestionHint(Guid quizId, string questionId, [FromQuery] int hintIndex = 0)
    {
        var hint = await _service.GetQuestionHintAsync(quizId, questionId, hintIndex, GetUserId());
        return Ok(ApiResponse<object>.Ok(new { hint }));
    }
}
