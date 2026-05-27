using MentraAI.API.Modules.Chat.Models;

namespace MentraAI.API.Modules.Chat.Repositories;

public interface IConversationRepository
{
    Task<Conversation?> GetByIdAsync(Guid id);
    Task<List<Conversation>> GetByUserIdAsync(string userId);
    Task<Conversation> CreateAsync(Conversation conversation);
    Task DeleteAsync(Guid id);
    Task UpdateLastMessageAtAsync(Guid id);
}