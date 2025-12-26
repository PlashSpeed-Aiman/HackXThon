using System;
using System.Threading.Tasks;
using System.Timers;
using Avalonia.Threading;
using AvaloniaApplication1.Models;
using AvaloniaApplication1.ViewModels.Base;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaApplication1.ViewModels;

public partial class BreathingViewModel : ViewModelBase, IDisposable
{
    private readonly Timer _breathingTimer;
    private const int RequiredCycles = 3;
    private const int InhaleDurationMs = 4000;  // 4 seconds
    private const int HoldDurationMs = 2000;     // 2 seconds
    private const int ExhaleDurationMs = 4000;   // 4 seconds
    private const int TimerIntervalMs = 50;      // Update every 50ms for smooth animation

    private DateTime _phaseStartTime;
    private int _currentPhaseDuration;

    [ObservableProperty]
    private BreathingPhase _currentPhase = BreathingPhase.Idle;

    [ObservableProperty]
    private int _cyclesCompleted = 0;

    [ObservableProperty]
    private double _circleScale = 1.0;

    [ObservableProperty]
    private string _instructionText = "Press Start to Begin";

    [ObservableProperty]
    private bool _isDeployEnabled = false;

    [ObservableProperty]
    private bool _isBreathing = false;

    [ObservableProperty]
    private bool _showDeployConfirmation = false;

    [ObservableProperty]
    private string _deployMessage = string.Empty;

    public int RequiredCyclesCount => RequiredCycles;

    public BreathingViewModel()
    {
        _breathingTimer = new Timer(TimerIntervalMs);
        _breathingTimer.Elapsed += OnBreathingTimerElapsed;
    }

    [RelayCommand]
    private void StartBreathing()
    {
        if (IsBreathing) return;

        // Reset state
        CyclesCompleted = 0;
        IsDeployEnabled = false;
        ShowDeployConfirmation = false;
        IsBreathing = true;

        // Start with inhale
        StartPhase(BreathingPhase.Inhale, InhaleDurationMs);
        _breathingTimer.Start();
    }

    [RelayCommand]
    private void StopBreathing()
    {
        _breathingTimer.Stop();
        IsBreathing = false;
        CurrentPhase = BreathingPhase.Idle;
        CircleScale = 1.0;
        InstructionText = "Press Start to Begin";
    }

    [RelayCommand(CanExecute = nameof(IsDeployEnabled))]
    private void Deploy()
    {
        ShowDeployConfirmation = true;
        DeployMessage = GenerateCalmingMessage();

        // Auto-hide after 5 seconds
        Task.Delay(5000).ContinueWith(_ => ShowDeployConfirmation = false);
    }

    private void StartPhase(BreathingPhase phase, int durationMs)
    {
        CurrentPhase = phase;
        _phaseStartTime = DateTime.UtcNow;
        _currentPhaseDuration = durationMs;

        InstructionText = phase switch
        {
            BreathingPhase.Inhale => "Breathe In...",
            BreathingPhase.Hold => "Hold...",
            BreathingPhase.Exhale => "Breathe Out...",
            _ => "Press Start to Begin"
        };
    }

    private void OnBreathingTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            if (!IsBreathing) return;

            var elapsed = (DateTime.UtcNow - _phaseStartTime).TotalMilliseconds;
            var progress = Math.Min(1.0, elapsed / _currentPhaseDuration);

            // Update circle scale based on phase
            switch (CurrentPhase)
            {
                case BreathingPhase.Inhale:
                    CircleScale = 1.0 + progress; // Scale from 1.0 to 2.0
                    break;
                case BreathingPhase.Hold:
                    CircleScale = 2.0; // Stay at max
                    break;
                case BreathingPhase.Exhale:
                    CircleScale = 2.0 - progress; // Scale from 2.0 to 1.0
                    break;
            }

            // Check if phase is complete
            if (elapsed >= _currentPhaseDuration)
            {
                AdvancePhase();
            }
        });
    }

    private void AdvancePhase()
    {
        switch (CurrentPhase)
        {
            case BreathingPhase.Inhale:
                StartPhase(BreathingPhase.Hold, HoldDurationMs);
                break;
            case BreathingPhase.Hold:
                StartPhase(BreathingPhase.Exhale, ExhaleDurationMs);
                break;
            case BreathingPhase.Exhale:
                CyclesCompleted++;

                if (CyclesCompleted >= RequiredCycles)
                {
                    // Enable deploy button
                    IsDeployEnabled = true;
                    StopBreathing();
                    InstructionText = "You are now calm. Deploy button unlocked!";
                    DeployCommand.NotifyCanExecuteChanged();
                }
                else
                {
                    // Start next cycle
                    StartPhase(BreathingPhase.Inhale, InhaleDurationMs);
                }
                break;
        }
    }

    private string GenerateCalmingMessage()
    {
        var messages = new[]
        {
            "Your code flows like a gentle stream. Deployment complete in your mind.",
            "You have achieved inner peace. The servers smile upon you.",
            "In stillness, there is no production bug. You are ready.",
            "The deployment is merely a thought, and thoughts are light as air.",
            "You have breathed. You have reflected. You are at one with the pipeline."
        };

        return messages[new Random().Next(messages.Length)];
    }

    public void Dispose()
    {
        _breathingTimer?.Stop();
        _breathingTimer?.Dispose();
    }
}
