namespace AvaloniaApplication1.Models;

public class BreathingParticle
{
    public double X { get; set; }
    public double Y { get; set; }
    public double VelocityX { get; set; }
    public double VelocityY { get; set; }
    public double Opacity { get; set; } = 1.0;
    public double Size { get; set; } = 8.0;
    public string Color { get; set; } = "#DDA0DD"; // Purple at exhale completion
}
