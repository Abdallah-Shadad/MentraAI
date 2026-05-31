namespace MentraAI.API.Modules.Onboarding.DTOs.Responses;

public class OnboardingStatusResponse
{
    public bool IsOnboarded { get; set; }
    public int AnsweredCount { get; set; }
    public int TotalQuestions { get; set; }
}