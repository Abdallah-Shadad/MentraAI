namespace MentraAI.API.Common.Exceptions;

// Thrown when AI returns valid HTTP 200 but the response structure fails validation.
// GlobalExceptionMiddleware catches this and returns 502 AI_RESPONSE_INVALID.
public class AIValidationException : Exception
{
    public AIValidationException(string message) : base(message) { }
}