using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AvaloniaApplication1.Services.Interfaces;

namespace AvaloniaApplication1.Services;

/// <summary>
/// Service for managing the Godot game process and its window.
/// </summary>
public class GodotGameService : IGodotGameService
{
    private Process? _gameProcess;
    private IntPtr _gameWindowHandle = IntPtr.Zero;
    private bool _disposed;

    public bool IsGameRunning => _gameProcess != null && !_gameProcess.HasExited;
    public bool IsGameEmbedded => _gameWindowHandle != IntPtr.Zero && IsGameRunning;

    public event EventHandler<bool>? GameStateChanged;
    public event EventHandler<string>? ErrorOccurred;

    #region Win32 API for Window Discovery

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    #endregion

    public async Task<bool> StartGameAsync(string executablePath)
    {
        if (IsGameRunning)
        {
            OnErrorOccurred("Game is already running");
            return false;
        }

        try
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = executablePath,
                UseShellExecute = true,
                WorkingDirectory = System.IO.Path.GetDirectoryName(executablePath) ?? string.Empty
            };

            _gameProcess = Process.Start(processStartInfo);

            if (_gameProcess == null)
            {
                OnErrorOccurred("Failed to start game process");
                return false;
            }

            // Subscribe to process exit event
            _gameProcess.EnableRaisingEvents = true;
            _gameProcess.Exited += OnGameProcessExited;

            // Wait for the game window to be created with retry logic
            _gameWindowHandle = await DiscoverGameWindowAsync(_gameProcess.Id);

            if (_gameWindowHandle == IntPtr.Zero)
            {
                OnErrorOccurred("Failed to find game window. The game may have closed or not created a window.");
                await StopGameAsync();
                return false;
            }

            OnGameStateChanged(true);
            return true;
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"Error starting game: {ex.Message}");
            return false;
        }
    }

    public async Task StopGameAsync()
    {
        if (_gameProcess == null)
            return;

        try
        {
            if (!_gameProcess.HasExited)
            {
                // Try graceful close first
                _gameProcess.CloseMainWindow();

                // Wait up to 3 seconds for graceful exit
                await Task.Run(() => _gameProcess.WaitForExit(3000));

                // If still running, force kill
                if (!_gameProcess.HasExited)
                {
                    _gameProcess.Kill();
                }
            }
        }
        catch (Exception ex)
        {
            OnErrorOccurred($"Error stopping game: {ex.Message}");
        }
        finally
        {
            _gameProcess?.Dispose();
            _gameProcess = null;
            _gameWindowHandle = IntPtr.Zero;
            OnGameStateChanged(false);
        }
    }

    public IntPtr GetGameWindowHandle()
    {
        return _gameWindowHandle;
    }

    private async Task<IntPtr> DiscoverGameWindowAsync(int processId)
    {
        const int maxRetries = 20; // 20 attempts
        const int delayMs = 250;   // 250ms between attempts = 5 seconds total

        for (int i = 0; i < maxRetries; i++)
        {
            IntPtr foundHandle = FindMainWindowForProcess(processId);

            if (foundHandle != IntPtr.Zero)
            {
                return foundHandle;
            }

            await Task.Delay(delayMs);
        }

        return IntPtr.Zero;
    }

    private IntPtr FindMainWindowForProcess(int processId)
    {
        IntPtr foundWindow = IntPtr.Zero;

        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint windowProcessId);

            if (windowProcessId == processId && IsWindowVisible(hWnd))
            {
                foundWindow = hWnd;
                return false; // Stop enumeration
            }

            return true; // Continue enumeration
        }, IntPtr.Zero);

        return foundWindow;
    }

    private void OnGameProcessExited(object? sender, EventArgs e)
    {
        _gameWindowHandle = IntPtr.Zero;
        OnGameStateChanged(false);
        OnErrorOccurred("Game process has exited");
    }

    private void OnGameStateChanged(bool isRunning)
    {
        GameStateChanged?.Invoke(this, isRunning);
    }

    private void OnErrorOccurred(string message)
    {
        ErrorOccurred?.Invoke(this, message);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopGameAsync().Wait();
        _disposed = true;
    }
}
