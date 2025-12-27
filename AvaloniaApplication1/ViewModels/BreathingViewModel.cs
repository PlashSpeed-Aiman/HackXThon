using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    private const int ParticleCount = 12;        // Particles in radial burst

    // Color constants for transitions
    private static readonly (byte R, byte G, byte B) BlueLight = (173, 216, 230);   // #ADD8E6
    private static readonly (byte R, byte G, byte B) BlueStroke = (70, 130, 180);   // #4682B4
    private static readonly (byte R, byte G, byte B) PurpleLight = (221, 160, 221); // #DDA0DD
    private static readonly (byte R, byte G, byte B) PurpleStroke = (147, 112, 219); // #9370DB

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

    // Glow effect properties
    [ObservableProperty]
    private double _glowIntensity = 0.0;

    [ObservableProperty]
    private double _shadowBlur = 20.0;

    [ObservableProperty]
    private string _boxShadowString = "0 0 20 0 #4682B4";

    // Color transition properties
    [ObservableProperty]
    private string _circleFillColor = "#ADD8E6";

    [ObservableProperty]
    private string _circleStrokeColor = "#4682B4";

    [ObservableProperty]
    private string _glowColor = "#4682B4";

    // Progress ring properties
    [ObservableProperty]
    private double _progressAngle = 0.0;

    [ObservableProperty]
    private string _progressRingColor = "#4682B4";

    // Particle effects
    public ObservableCollection<BreathingParticle> Particles { get; } = new();

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
        Particles.Clear();

        // Reset visual effects
        GlowIntensity = 0.0;
        ShadowBlur = 20.0;
        CircleFillColor = "#ADD8E6";
        CircleStrokeColor = "#4682B4";
        GlowColor = "#4682B4";
        ProgressAngle = 0.0;

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

        // Reset visual effects
        GlowIntensity = 0.0;
        ProgressAngle = 0.0;
        Particles.Clear();
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

        // Update progress ring color based on phase
        ProgressRingColor = phase switch
        {
            BreathingPhase.Inhale => "#4682B4",   // Blue
            BreathingPhase.Hold => "#FFA500",     // Orange
            BreathingPhase.Exhale => "#9370DB",   // Purple
            _ => "#CCCCCC"                         // Gray
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

            // Update glow effect
            switch (CurrentPhase)
            {
                case BreathingPhase.Inhale:
                    GlowIntensity = progress; // 0.0 → 1.0
                    break;
                case BreathingPhase.Hold:
                    GlowIntensity = 1.0; // Max glow
                    break;
                case BreathingPhase.Exhale:
                    GlowIntensity = 1.0 - progress; // 1.0 → 0.0
                    break;
                case BreathingPhase.Idle:
                    GlowIntensity = 0.0;
                    break;
            }
            ShadowBlur = 20.0 + (GlowIntensity * 40.0); // 20px → 60px
            BoxShadowString = $"0 0 {ShadowBlur:F0} 0 {GlowColor}";

            // Update progress ring
            ProgressAngle = progress * 360.0;

            // Update color transitions
            UpdateColorTransitions();

            // Update particles
            UpdateParticles();

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
                // Spawn particles on exhale completion
                SpawnParticleBurst();

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

    private void UpdateColorTransitions()
    {
        // Calculate overall progress through the breathing cycle
        double colorProgress = 0.0;
        var totalCycleDuration = InhaleDurationMs + HoldDurationMs + ExhaleDurationMs;
        var elapsed = (DateTime.UtcNow - _phaseStartTime).TotalMilliseconds;

        switch (CurrentPhase)
        {
            case BreathingPhase.Inhale:
                // 0.0 → 0.5 (blue → purple)
                colorProgress = (elapsed / totalCycleDuration) * 0.5;
                break;
            case BreathingPhase.Hold:
                // 0.5 (mid-purple)
                colorProgress = (InhaleDurationMs / totalCycleDuration) * 0.5 +
                               (elapsed / totalCycleDuration) * 0.2;
                break;
            case BreathingPhase.Exhale:
                // 0.5 → 1.0 (purple → blue)
                colorProgress = 0.5 + (elapsed / ExhaleDurationMs) * 0.5;
                break;
            case BreathingPhase.Idle:
                colorProgress = 0.0; // Blue
                break;
        }

        // Interpolate colors based on progress
        if (colorProgress <= 0.5)
        {
            // Blue → Purple (0.0 to 0.5)
            var t = colorProgress * 2.0; // Normalize to 0.0-1.0
            CircleFillColor = LerpColor(BlueLight, PurpleLight, t);
            CircleStrokeColor = LerpColor(BlueStroke, PurpleStroke, t);
        }
        else
        {
            // Purple → Blue (0.5 to 1.0)
            var t = (colorProgress - 0.5) * 2.0; // Normalize to 0.0-1.0
            CircleFillColor = LerpColor(PurpleLight, BlueLight, t);
            CircleStrokeColor = LerpColor(PurpleStroke, BlueStroke, t);
        }

        GlowColor = CircleStrokeColor;
    }

    private void UpdateParticles()
    {
        // Update existing particles
        var particlesToRemove = new List<BreathingParticle>();

        foreach (var particle in Particles)
        {
            particle.X += particle.VelocityX;
            particle.Y += particle.VelocityY;
            particle.Opacity = Math.Max(0, particle.Opacity - 0.02); // Fade over ~2.5s
            particle.VelocityY += 0.05; // Slight downward gravity

            if (particle.Opacity <= 0)
            {
                particlesToRemove.Add(particle);
            }
        }

        // Remove faded particles
        foreach (var particle in particlesToRemove)
        {
            Particles.Remove(particle);
        }
    }

    private void SpawnParticleBurst()
    {
        var centerX = 200.0; // Canvas center
        var centerY = 200.0;
        var random = new Random();

        for (int i = 0; i < ParticleCount; i++)
        {
            var angle = (i / (double)ParticleCount) * Math.PI * 2.0;
            var speed = 2.0 + random.NextDouble() * 1.5; // 2.0-3.5px per tick

            var particle = new BreathingParticle
            {
                X = centerX + Math.Cos(angle) * 75,  // Start at circle edge
                Y = centerY + Math.Sin(angle) * 75,
                VelocityX = Math.Cos(angle) * speed,
                VelocityY = Math.Sin(angle) * speed,
                Opacity = 1.0,
                Size = 6.0 + random.NextDouble() * 4.0, // 6-10px
                Color = CircleFillColor // Current color at exhale completion
            };

            Particles.Add(particle);
        }
    }

    private static string LerpColor((byte R, byte G, byte B) from, (byte R, byte G, byte B) to, double t)
    {
        var r = (byte)(from.R + (to.R - from.R) * t);
        var g = (byte)(from.G + (to.G - from.G) * t);
        var b = (byte)(from.B + (to.B - from.B) * t);
        return $"#{r:X2}{g:X2}{b:X2}";
    }

    public void Dispose()
    {
        _breathingTimer?.Stop();
        _breathingTimer?.Dispose();
    }
}
