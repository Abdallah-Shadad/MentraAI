namespace MentraAI.API.Modules.Chat.DTOs.Responses;

public class ConversationResponse
{
    public Guid ConversationId { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastMessageAt { get; set; }
}

public class ConversationListResponse
{
    public List<ConversationResponse> Conversations { get; set; } = new();
}