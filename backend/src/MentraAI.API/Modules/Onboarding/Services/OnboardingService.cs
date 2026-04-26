using System.Text.Json;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.Onboarding.DTOs.Requests;
using MentraAI.API.Modules.Onboarding.DTOs.Responses;
using MentraAI.API.Modules.Onboarding.Models;
using MentraAI.API.Modules.Onboarding.Repositories;
using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.Services;

namespace MentraAI.API.Modules.Onboarding.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _onboardingRepo;
    private readonly IUserService _userService;
    private readonly IAIGatewayService _aiGateway;

    public OnboardingService(
        IOnboardingRepository onboardingRepo,
        IUserService userService,
        IAIGatewayService aiGateway)
    {
        _onboardingRepo = onboardingRepo;
        _userService = userService;
        _aiGateway = aiGateway;
    }

    // GET QUESTIONS
    public async Task<QuestionsListResponse> GetQuestionsAsync()
    {
        var questions = await _onboardingRepo.GetAllActiveQuestionsAsync();

        var items = questions
            .Select(OnboardingMappings.ToQuestionItem)
            .ToList();

        return new QuestionsListResponse { Questions = items };
    }

    // GET STATUS
    public async Task<OnboardingStatusResponse> GetStatusAsync(string userId)
    {
        var answeredCount = await _onboardingRepo.GetAnswerCountByUserIdAsync(userId);
        var activeQuestions = await _onboardingRepo.GetAllActiveQuestionsAsync();
        var isOnboarded = await _userService.GetIsOnboardedAsync(userId);

        return new OnboardingStatusResponse
        {
            IsOnboarded = isOnboarded,
            AnsweredCount = answeredCount,
            TotalQuestions = activeQuestions.Count
        };
    }

    // SUBMIT ANSWERS
    public async Task<SubmitAnswersResponse> SubmitAnswersAsync(
        string userId, SubmitAnswersRequest request)
    {
        // Step 1: validate all submitted questionIds exist and are active
        var submittedIds = request.Answers.Select(a => a.QuestionId).ToList();
        var validQuestions = await _onboardingRepo.GetQuestionsByIdsAsync(submittedIds);
        var validIds = validQuestions.Select(q => q.Id).ToHashSet();

        var invalidId = submittedIds.FirstOrDefault(id => !validIds.Contains(id));
        if (invalidId != 0)
            throw new AppException(
                ErrorCodes.QUESTION_NOT_FOUND,
                $"Question with ID {invalidId} does not exist or is not active.",
                404);

        // Upsert answers BEFORE calling AI
        // If AI fails the user can retry — answers are already saved, no duplicates
        var answerEntities = request.Answers
            .Select(a => new OnboardingAnswer
            {
                UserId = userId,
                QuestionId = a.QuestionId,
                AnswerText = a.AnswerText,
                AnsweredAt = DateTime.UtcNow
            })
            .ToList();

        await _onboardingRepo.UpsertAnswersAsync(userId, answerEntities);

        // Map answers to profile fields using QuestionKey
        var questionKeyMap = validQuestions.ToDictionary(q => q.Id, q => q.QuestionKey);
        var answerByKey = BuildAnswerByKeyMap(request.Answers, questionKeyMap);

        var profileData = new ProfileUpdateData
        {
            Background = answerByKey.GetValueOrDefault("background"),
            WeeklyHours = ParseWeeklyHours(answerByKey.GetValueOrDefault("weekly_hours")),
            CurrentSkillsJson = NormalizeJsonArray(answerByKey.GetValueOrDefault("current_skills")),
            InterestsJson = NormalizeJsonArray(answerByKey.GetValueOrDefault("interests")),
            CareerGoals = answerByKey.GetValueOrDefault("career_goals")
        };

        // update UserProfile via IUserService
        await _userService.UpdateProfileFromAnswersAsync(userId, profileData);

        // fetch the updated profile to pass into AI request
        var profile = await _userService.GetProfileEntityAsync(userId)
            ?? throw new AppException(
                ErrorCodes.NOT_FOUND,
                "Profile not found.",
                404);

        // call AI prediction
        // AIServiceException / AIValidationException bubble to GlobalExceptionMiddleware
        // answers are already saved at step 2, user can safely retry on AI failure
        var predictionResult = await _aiGateway.PredictCareerAsync(userId, profile);

        // save prediction — stays inside Onboarding module, not UserService
        await _onboardingRepo.SavePredictionAsync(userId, predictionResult);

        // check if all active questions are now answered
        var allActive = await _onboardingRepo.GetAllActiveQuestionsAsync();
        var answeredCount = await _onboardingRepo.GetAnswerCountByUserIdAsync(userId);
        var allAnswered = answeredCount >= allActive.Count;

        // set IsOnboarded only when every active question is covered
        if (allAnswered)
            await _userService.SetOnboardedAsync(userId);

        // build and return response
        return OnboardingMappings.ToSubmitResponse(predictionResult, allAnswered);
    }


    // PRIVATE HELPERS


    // Builds QuestionKey -> AnswerText lookup so mapping never relies on hardcoded IDs
    private static Dictionary<string, string> BuildAnswerByKeyMap(
        List<AnswerItem> answers,
        Dictionary<int, string> questionKeyMap)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var answer in answers)
        {
            if (questionKeyMap.TryGetValue(answer.QuestionId, out var key))
                result[key] = answer.AnswerText;
        }
        return result;
    }

    // Parses weekly_hours answer string to nullable int
    private static int? ParseWeeklyHours(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return int.TryParse(raw.Trim(), out var hours) ? hours : null;
    }

    // Ensures MULTISELECT answers are stored as valid JSON array strings
    // Input may already be ["Python","SQL"] or a plain "Python"
    private static string? NormalizeJsonArray(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var trimmed = raw.Trim();
        if (trimmed.StartsWith('['))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<List<string>>(trimmed);
                return parsed is not null ? trimmed : null;
            }
            catch { return null; }
        }

        // Plain string — wrap in array
        return JsonSerializer.Serialize(new List<string> { trimmed });
    }
}