namespace MentraAI.API.Common.Exceptions;

public class AIValidationException : Exception
{
    public AIValidationException(string message) : base(message) { }
}