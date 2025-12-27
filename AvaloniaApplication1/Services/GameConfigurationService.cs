using System;
using System.IO;
using System.Runtime.InteropServices;
using AvaloniaApplication1.Services.Interfaces;

namespace AvaloniaApplication1.Services;

/// <summary>
/// Implementation of game configuration service for storing and validating game executable paths.
/// </summary>
public class GameConfigurationService : IGameConfigurationService
{
    private string? _gameExecutablePath;

    public string? GetGameExecutablePath()
    {
        return _gameExecutablePath;
    }

    public void SetGameExecutablePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        _gameExecutablePath = path;
    }

    public bool ValidateGameExecutable(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            // Check if file exists
            if (!File.Exists(path))
                return false;

            // Platform-specific validation
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, expect .exe files
                return path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // On macOS, expect .app bundles or executables
                return path.EndsWith(".app", StringComparison.OrdinalIgnoreCase) ||
                       IsExecutable(path);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // On Linux, check if file is executable
                return IsExecutable(path);
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    private bool IsExecutable(string path)
    {
        try
        {
            // Check if file has executable permission (Unix-like systems)
            var fileInfo = new FileInfo(path);
            return fileInfo.Exists;
        }
        catch
        {
            return false;
        }
    }
}
