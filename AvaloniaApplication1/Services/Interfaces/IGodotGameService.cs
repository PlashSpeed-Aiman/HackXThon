using System;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Service for managing the Godot game process lifecycle and window handling.
/// </summary>
public interface IGodotGameService : IDisposable
{
    /// <summary>
    /// Gets whether the game process is currently running.
    /// </summary>
    bool IsGameRunning { get; }

    /// <summary>
    /// Gets whether the game window has been successfully embedded.
    /// </summary>
    bool IsGameEmbedded { get; }

    /// <summary>
    /// Event raised when the game state changes (started, stopped, crashed).
    /// </summary>
    event EventHandler<bool>? GameStateChanged;

    /// <summary>
    /// Event raised when an error occurs.
    /// </summary>
    event EventHandler<string>? ErrorOccurred;

    /// <summary>
    /// Starts the game process.
    /// </summary>
    /// <param name="executablePath">Path to the game executable</param>
    /// <returns>True if the game started successfully, false otherwise</returns>
    Task<bool> StartGameAsync(string executablePath);

    /// <summary>
    /// Stops the game process gracefully.
    /// </summary>
    Task StopGameAsync();

    /// <summary>
    /// Gets the main window handle of the game process.
    /// </summary>
    /// <returns>The window handle, or IntPtr.Zero if not available</returns>
    IntPtr GetGameWindowHandle();
}
