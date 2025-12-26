using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaApplication1.Services.Interfaces;
using LibVLCSharp.Shared;

namespace AvaloniaApplication1.Services;

/// <summary>
/// LibVLC-based audio player service implementation.
/// Handles cross-platform audio playback using LibVLCSharp.
/// </summary>
public class LibVlcAudioPlayerService : IAudioPlayerService, IDisposable
{
    private LibVLC? _libVlc;
    private MediaPlayer? _mediaPlayer;
    private Media? _media;
    private string? _tempAudioFilePath;
    private bool _disposed;

    public bool IsPlaying => _mediaPlayer?.IsPlaying ?? false;

    public event EventHandler<bool>? PlaybackStateChanged;

    /// <summary>
    /// Initializes the audio player with an embedded resource.
    /// </summary>
    public async Task InitializeAsync(string resourcePath)
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LibVlcAudioPlayerService));

        try
        {
            // Initialize LibVLC core
            Core.Initialize();
            _libVlc = new LibVLC();

            // Extract embedded resource to temp file (VLC requires file path)
            _tempAudioFilePath = await ExtractResourceToTempFileAsync(resourcePath);

            // Create media and player
            _media = new Media(_libVlc, _tempAudioFilePath, FromType.FromPath);
            _mediaPlayer = new MediaPlayer(_media);

            // Configure looping: restart when playback ends
            _mediaPlayer.EndReached += OnEndReached;
            _mediaPlayer.Playing += (s, e) => RaisePlaybackStateChanged(true);
            _mediaPlayer.Paused += (s, e) => RaisePlaybackStateChanged(false);
            _mediaPlayer.Stopped += (s, e) => RaisePlaybackStateChanged(false);
        }
        catch (Exception ex)
        {
            // Clean up on initialization failure
            Dispose();
            throw new InvalidOperationException($"Failed to initialize audio player: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Starts or resumes audio playback.
    /// </summary>
    public Task PlayAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LibVlcAudioPlayerService));

        if (_mediaPlayer == null)
            throw new InvalidOperationException("Audio player not initialized. Call InitializeAsync first.");

        _mediaPlayer.Play();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Pauses audio playback.
    /// </summary>
    public Task PauseAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(LibVlcAudioPlayerService));

        if (_mediaPlayer == null)
            throw new InvalidOperationException("Audio player not initialized. Call InitializeAsync first.");

        _mediaPlayer.Pause();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Extracts an embedded Avalonia resource to a temporary file.
    /// LibVLC requires a file path and cannot stream from memory reliably.
    /// </summary>
    private async Task<string> ExtractResourceToTempFileAsync(string resourcePath)
    {
        var uri = new Uri(resourcePath);
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();

        if (assets == null)
            throw new InvalidOperationException("Asset loader service not available.");

        // Open the embedded resource stream
        using var resourceStream = assets.Open(uri);

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
    /// Handles the EndReached event to implement looping.
    /// </summary>
    private void OnEndReached(object? sender, EventArgs e)
    {
        // Restart playback for seamless looping
        _mediaPlayer?.Stop();
        _mediaPlayer?.Play();
    }

    /// <summary>
    /// Raises the PlaybackStateChanged event on the UI thread.
    /// </summary>
    private void RaisePlaybackStateChanged(bool isPlaying)
    {
        // LibVLC events fire on background threads, so marshal to UI thread
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
        if (_mediaPlayer != null)
        {
            _mediaPlayer.EndReached -= OnEndReached;
            _mediaPlayer.Stop();
        }

        // Dispose LibVLC objects
        _mediaPlayer?.Dispose();
        _media?.Dispose();
        _libVlc?.Dispose();

        // Delete temp file
        if (_tempAudioFilePath != null && File.Exists(_tempAudioFilePath))
        {
            try
            {
                File.Delete(_tempAudioFilePath);
            }
            catch
            {
                // Ignore errors when cleaning up temp files
            }
        }

        _mediaPlayer = null;
        _media = null;
        _libVlc = null;
        _tempAudioFilePath = null;
    }
}
