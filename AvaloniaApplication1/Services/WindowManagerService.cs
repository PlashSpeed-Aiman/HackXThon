using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaApplication1.Services.Interfaces;

namespace AvaloniaApplication1.Services;

/// <summary>
/// Manages main window visibility and state.
/// </summary>
public class WindowManagerService : IWindowManagerService
{
    private Window? _mainWindow;

    public bool IsWindowVisible { get; private set; }

    public event EventHandler<bool>? WindowVisibilityChanged;

    /// <summary>
    /// Initializes the service with a reference to the main window.
    /// Should be called after the MainWindow is created.
    /// </summary>
    public void Initialize(Window mainWindow)
    {
        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        IsWindowVisible = true; // Window starts visible

        // Track window closing to prevent exit
        _mainWindow.Closing += OnWindowClosing;
    }

    public void ShowWindow()
    {
        if (_mainWindow == null)
            throw new InvalidOperationException("WindowManagerService not initialized");

        Dispatcher.UIThread.Post(() =>
        {
            _mainWindow.Show();
            _mainWindow.Activate();
            _mainWindow.WindowState = WindowState.Normal;

            IsWindowVisible = true;
            WindowVisibilityChanged?.Invoke(this, true);
        });
    }

    public void HideWindow()
    {
        if (_mainWindow == null)
            throw new InvalidOperationException("WindowManagerService not initialized");

        Dispatcher.UIThread.Post(() =>
        {
            _mainWindow.Hide();

            IsWindowVisible = false;
            WindowVisibilityChanged?.Invoke(this, false);
        });
    }

    public void ToggleWindow()
    {
        if (IsWindowVisible)
            HideWindow();
        else
            ShowWindow();
    }

    public void ExitApplication()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Shutdown();
        }
    }

    private void OnWindowClosing(object? sender, WindowClosingEventArgs e)
    {
        // Prevent window close, hide instead
        e.Cancel = true;
        HideWindow();
    }
}
