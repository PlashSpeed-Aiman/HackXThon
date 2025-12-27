namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Service for managing game configuration, including executable path storage and validation.
/// </summary>
public interface IGameConfigurationService
{
    /// <summary>
    /// Gets the stored game executable path, or null if not configured.
    /// </summary>
    string? GetGameExecutablePath();

    /// <summary>
    /// Sets the game executable path.
    /// </summary>
    /// <param name="path">The full path to the game executable</param>
    void SetGameExecutablePath(string path);

    /// <summary>
    /// Validates that the game executable exists and is accessible.
    /// </summary>
    /// <param name="path">The path to validate</param>
    /// <returns>True if the executable is valid, false otherwise</returns>
    bool ValidateGameExecutable(string path);
}
