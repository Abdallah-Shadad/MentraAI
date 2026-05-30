using System.Security.Claims;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Common.Models;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.Chat.DTOs.Requests;
using MentraAI.API.Modules.Chat.DTOs.Responses;
using MentraAI.API.Modules.Chat.Services;

namespace MentraAI.API.Modules.Chat.Controllers;

[ApiController]
[Route("api/v1/chat/conversations")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chatService;
    private readonly IAIGatewayService _aiGateway;
    private readonly IValidator<ChatRequest> _chatValidator;
    private readonly ILogger<ChatController> _logger;

    public ChatController(
        IChatService chatService,
        IAIGatewayService aiGateway,
        IValidator<ChatRequest> chatValidator,
        ILogger<ChatController> logger)
    {
        _chatService = chatService;
        _aiGateway = aiGateway;
        _chatValidator = chatValidator;
        _logger = logger;
    }

    private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // =====================================================================
    // POST /api/v1/chat/conversations
    // Create a new conversation — returns the conversationId for subsequent messages.
    // =====================================================================
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ConversationResponse>), 201)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> CreateConversation(
        [FromBody] CreateConversationRequest request)
    {
        var result = await _chatService.CreateConversationAsync(GetUserId(), request.Title);
        return StatusCode(201, ApiResponse<ConversationResponse>.Ok(result));
    }

    // =====================================================================
    // GET /api/v1/chat/conversations
    // List all conversations for the authenticated user.
    // =====================================================================
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<ConversationListResponse>), 200)]
    [ProducesResponseType(typeof(object), 401)]
    public async Task<IActionResult> GetConversations()
    {
        var result = await _chatService.GetConversationsAsync(GetUserId());
        return Ok(ApiResponse<ConversationListResponse>.Ok(result));
    }

    // =====================================================================
    // DELETE /api/v1/chat/conversations/{conversationId}
    // Delete conversation from our DB and clear AI Redis memory.
    // =====================================================================
    [HttpDelete("{conversationId:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> DeleteConversation(Guid conversationId)
    {
        var userId = GetUserId();

        // Verify ownership before deletion
        var conversation = await _chatService.GetConversationAsync(conversationId, userId);

        // Already checked in the service layer, but double-check here to avoid clearing AI memory for the wrong user if something is off with the service layer validation.
        if (conversation is null)
            throw new AppException(ErrorCodes.NOT_FOUND, "Conversation not found.", 404);

        // Delete our DB row
        await _chatService.DeleteConversationAsync(conversationId, userId);

        // Best-effort: clear AI Redis memory (non-fatal if it fails)
        try
        {
            await _aiGateway.DeleteChatMemoryAsync(userId, conversationId.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to clear AI memory for conversation {ConversationId}. " +
                "DB row deleted successfully.",
                conversationId);
        }

        return NoContent();
    }

    // =====================================================================
    // POST /api/v1/chat/conversations/{conversationId}/messages
    // Send a message — streams the AI SSE response directly to the frontend.
    //
    // CRITICAL: All validation and DB checks MUST happen before StreamChatAsync.
    // Once streaming begins the response is committed — no error JSON can be written.
    // =====================================================================
    [HttpPost("{conversationId:guid}/messages")]
    [ProducesResponseType(200)] // SSE stream
    [ProducesResponseType(typeof(object), 400)]
    [ProducesResponseType(typeof(object), 401)]
    [ProducesResponseType(typeof(object), 404)]
    public async Task<IActionResult> SendMessage(
        Guid conversationId,
        [FromBody] ChatRequest request,
        CancellationToken ct)
    {
        // 1. Validate request body
        var validation = await _chatValidator.ValidateAsync(request, ct);
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

        var userId = GetUserId();

        // 2. Verify conversation ownership — MUST be before streaming starts
        var conversation = await _chatService.GetConversationAsync(conversationId, userId);
        if (conversation is null)
            throw new AppException(ErrorCodes.NOT_FOUND, "Conversation not found.", 404);

        // 3. Build AI request — inject userId from JWT, never from client
        var aiRequest = new ChatAIRequest
        {
            UserId = userId,
            ConversationId = conversationId.ToString(),
            Query = request.Query,
            CareerTrack = request.CareerTrack,
            Stage = request.Stage,
            LessonId = request.LessonId,
            QuizDetails = request.QuizDetails,
            QuizScore = request.QuizScore
        };

        // 4. Update timestamp (fire-and-forget style, before streaming)
        await _chatService.UpdateLastMessageAsync(conversationId);

        // 5. Stream directly — no buffering
        // Note: after this point the response is committed.
        // EmptyResult() after streaming is intentional and harmless.
        await _aiGateway.StreamChatAsync(aiRequest, Response, ct);

        return new EmptyResult();
    }
    // =====================================================================
    // GET /api/v1/chat/health
    // Acts as a proxy to check if the external AI chat server is currently online.
    // The frontend should call this to enable/disable the chat UI dynamically.
    // =====================================================================
    [HttpGet("health")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(object), 503)]
    public async Task<IActionResult> CheckHealth([FromServices] IAIGatewayService aiGatewayService)
    {
        var isHealthy = await aiGatewayService.CheckChatHealthAsync();

        if (isHealthy)
        {
            // AI is up
            return Ok(ApiResponse<object>.Ok(new { status = "ok", message = "Chat AI is up and running." }));
        }

        // AI is down - Return 503 Service Unavailable
        return StatusCode(503, ApiResponse<object>.Fail(
            ErrorCodes.AI_INTERNAL_ERROR,
            "Chat service is temporarily unavailable.",
            503));
    }
}