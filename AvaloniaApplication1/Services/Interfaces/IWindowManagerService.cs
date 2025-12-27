using System;
using Avalonia.Controls;

namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Service for managing main window visibility and state.
/// </summary>
public interface IWindowManagerService
{
    /// <summary>
    /// Gets whether the window is currently visible.
    /// </summary>
    bool IsWindowVisible { get; }

    /// <summary>
    /// Event raised when window visibility changes.
    /// </summary>
    event EventHandler<bool>? WindowVisibilityChanged;

    /// <summary>
    /// Initializes the service with a reference to the main window.
    /// Should be called after the MainWindow is created.
    /// </summary>
    /// <param name="mainWindow">The main application window</param>
    void Initialize(Window mainWindow);

    /// <summary>
    /// Shows and activates the main window.
    /// </summary>
    void ShowWindow();

    /// <summary>
    /// Hides the main window.
    /// </summary>
    void HideWindow();

    /// <summary>
    /// Toggles window visibility (show if hidden, hide if shown).
    /// </summary>
    void ToggleWindow();

    /// <summary>
    /// Exits the application.
    /// </summary>
    void ExitApplication();
}
