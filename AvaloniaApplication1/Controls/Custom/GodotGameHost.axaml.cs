using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;
using AvaloniaApplication1.Services;
using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.Services.PlatformInterop;

namespace AvaloniaApplication1.Controls.Custom;

/// <summary>
/// Custom control for hosting a Godot game window using native window embedding.
/// </summary>
public class GodotGameHost : NativeControlHost
{
    private IGodotGameService? _gameService;
    private IWindowEmbedder? _windowEmbedder;
    private IntPtr _gameWindowHandle = IntPtr.Zero;
    private bool _isEmbedded;

    /// <summary>
    /// Dependency property for the game service.
    /// </summary>
    public static readonly StyledProperty<IGodotGameService?> GameServiceProperty =
        AvaloniaProperty.Register<GodotGameHost, IGodotGameService?>(nameof(GameService));

    /// <summary>
    /// Gets or sets the game service used to manage the game process.
    /// </summary>
    public IGodotGameService? GameService
    {
        get => GetValue(GameServiceProperty);
        set => SetValue(GameServiceProperty, value);
    }

    static GodotGameHost()
    {
        GameServiceProperty.Changed.AddClassHandler<GodotGameHost>((control, args) =>
        {
            control.OnGameServiceChanged(args);
        });
    }

    public GodotGameHost()
    {
        try
        {
            _windowEmbedder = WindowEmbedderFactory.Create();
        }
        catch (PlatformNotSupportedException ex)
        {
            // Platform not supported - the control will not function but won't crash
            System.Diagnostics.Debug.WriteLine($"Platform not supported for window embedding: {ex.Message}");
        }
    }

    private void OnGameServiceChanged(AvaloniaPropertyChangedEventArgs args)
    {
        if (args.OldValue is IGodotGameService oldService)
        {
            oldService.GameStateChanged -= OnGameStateChanged;
        }

        _gameService = args.NewValue as IGodotGameService;

        if (_gameService != null)
        {
            _gameService.GameStateChanged += OnGameStateChanged;
        }
    }

    private void OnGameStateChanged(object? sender, bool isRunning)
    {
        if (isRunning && !_isEmbedded)
        {
            // Game started, try to embed it
            EmbedGameWindow();
        }
        else if (!isRunning && _isEmbedded)
        {
            // Game stopped, cleanup
            _isEmbedded = false;
            _gameWindowHandle = IntPtr.Zero;
        }
    }

    protected override IPlatformHandle CreateNativeControlCore(IPlatformHandle parent)
    {
        if (_gameService == null || _windowEmbedder == null)
        {
            return base.CreateNativeControlCore(parent);
        }

        _gameWindowHandle = _gameService.GetGameWindowHandle();

        if (_gameWindowHandle != IntPtr.Zero)
        {
            // Embed the game window
            bool success = _windowEmbedder.EmbedWindow(_gameWindowHandle, parent.Handle);

            if (success)
            {
                _isEmbedded = true;

                // Resize to current bounds
                if (Bounds.Width > 0 && Bounds.Height > 0)
                {
                    _windowEmbedder.ResizeEmbeddedWindow(
                        _gameWindowHandle,
                        (int)Bounds.Width,
                        (int)Bounds.Height);
                }

                return new PlatformHandle(_gameWindowHandle, "HWND");
            }
        }

        return base.CreateNativeControlCore(parent);
    }

    protected override void DestroyNativeControlCore(IPlatformHandle control)
    {
        if (_windowEmbedder != null && _gameWindowHandle != IntPtr.Zero)
        {
            _windowEmbedder.CleanupEmbeddedWindow(_gameWindowHandle);
        }

        _isEmbedded = false;
        _gameWindowHandle = IntPtr.Zero;

        base.DestroyNativeControlCore(control);
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);

        if (_isEmbedded && _windowEmbedder != null && _gameWindowHandle != IntPtr.Zero)
        {
            _windowEmbedder.ResizeEmbeddedWindow(
                _gameWindowHandle,
                (int)e.NewSize.Width,
                (int)e.NewSize.Height);
        }
    }

    private void EmbedGameWindow()
    {
        // Trigger recreation of the native control
        if (IsInitialized)
        {
            InvalidateVisual();
        }
    }
}
