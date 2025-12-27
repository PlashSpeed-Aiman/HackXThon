using System;

namespace AvaloniaApplication1.Services.PlatformInterop;

/// <summary>
/// Platform-agnostic interface for embedding external windows into the application.
/// </summary>
public interface IWindowEmbedder
{
    /// <summary>
    /// Embeds a child window into a parent window.
    /// </summary>
    /// <param name="childWindowHandle">The handle of the window to embed</param>
    /// <param name="parentWindowHandle">The handle of the parent window</param>
    /// <returns>True if embedding was successful, false otherwise</returns>
    bool EmbedWindow(IntPtr childWindowHandle, IntPtr parentWindowHandle);

    /// <summary>
    /// Resizes an embedded window.
    /// </summary>
    /// <param name="windowHandle">The handle of the window to resize</param>
    /// <param name="width">The new width in pixels</param>
    /// <param name="height">The new height in pixels</param>
    /// <returns>True if resize was successful, false otherwise</returns>
    bool ResizeEmbeddedWindow(IntPtr windowHandle, int width, int height);

    /// <summary>
    /// Performs cleanup on an embedded window before it's removed.
    /// </summary>
    /// <param name="windowHandle">The handle of the window to clean up</param>
    void CleanupEmbeddedWindow(IntPtr windowHandle);
}
