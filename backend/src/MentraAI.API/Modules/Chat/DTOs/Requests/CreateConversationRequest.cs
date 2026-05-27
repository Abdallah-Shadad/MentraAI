namespace MentraAI.API.Modules.Chat.DTOs.Requests;

public class CreateConversationRequest
{
    /// <summary>Optional title — set from first message or topic description.</summary>
    public string? Title { get; set; }
}