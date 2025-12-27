using System;
using System.Threading.Tasks;

namespace AvaloniaApplication1.Services.Interfaces;

/// <summary>
/// Service interface for audio playback functionality.
/// Provides methods to control music playback (play, pause, stop) and track playback state.
/// </summary>
public interface IAudioPlayerService
{
    /// <summary>
    /// Gets a value indicating whether audio is currently playing.
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Initializes the audio player with a resource path.
    /// </summary>
    /// <param name="resourcePath">The Avalonia resource URI (e.g., "avares://App/Assets/Audio/music.mp3")</param>
    Task InitializeAsync(string resourcePath);

    /// <summary>
    /// Starts or resumes audio playback.
    /// </summary>
    Task PlayAsync();

    /// <summary>
    /// Stops audio playback. Music will restart from the beginning when PlayAsync() is called.
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// Stops audio playback and releases resources.
    /// </summary>
    void Dispose();

    /// <summary>
    /// Event raised when playback state changes.
    /// </summary>
    event EventHandler<bool>? PlaybackStateChanged;
}
