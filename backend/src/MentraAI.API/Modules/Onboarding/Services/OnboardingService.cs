using System.Text.Json;
using AutoMapper;
using MentraAI.API.Common.Errors;
using MentraAI.API.Common.Exceptions;
using MentraAI.API.Modules.AIGateway.Services;
using MentraAI.API.Modules.Onboarding.DTOs.Requests;
using MentraAI.API.Modules.Onboarding.DTOs.Responses;
using MentraAI.API.Modules.Onboarding.Models;
using MentraAI.API.Modules.Onboarding.Repositories;
using MentraAI.API.Modules.Users.DTOs.Requests;
using MentraAI.API.Modules.Users.Repositories;
using MentraAI.API.Modules.Users.Services;

namespace MentraAI.API.Modules.Onboarding.Services;

public class OnboardingService : IOnboardingService
{
    private readonly IOnboardingRepository _onboardingRepo;
    private readonly IUserService _userService;
    //private readonly IAIGatewayService _aiGateway;

    public OnboardingService(
        IOnboardingRepository onboardingRepo,
        IUserService userService)
    {
        _onboardingRepo = onboardingRepo;
        _userService = userService;

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

    public async Task<SubmitAnswersResponse> SubmitAnswersAsync(string userId, SubmitAnswersRequest request)
    {
        // 1. Validate: Get all questions to map QuestionId to QuestionKey (e.g., "Age", "EdLevel")
        var submittedIds = request.Answers.Select(a => a.QuestionId).ToList();
        var validQuestions = await _onboardingRepo.GetQuestionsByIdsAsync(submittedIds);
        var questionKeyMap = validQuestions.ToDictionary(q => q.Id, q => q.QuestionKey);

        // 2. Persist answers: Save raw answers to DB
        var answerEntities = request.Answers.Select(a => new OnboardingAnswer
        {
            UserId = userId,
            QuestionId = a.QuestionId,
            AnswerText = a.AnswerText,
            AnsweredAt = DateTime.UtcNow
        }).ToList();
        await _onboardingRepo.UpsertAnswersAsync(userId, answerEntities);

        // 3. Map to Profile: Use the helper to map keys to values
        var answerByKey = BuildAnswerByKeyMap(request.Answers, questionKeyMap);
        var profileData = new ProfileUpdateData
        {
            Age = answerByKey.GetValueOrDefault("Age"),
            EdLevel = answerByKey.GetValueOrDefault("EdLevel"),
            YearsCode = ParseDouble(answerByKey.GetValueOrDefault("YearsCode")),
            WorkExp = ParseDouble(answerByKey.GetValueOrDefault("WorkExp")),
            Employment = answerByKey.GetValueOrDefault("Employment"),
            RemoteWork = answerByKey.GetValueOrDefault("RemoteWork"),
            Industry = answerByKey.GetValueOrDefault("Industry"),
            OrgSize = answerByKey.GetValueOrDefault("OrgSize"),
            AISelect = answerByKey.GetValueOrDefault("AISelect"),
            CurrentSkillsJson = NormalizeJsonArray(answerByKey.GetValueOrDefault("current_skills")),
            FutureSkillsJson = NormalizeJsonArray(answerByKey.GetValueOrDefault("future_skills"))
        };

        // 4. Update Profile: This now correctly populates the user's data
        await _userService.UpdateProfileFromAnswersAsync(userId, profileData);

        // 5. Finalize Onboarding Status
        await _userService.SetOnboardedAsync(userId);

        return new SubmitAnswersResponse
        {
            Success = true,
            Message = "Onboarding completed successfully. You can now select a track manually or use the AI Recommender."
        };
    }

    //// SUBMIT ANSWERS
    //public async Task<SubmitAnswersResponse> SubmitAnswersAsync(
    //    string userId, SubmitAnswersRequest request)
    //{
    //    // Step 1: validate all submitted questionIds exist and are active
    //    var submittedIds = request.Answers.Select(a => a.QuestionId).ToList();
    //    var validQuestions = await _onboardingRepo.GetQuestionsByIdsAsync(submittedIds);
    //    var validIds = validQuestions.Select(q => q.Id).ToHashSet();

    //    var invalidId = submittedIds.FirstOrDefault(id => !validIds.Contains(id));
    //    if (invalidId != 0)
    //        throw new AppException(
    //            ErrorCodes.QUESTION_NOT_FOUND,
    //            $"Question with ID {invalidId} does not exist or is not active.",
    //            404);

    //    // Upsert answers BEFORE calling AI
    //    var answerEntities = request.Answers
    //        .Select(a => new OnboardingAnswer
    //        {
    //            UserId = userId,
    //            QuestionId = a.QuestionId,
    //            AnswerText = a.AnswerText,
    //            AnsweredAt = DateTime.UtcNow
    //        })
    //        .ToList();

    //    await _onboardingRepo.UpsertAnswersAsync(userId, answerEntities);

    //    // Map answers to profile fields using QuestionKey matching the 11 new DB entries
    //    var questionKeyMap = validQuestions.ToDictionary(q => q.Id, q => q.QuestionKey);
    //    var answerByKey = BuildAnswerByKeyMap(request.Answers, questionKeyMap);

    //    var profileData = new ProfileUpdateData
    //    {
    //        Age = answerByKey.GetValueOrDefault("Age"),
    //        EdLevel = answerByKey.GetValueOrDefault("EdLevel"),
    //        YearsCode = ParseDouble(answerByKey.GetValueOrDefault("YearsCode")),
    //        WorkExp = ParseDouble(answerByKey.GetValueOrDefault("WorkExp")),
    //        Employment = answerByKey.GetValueOrDefault("Employment"),
    //        RemoteWork = answerByKey.GetValueOrDefault("RemoteWork"),
    //        Industry = answerByKey.GetValueOrDefault("Industry"),
    //        OrgSize = answerByKey.GetValueOrDefault("OrgSize"),
    //        AISelect = answerByKey.GetValueOrDefault("AISelect"),
    //        CurrentSkillsJson = NormalizeJsonArray(answerByKey.GetValueOrDefault("current_skills")),
    //        FutureSkillsJson = NormalizeJsonArray(answerByKey.GetValueOrDefault("future_skills"))
    //    };

    //    // update UserProfile via IUserService
    //    await _userService.UpdateProfileFromAnswersAsync(userId, profileData);

    //    // fetch the updated profile to pass into AI request
    //    var profile = await _userService.GetProfileEntityAsync(userId)
    //        ?? throw new AppException(
    //            ErrorCodes.NOT_FOUND,
    //            "Profile not found.",
    //            404);

    //    // call AI prediction
    //    var predictionResult = await _aiGateway.PredictCareerAsync(userId, profile);

    //    // save prediction
    //    await _onboardingRepo.SavePredictionAsync(userId, predictionResult);

    //    // check if all active questions are now answered
    //    var allActive = await _onboardingRepo.GetAllActiveQuestionsAsync();
    //    var answeredCount = await _onboardingRepo.GetAnswerCountByUserIdAsync(userId);
    //    var allAnswered = answeredCount >= allActive.Count;

    //    // set IsOnboarded only when every active question is covered
    //    if (allAnswered)
    //        await _userService.SetOnboardedAsync(userId);

    //    // build and return response
    //    return OnboardingMappings.ToSubmitResponse(predictionResult, allAnswered);
    //}

    // PRIVATE HELPERS

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

    private static double? ParseDouble(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        return double.TryParse(raw.Trim(), out var val) ? val : null;
    }

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

        return JsonSerializer.Serialize(new List<string> { trimmed });
    }
}