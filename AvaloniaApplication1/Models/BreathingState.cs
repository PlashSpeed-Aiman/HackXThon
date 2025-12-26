namespace AvaloniaApplication1.Models;

public enum BreathingPhase
{
    Idle,
    Inhale,
    Hold,
    Exhale
}

public class BreathingState
{
    public BreathingPhase Phase { get; set; } = BreathingPhase.Idle;
    public int CyclesCompleted { get; set; } = 0;
    public double CircleScale { get; set; } = 1.0; // 1.0 to 2.0 for animation
    public string InstructionText { get; set; } = "Press Start to Begin";
}
