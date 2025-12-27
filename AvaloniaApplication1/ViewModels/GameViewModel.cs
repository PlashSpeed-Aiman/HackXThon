using System;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class GameViewModel : ViewModelBase
{
    private readonly IGodotGameService _gameService;
    private readonly IGameConfigurationService _configService;

    [ObservableProperty]
    private bool _isGameRunning;

    [ObservableProperty]
    private bool _isGameLoading;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private string? _gameExecutablePath;

    [ObservableProperty]
    private string _statusMessage = "No game loaded. Click Browse to select a Godot game executable.";

    public GameViewModel(IGodotGameService gameService, IGameConfigurationService configService)
    {
        _gameService = gameService;
        _configService = configService;

        // Subscribe to service events
        _gameService.GameStateChanged += OnGameStateChanged;
        _gameService.ErrorOccurred += OnGameErrorOccurred;

        // Load saved path if available
        GameExecutablePath = _configService.GetGameExecutablePath();
    }

    /// <summary>
    /// Gets the game service to be used by the GodotGameHost control.
    /// </summary>
    public IGodotGameService GameService => _gameService;

    private bool CanStartGame => !string.IsNullOrWhiteSpace(GameExecutablePath) &&
                                  !IsGameRunning &&
                                  !IsGameLoading;

    private bool CanStopGame => IsGameRunning && !IsGameLoading;

    [RelayCommand(CanExecute = nameof(CanStartGame))]
    private async Task StartGameAsync()
    {
        if (string.IsNullOrWhiteSpace(GameExecutablePath))
        {
            ErrorMessage = "Please select a game executable first.";
            return;
        }

        // Validate the executable
        if (!_configService.ValidateGameExecutable(GameExecutablePath))
        {
            ErrorMessage = "The selected file is not a valid executable or does not exist.";
            return;
        }

        ErrorMessage = null;
        IsGameLoading = true;
        StatusMessage = "Starting game...";

        try
        {
            // Save the path
            _configService.SetGameExecutablePath(GameExecutablePath);

            // Start the game
            bool success = await _gameService.StartGameAsync(GameExecutablePath);

            if (success)
            {
                StatusMessage = "Game is running. Enjoy!";
            }
            else
            {
                StatusMessage = "Failed to start game. See error message above.";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error starting game: {ex.Message}";
            StatusMessage = "Failed to start game.";
        }
        finally
        {
            IsGameLoading = false;
            UpdateCommandStates();
        }
    }

    [RelayCommand(CanExecute = nameof(CanStopGame))]
    private async Task StopGameAsync()
    {
        IsGameLoading = true;
        StatusMessage = "Stopping game...";

        try
        {
            await _gameService.StopGameAsync();
            StatusMessage = "Game stopped.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error stopping game: {ex.Message}";
            StatusMessage = "Error stopping game.";
        }
        finally
        {
            IsGameLoading = false;
            UpdateCommandStates();
        }
    }

    [RelayCommand]
    private async Task BrowseForGameAsync()
    {
        // Note: File picker requires access to the window, which we don't have in the ViewModel
        // This will be handled in the View's code-behind
        // For now, this is a placeholder that could be wired up
        await Task.CompletedTask;
    }

    private void OnGameStateChanged(object? sender, bool isRunning)
    {
        IsGameRunning = isRunning;

        if (!isRunning && !IsGameLoading)
        {
            StatusMessage = "Game has stopped.";
        }

        UpdateCommandStates();
    }

    private void OnGameErrorOccurred(object? sender, string message)
    {
        ErrorMessage = message;
    }

    private void UpdateCommandStates()
    {
        StartGameCommand.NotifyCanExecuteChanged();
        StopGameCommand.NotifyCanExecuteChanged();
    }

    partial void OnGameExecutablePathChanged(string? value)
    {
        UpdateCommandStates();
    }
}
