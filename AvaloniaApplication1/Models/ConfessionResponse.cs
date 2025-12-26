namespace AvaloniaApplication1.Models;

public class ConfessionResponse
{
    public bool Success { get; init; }
    public string? AbsolutionMessage { get; init; }
    public string? ErrorMessage { get; init; }

    public static ConfessionResponse CreateSuccess(string absolutionMessage)
    {
        return new ConfessionResponse
        {
            Success = true,
            AbsolutionMessage = absolutionMessage,
            ErrorMessage = null
        };
    }

    public static ConfessionResponse CreateError(string errorMessage)
    {
        return new ConfessionResponse
        {
            Success = false,
            AbsolutionMessage = null,
            ErrorMessage = errorMessage
        };
    }
}
