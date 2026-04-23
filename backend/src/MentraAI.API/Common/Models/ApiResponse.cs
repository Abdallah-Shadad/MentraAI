namespace MentraAI.API.Common.Models;

public class ApiResponse<T>
{
    public bool Success { get; private set; }
    public T? Data { get; private set; }
    public object? Error { get; private set; }

    private ApiResponse() { }

    public static ApiResponse<T> Ok(T data) =>
        new() { Success = true, Data = data };

    public static ApiResponse<T> Fail(string code, string message, int statusCode) =>
        new()
        {
            Success = false,
            Error = new { code, message, statusCode }
        };
}