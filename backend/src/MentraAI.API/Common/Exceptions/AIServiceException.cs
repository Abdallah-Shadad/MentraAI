namespace MentraAI.API.Common.Exceptions;

public class AIServiceException : Exception
{
    public AIServiceException(string message) : base(message) { }
    public AIServiceException(string message, Exception inner) : base(message, inner) { }
}