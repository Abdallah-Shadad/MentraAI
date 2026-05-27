using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.Chat.DTOs.Responses;
using MentraAI.API.Modules.Chat.Models;
using MentraAI.API.Modules.Chat.Repositories;

namespace MentraAI.API.Modules.Chat.Services;

public class ChatService : IChatService
{
    private readonly IConversationRepository _repo;
    private readonly ILogger<ChatService> _logger;

    public ChatService(IConversationRepository repo, ILogger<ChatService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<ConversationResponse> CreateConversationAsync(string userId, string? title)
    {
        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ConversationTitle = title,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var created = await _repo.CreateAsync(conversation);

        _logger.LogInformation(
            "Created conversation {ConversationId} for user {UserId}",
            created.Id, userId);

        return ToResponse(created);
    }

    public async Task<ConversationListResponse> GetConversationsAsync(string userId)
    {
        var conversations = await _repo.GetByUserIdAsync(userId);

        return new ConversationListResponse
        {
            Conversations = conversations.Select(ToResponse).ToList()
        };
    }

    public async Task<Conversation?> GetConversationAsync(Guid conversationId, string userId)
    {
        var conversation = await _repo.GetByIdAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
            return null;

        return conversation;
    }

    public async Task DeleteConversationAsync(Guid conversationId, string userId)
    {
        var conversation = await _repo.GetByIdAsync(conversationId);
        if (conversation is null || conversation.UserId != userId)
            throw new AppException(ErrorCodes.NOT_FOUND, "Conversation not found.", 404);

        await _repo.DeleteAsync(conversationId);

        _logger.LogInformation(
            "Deleted conversation {ConversationId} for user {UserId}",
            conversationId, userId);
    }

    public async Task UpdateLastMessageAsync(Guid conversationId) =>
        await _repo.UpdateLastMessageAtAsync(conversationId);

    private static ConversationResponse ToResponse(Conversation c) =>
        new()
        {
            ConversationId = c.Id,
            Title = c.ConversationTitle,
            CreatedAt = c.CreatedAt,
            LastMessageAt = c.LastMessageAt
        };
}