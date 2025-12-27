using System;
using System.Threading.Tasks;
using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class HaikuViewModel : ViewModelBase
{
    private readonly IGeminiService _geminiService;
    private readonly IAudioPlayerService _audioPlayerService;
    private bool _audioInitialized;

    [ObservableProperty]
    private string _frustrationText = string.Empty;

    [ObservableProperty]
    private string? _generatedHaiku;

    [ObservableProperty]
    private bool _isGenerating;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isMusicPlaying;

    public HaikuViewModel(IGeminiService geminiService, IAudioPlayerService audioPlayerService)
    {
        _geminiService = geminiService;
        _audioPlayerService = audioPlayerService;

        // Subscribe to playback state changes
        _audioPlayerService.PlaybackStateChanged += OnPlaybackStateChanged;
    }

    private bool CanGenerate => !string.IsNullOrWhiteSpace(FrustrationText) && !IsGenerating;

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private async Task GenerateHaikuAsync()
    {
        // Clear previous results
        GeneratedHaiku = null;
        ErrorMessage = null;
        IsGenerating = true;

        try
        {
            var response = await _geminiService.GenerateHaikuAsync(FrustrationText);

            if (response.Success)
            {
                GeneratedHaiku = response.Haiku;
            }
            else
            {
                ErrorMessage = response.ErrorMessage;
            }
        }
        finally
        {
            IsGenerating = false;
        }
    }

    partial void OnFrustrationTextChanged(string value)
    {
        // Notify that CanGenerate might have changed
        GenerateHaikuCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Handles playback state changes from the audio service.
    /// </summary>
    private void OnPlaybackStateChanged(object? sender, bool isPlaying)
    {
        IsMusicPlaying = isPlaying;
    }

    /// <summary>
    /// Called when the view is loaded. Initializes and auto-plays music.
    /// </summary>
    public async Task OnViewLoadedAsync()
    {
        if (!_audioInitialized)
        {
            try
            {
                await _audioPlayerService.InitializeAsync("avares://AvaloniaApplication1/Assets/Audio/ambient-japanese.mp3");
                _audioInitialized = true;

                // Auto-play music
                await _audioPlayerService.PlayAsync();
                // State will be updated via PlaybackStateChanged event
            }
            catch (Exception ex)
            {
                // If audio initialization fails, log but don't crash the app
                ErrorMessage = $"Failed to load background music: {ex.Message}";
            }
        }
    }

    /// <summary>
    /// Toggles music playback between play and pause.
    /// </summary>
    [RelayCommand]
    private async Task ToggleMusicAsync()
    {
        try
        {
            if (_audioPlayerService.IsPlaying)
            {
                await _audioPlayerService.PauseAsync();
            }
            else
            {
                await _audioPlayerService.PlayAsync();
            }

            // State will be updated via PlaybackStateChanged event
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Music playback error: {ex.Message}";
        }
    }
}
