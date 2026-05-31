using Microsoft.EntityFrameworkCore;
using MentraAI.API.Data;
using MentraAI.API.Modules.Chat.Models;

namespace MentraAI.API.Modules.Chat.Repositories;

public class ConversationRepository : IConversationRepository
{
    private readonly AppDbContext _db;

    public ConversationRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Conversation?> GetByIdAsync(Guid id) =>
        await _db.Conversations.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Conversation>> GetByUserIdAsync(string userId) =>
        await _db.Conversations
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<Conversation> CreateAsync(Conversation conversation)
    {
        _db.Conversations.Add(conversation);
        await _db.SaveChangesAsync();
        return conversation;
    }

    public async Task DeleteAsync(Guid id)
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == id);
        if (conversation is null) return;

        _db.Conversations.Remove(conversation);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateLastMessageAtAsync(Guid id)
    {
        var conversation = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == id);
        if (conversation is null) return;

        conversation.LastMessageAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}