using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;

namespace MentraAI.API.Modules.Quizzes.Services;

public interface IQuizService
{
    Task<QuizResponse>       GenerateQuizAsync(Guid stageProgressId, string userId);
    Task<QuizResponse>       GetQuizAsync(Guid quizId, string userId);
    Task<QuizSubmitResponse> SubmitQuizAsync(Guid quizId, string userId, SubmitQuizRequest request);
    Task<QuizHistoryResponse> GetHistoryAsync(Guid stageProgressId, string userId);
}
