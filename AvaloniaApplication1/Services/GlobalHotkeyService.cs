using System;
using System.Runtime.InteropServices;
using AvaloniaApplication1.Services.Interfaces;
using SharpHook;
using SharpHook.Data;

namespace AvaloniaApplication1.Services;

/// <summary>
/// Global hotkey service using SharpHook library.
/// Listens for Command+Shift+Space (macOS) or Ctrl+Shift+Space (Windows).
/// </summary>
public class GlobalHotkeyService : IGlobalHotkeyService
{
    private TaskPoolGlobalHook? _hook;
    private bool _disposed;

    public event EventHandler? HotkeyPressed;

    public bool IsRunning { get; private set; }

    public GlobalHotkeyService()
    {
        _hook = new TaskPoolGlobalHook();
        _hook.KeyPressed += OnKeyPressed;
    }

    public void Start()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GlobalHotkeyService));

        if (IsRunning)
            return;

        try
        {
            _hook?.RunAsync();
            IsRunning = true;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to start global hotkey listener. " +
                $"On macOS, ensure Accessibility permissions are granted. Error: {ex.Message}",
                ex);
        }
    }

    public void Stop()
    {
        if (!IsRunning)
            return;

        _hook?.Dispose();
        IsRunning = false;
    }

    private void OnKeyPressed(object? sender, KeyboardHookEventArgs e)
    {
        // Check for the hotkey combination:
        // - Space key
        // - Shift modifier
        // - Command (macOS) or Ctrl (Windows) modifier

        if (e.Data.KeyCode != KeyCode.VcSpace)
            return;

        var hasShift = (e.RawEvent.Mask & EventMask.Shift) != EventMask.None;
        if (!hasShift)
            return;

        bool hasCommandOrCtrl;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // macOS: Check for Command key (Meta)
            hasCommandOrCtrl = (e.RawEvent.Mask & EventMask.Meta) != EventMask.None;
        }
        else
        {
            // Windows/Linux: Check for Ctrl key
            hasCommandOrCtrl = (e.RawEvent.Mask & EventMask.Ctrl) != EventMask.None;
        }

        if (hasCommandOrCtrl)
        {
            HotkeyPressed?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        Stop();
        _hook?.Dispose();
        _hook = null;
    }
}
