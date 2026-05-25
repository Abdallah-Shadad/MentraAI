namespace MentraAI.API.Common.Exceptions;

public class AppException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public object? Errors { get; }

    public AppException(string errorCode, string message, int statusCode = 400, object? errors = null)
        : base(message)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        Errors = errors;
    }
}