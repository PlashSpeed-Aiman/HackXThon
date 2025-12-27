using System;

namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Service for registering and handling global hotkeys across the system.
/// </summary>
public interface IGlobalHotkeyService : IDisposable
{
    /// <summary>
    /// Event raised when the registered global hotkey is pressed.
    /// </summary>
    event EventHandler? HotkeyPressed;

    /// <summary>
    /// Starts listening for the global hotkey.
    /// Command+Shift+Space on macOS, Ctrl+Shift+Space on Windows.
    /// </summary>
    void Start();

    /// <summary>
    /// Stops listening for the global hotkey.
    /// </summary>
    void Stop();

    /// <summary>
    /// Gets whether the hotkey listener is currently running.
    /// </summary>
    bool IsRunning { get; }
}
