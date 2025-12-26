using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaApplication1.Services.Interfaces;
using NetCoreAudio;

namespace AvaloniaApplication1.Services;

/// <summary>
/// NetCoreAudio-based audio player service implementation.
/// Handles cross-platform audio playback using NetCoreAudio library.
/// </summary>
public class NetCoreAudioPlayerService : IAudioPlayerService, IDisposable
{
    private readonly Player _player;
    private string? _cachedFilePath;
    private bool _isPaused;
    private bool _disposed;

    public bool IsPlaying => !_disposed && _player.Playing;

    public event EventHandler<bool>? PlaybackStateChanged;

    public NetCoreAudioPlayerService()
    {
        _player = new Player();
    }

    /// <summary>
    /// Initializes the audio player with an embedded resource.
    /// </summary>
    public async Task InitializeAsync(string resourcePath)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(NetCoreAudioPlayerService));

        try
        {
            // Extract embedded resource to temp file (NetCoreAudio requires file path)
            _cachedFilePath = await ExtractResourceToTempFileAsync(resourcePath);

            // Subscribe to playback finished for looping
            _player.PlaybackFinished += OnPlaybackFinished;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize audio player: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Starts or resumes audio playback.
    /// </summary>
    public async Task PlayAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(NetCoreAudioPlayerService));

        if (_cachedFilePath == null)
            throw new InvalidOperationException("Audio player not initialized. Call InitializeAsync first.");

        try
        {
            if (_isPaused)
            {
                // Resume from pause
                _player.Resume();
                _isPaused = false;
            }
            else
            {
                // Start new playback
                await _player.Play(_cachedFilePath);
            }

            RaisePlaybackStateChanged(true);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to play audio: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Pauses audio playback.
    /// </summary>
    public Task PauseAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(NetCoreAudioPlayerService));

        if (_cachedFilePath == null)
            throw new InvalidOperationException("Audio player not initialized. Call InitializeAsync first.");

        try
        {
            _player.Pause();
            _isPaused = true;
            RaisePlaybackStateChanged(false);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to pause audio: {ex.Message}", ex);
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Extracts an embedded Avalonia resource to a temporary file.
    /// NetCoreAudio requires a file path and cannot play from streams.
    /// </summary>
    private async Task<string> ExtractResourceToTempFileAsync(string resourcePath)
    {
        var uri = new Uri(resourcePath);

        // Open the embedded resource stream using AssetLoader
        using var resourceStream = AssetLoader.Open(uri);

        // Create temp file with appropriate extension
        var extension = Path.GetExtension(resourcePath);
        var tempFilePath = Path.Combine(Path.GetTempPath(), $"avalonia_audio_{Guid.NewGuid()}{extension}");

        // Copy resource to temp file
        using (var fileStream = File.Create(tempFilePath))
        {
            await resourceStream.CopyToAsync(fileStream);
        }

        return tempFilePath;
    }

    /// <summary>
    /// Handles the PlaybackFinished event to implement looping.
    /// </summary>
    private async void OnPlaybackFinished(object? sender, EventArgs e)
    {
        if (_disposed || _cachedFilePath == null)
            return;

        // Restart playback for seamless looping
        try
        {
            _isPaused = false;
            await _player.Play(_cachedFilePath);
            RaisePlaybackStateChanged(true);
        }
        catch
        {
            // Ignore errors during auto-restart
        }
    }

    /// <summary>
    /// Raises the PlaybackStateChanged event on the UI thread.
    /// </summary>
    private void RaisePlaybackStateChanged(bool isPlaying)
    {
        // Marshal to UI thread for safe property updates
        Dispatcher.UIThread.Post(() =>
        {
            PlaybackStateChanged?.Invoke(this, isPlaying);
        });
    }

    /// <summary>
    /// Disposes the audio player and cleans up resources.
    /// </summary>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        // Unsubscribe from events
        _player.PlaybackFinished -= OnPlaybackFinished;

        // Stop playback
        try
        {
            _player.Stop();
        }
        catch
        {
            // Ignore errors during disposal
        }

        // NetCoreAudio Player doesn't implement IDisposable, no need to dispose

        // Delete temp file
        if (_cachedFilePath != null && File.Exists(_cachedFilePath))
        {
            try
            {
                File.Delete(_cachedFilePath);
            }
            catch
            {
                // Ignore errors when cleaning up temp files
            }
        }

        _cachedFilePath = null;
    }
}
