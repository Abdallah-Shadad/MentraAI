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

        if (isOnboarded && answeredCount == 5)
        {
            isOnboarded = false; // Update dynamically for frontend to see correctly

        }
        return new OnboardingStatusResponse
        {
            IsOnboarded = isOnboarded,
            AnsweredCount = answeredCount,
            TotalQuestions = activeQuestions.Count
        };
    }

    public async Task<SubmitAnswersResponse> SubmitAnswersAsync(string userId, SubmitAnswersRequest request)
    {
        // 1.Fetch all active questions to validate against and to get QuestionKeys for mapping answers to profile fields.
        // This also ensures that if new questions were added since the user last fetched the form,
        // we can handle them gracefully by treating missing answers as empty.
        var allActiveQuestions = await _onboardingRepo.GetAllActiveQuestionsAsync()
                                 ?? new List<OnboardingQuestion>();

        if (!allActiveQuestions.Any())
        {
            throw new AppException(ErrorCodes.INTERNAL_ERROR, "No active onboarding questions found in the system.", 500);
        }

        // 2. Create a user answers map for quick lookup
        var userAnswersMap = request.Answers
            .Where(a => a != null)
            .ToDictionary(a => a.QuestionId, a => a.AnswerText ?? string.Empty);

        // 3. Ensure unique index: create a row for each active question in the platform (even if optional and not answered by the user) 
        var answerEntities = new List<OnboardingAnswer>();
        foreach (var question in allActiveQuestions)
        {
            // If the user did not submit an answer for this question, we still want to create an entry to satisfy the unique index constraint.
            var answerText = userAnswersMap.GetValueOrDefault(question.Id);

            if (answerText == null)
            {
                // If the question is about skills, we set it to an empty JSON array; otherwise, an empty string.
                answerText = (question.QuestionKey == "current_skills" || question.QuestionKey == "future_skills")
                    ? "[]"
                    : string.Empty;
            }

            answerEntities.Add(new OnboardingAnswer
            {
                UserId = userId,
                QuestionId = question.Id,
                AnswerText = answerText,
                AnsweredAt = DateTime.UtcNow
            });
        }

        // 4. Upsert answers into the database
        await _onboardingRepo.UpsertAnswersAsync(userId, answerEntities);

        // 5. Map answers to profile fields
        var questionKeyMap = allActiveQuestions.ToDictionary(q => q.Id, q => q.QuestionKey);
        var answerByKey = answerEntities.ToDictionary(a => questionKeyMap[a.QuestionId], a => a.AnswerText, StringComparer.OrdinalIgnoreCase);

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

        // 6. Update user profile and onboarding status
        await _userService.UpdateProfileFromAnswersAsync(userId, profileData);
        await _userService.SetOnboardedAsync(userId);

        // 7. Return response
        return new SubmitAnswersResponse
        {
            Success = true,
            Message = "Onboarding completed successfully. Required skills and profile features updated.",
            IsOnboarded = true,
            Prediction = new PredictionData
            {
                PrimaryRole = new RoleData { Name = "Ready for Recommendation", Confidence = 0 },
                TopRoles = new List<RoleData>()
            }
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