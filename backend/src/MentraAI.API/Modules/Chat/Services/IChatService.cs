using MentraAI.API.Modules.Chat.DTOs.Responses;
using MentraAI.API.Modules.Chat.Models;

namespace MentraAI.API.Modules.Chat.Services;

public interface IChatService
{
    /// <summary>
    /// Creates a new Conversation row and returns its ID.
    /// The AI service memory will be keyed by this ID.
    /// </summary>
    Task<ConversationResponse> CreateConversationAsync(string userId, string? title);

    /// <summary>Lists all active conversations for a user, ordered newest first.</summary>
    Task<ConversationListResponse> GetConversationsAsync(string userId);

    /// <summary>
    /// Verifies ownership and returns the conversation or null.
    /// Used by the controller to authorize chat message and delete requests.
    /// </summary>
    Task<Conversation?> GetConversationAsync(Guid conversationId, string userId);

    /// <summary>Deletes the conversation row from our DB (AI memory cleared separately).</summary>
    Task DeleteConversationAsync(Guid conversationId, string userId);

    /// <summary>Updates LastMessageAt timestamp — called after each successful message send.</summary>
    Task UpdateLastMessageAsync(Guid conversationId);
}