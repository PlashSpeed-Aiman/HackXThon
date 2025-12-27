using System;
using System.Runtime.InteropServices;

namespace AvaloniaApplication1.Services.PlatformInterop;

/// <summary>
/// Factory for creating platform-specific window embedders.
/// </summary>
public static class WindowEmbedderFactory
{
    /// <summary>
    /// Creates the appropriate window embedder for the current platform.
    /// </summary>
    /// <returns>Platform-specific IWindowEmbedder implementation</returns>
    /// <exception cref="PlatformNotSupportedException">Thrown when the current platform is not supported</exception>
    public static IWindowEmbedder Create()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return new WindowsWindowEmbedder();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return new MacOSWindowEmbedder();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new PlatformNotSupportedException("Window embedding is not yet supported on Linux.");
        }
        else
        {
            throw new PlatformNotSupportedException($"Window embedding is not supported on this platform.");
        }
    }
}
