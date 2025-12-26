using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.Services.Interfaces;
using AvaloniaApplication1.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class ConfessionalViewModel : ViewModelBase, IDisposable
{
    private readonly IConfessionService _confessionService;
    private readonly Timer _fadeTimer;

    [ObservableProperty]
    private string _confessionText = string.Empty;

    [ObservableProperty]
    private string? _currentAbsolution;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string? _errorMessage;

    public ObservableCollection<Confession> FloatingConfessions { get; } = new();

    public ConfessionalViewModel(IConfessionService confessionService)
    {
        _confessionService = confessionService;

        // Timer for fade/float animations (updates every 50ms)
        _fadeTimer = new Timer(50);
        _fadeTimer.Elapsed += OnFadeTimerElapsed;
        _fadeTimer.Start();
    }

    private bool CanConfess => !string.IsNullOrWhiteSpace(ConfessionText) && !IsProcessing;

    [RelayCommand(CanExecute = nameof(CanConfess))]
    private async Task ConfessAsync()
    {
        CurrentAbsolution = null;
        ErrorMessage = null;
        IsProcessing = true;

        try
        {
            var response = await _confessionService.GetAbsolutionAsync(ConfessionText);

            if (response.Success)
            {
                CurrentAbsolution = response.AbsolutionMessage;

                // Add to floating confessions
                var confession = new Confession
                {
                    Text = ConfessionText,
                    MonkResponse = response.AbsolutionMessage,
                    ConfessedAt = DateTime.UtcNow,
                    Opacity = 1.0,
                    VerticalOffset = 0.0
                };

                FloatingConfessions.Add(confession);

                // Clear input
                ConfessionText = string.Empty;
            }
            else
            {
                ErrorMessage = response.ErrorMessage;
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }

    partial void OnConfessionTextChanged(string value)
    {
        ConfessCommand.NotifyCanExecuteChanged();
    }

    private void OnFadeTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            // Update opacity and vertical offset for floating animations
            var toRemove = FloatingConfessions.Where(c => c.Opacity <= 0).ToList();

            foreach (var confession in FloatingConfessions)
            {
                // Fade out over ~6 seconds (120 ticks * 50ms)
                confession.Opacity = Math.Max(0, confession.Opacity - 0.0083);

                // Float upward slowly
                confession.VerticalOffset -= 0.5;
            }

            // Remove fully faded confessions
            foreach (var confession in toRemove)
            {
                FloatingConfessions.Remove(confession);
            }
        });
    }

    public void Dispose()
    {
        _fadeTimer?.Stop();
        _fadeTimer?.Dispose();
    }
}
