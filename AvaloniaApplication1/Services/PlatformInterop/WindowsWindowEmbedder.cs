using System;
using System.Runtime.InteropServices;

namespace AvaloniaApplication1.Services.PlatformInterop;

/// <summary>
/// Windows-specific implementation for embedding external windows using Win32 APIs.
/// </summary>
public class WindowsWindowEmbedder : IWindowEmbedder
{
    // Window Styles
    private const int GWL_STYLE = -16;
    private const uint WS_VISIBLE = 0x10000000;
    private const uint WS_CHILD = 0x40000000;
    private const uint WS_CAPTION = 0x00C00000;
    private const uint WS_THICKFRAME = 0x00040000;
    private const uint WS_SYSMENU = 0x00080000;

    // SetWindowPos flags
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const uint SWP_FRAMECHANGED = 0x0020;

    #region Win32 API Declarations

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
        int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

    #endregion

    public bool EmbedWindow(IntPtr childWindowHandle, IntPtr parentWindowHandle)
    {
        if (childWindowHandle == IntPtr.Zero || parentWindowHandle == IntPtr.Zero)
            return false;

        try
        {
            // Get current window style
            uint style = GetWindowLong(childWindowHandle, GWL_STYLE);

            // Remove window decorations and add child style
            style &= ~WS_CAPTION;      // Remove title bar
            style &= ~WS_THICKFRAME;   // Remove resize border
            style &= ~WS_SYSMENU;      // Remove system menu
            style |= WS_CHILD;         // Add child window style
            style |= WS_VISIBLE;       // Keep visible

            // Apply new style
            SetWindowLong(childWindowHandle, GWL_STYLE, style);

            // Set the parent window
            IntPtr result = SetParent(childWindowHandle, parentWindowHandle);
            if (result == IntPtr.Zero)
            {
                return false;
            }

            // Update the window to reflect style changes
            SetWindowPos(childWindowHandle, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);

            return true;
        }
        catch
        {
            return false;
        }
    }

    public bool ResizeEmbeddedWindow(IntPtr windowHandle, int width, int height)
    {
        if (windowHandle == IntPtr.Zero)
            return false;

        try
        {
            return MoveWindow(windowHandle, 0, 0, width, height, true);
        }
        catch
        {
            return false;
        }
    }

    public void CleanupEmbeddedWindow(IntPtr windowHandle)
    {
        if (windowHandle == IntPtr.Zero)
            return;

        try
        {
            // Restore the window to a normal state before cleanup
            uint style = GetWindowLong(windowHandle, GWL_STYLE);
            style &= ~WS_CHILD;
            style |= WS_CAPTION | WS_THICKFRAME | WS_SYSMENU;
            SetWindowLong(windowHandle, GWL_STYLE, style);

            // Remove parent
            SetParent(windowHandle, IntPtr.Zero);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}
