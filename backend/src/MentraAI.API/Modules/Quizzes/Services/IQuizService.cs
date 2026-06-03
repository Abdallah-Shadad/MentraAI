using MentraAI.API.Modules.Quizzes.DTOs.Requests;
using MentraAI.API.Modules.Quizzes.DTOs.Responses;

namespace MentraAI.API.Modules.Quizzes.Services;

public interface IQuizService
{
    Task<QuizResponse> GenerateQuizAsync(Guid stageProgressId, string userId, CancellationToken ct = default);

    Task<QuizResponse> GetQuizAsync(Guid quizId, string userId);

    Task<QuizSubmitResponse> SubmitQuizAsync(Guid quizId, SubmitQuizRequest request, string userId);

    Task<QuizHistoryResponse> GetHistoryAsync(Guid stageProgressId, string userId);

    Task<string> GetQuestionHintAsync(Guid quizId, string questionId, int hintIndex, string userId);
}