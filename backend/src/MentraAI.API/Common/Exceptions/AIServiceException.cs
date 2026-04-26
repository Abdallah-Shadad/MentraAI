namespace MentraAI.API.Common.Exceptions;

// Thrown when AI returns a non-success HTTP status or connection is refused.
// GlobalExceptionMiddleware catches this and returns 502 AI_INTERNAL_ERROR.
public class AIServiceException : Exception
{
    public AIServiceException(string message) : base(message) { }
    public AIServiceException(string message, Exception inner) : base(message, inner) { }
}