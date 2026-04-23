namespace MentraAI.API.Common.Errors;

public static class ErrorCodes
{
    // Auth
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string INVALID_CREDENTIALS = "INVALID_CREDENTIALS";
    public const string REFRESH_TOKEN_INVALID = "REFRESH_TOKEN_INVALID";

    // User / Onboarding
    public const string EMAIL_ALREADY_EXISTS = "EMAIL_ALREADY_EXISTS";
    public const string NOT_ONBOARDED = "NOT_ONBOARDED";
    public const string QUESTION_NOT_FOUND = "QUESTION_NOT_FOUND";

    // Career Tracks
    public const string TRACK_NOT_FOUND = "TRACK_NOT_FOUND";
    public const string NO_ACTIVE_TRACK = "NO_ACTIVE_TRACK";

    // Roadmaps
    public const string ROADMAP_ALREADY_EXISTS = "ROADMAP_ALREADY_EXISTS";
    public const string ROADMAP_NOT_FOUND = "ROADMAP_NOT_FOUND";

    // Stages
    public const string STAGE_NOT_FOUND = "STAGE_NOT_FOUND";
    public const string STAGE_LOCKED = "STAGE_LOCKED";
    public const string RESOURCES_NOT_FETCHED = "RESOURCES_NOT_FETCHED";

    // Quizzes
    public const string QUIZ_NOT_FOUND = "QUIZ_NOT_FOUND";
    public const string QUIZ_ALREADY_SUBMITTED = "QUIZ_ALREADY_SUBMITTED";
    public const string STAGE_NOT_ACTIVE = "STAGE_NOT_ACTIVE";

    // AI
    public const string AI_SERVICE_UNAVAILABLE = "AI_SERVICE_UNAVAILABLE";
    public const string AI_INTERNAL_ERROR = "AI_INTERNAL_ERROR";
    public const string AI_TIMEOUT = "AI_TIMEOUT";
    public const string AI_RESPONSE_INVALID = "AI_RESPONSE_INVALID";

    // General
    public const string NOT_FOUND = "NOT_FOUND";
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
}