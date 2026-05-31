namespace MentraAI.API.Modules.StageProgress.DTOs.Responses;

public class StageResourcesResponse
{
    public Guid StageProgressId { get; set; }
    public string StageName { get; set; } = string.Empty;
    public int StageIndex { get; set; }
    public string Status { get; set; } = string.Empty;
    public StageResources Resources { get; set; } = new();
}

public class StageResources
{
    public List<VideoResource> Videos { get; set; } = new();
    public List<ArticleResource> Articles { get; set; } = new();
    public List<DocumentationResource> Documentation { get; set; } = new();
}

public class VideoResource
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}

public class ArticleResource
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
}

public class DocumentationResource
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}