namespace AvaloniaApplication1.Models;

public class GeminiResponse
{
    public bool Success { get; init; }
    public string? Haiku { get; init; }
    public string? ErrorMessage { get; init; }

    public static GeminiResponse CreateSuccess(string haiku)
    {
        return new GeminiResponse
        {
            Success = true,
            Haiku = haiku,
            ErrorMessage = null
        };
    }

    public static GeminiResponse CreateError(string errorMessage)
    {
        return new GeminiResponse
        {
            Success = false,
            Haiku = null,
            ErrorMessage = errorMessage
        };
    }
}
