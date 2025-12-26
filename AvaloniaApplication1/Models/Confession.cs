using System;

namespace AvaloniaApplication1.Models;

public class Confession
{
    public string Id { get; init; } = Guid.NewGuid().ToString();
    public string Text { get; init; } = string.Empty;
    public string? MonkResponse { get; init; }
    public DateTime ConfessedAt { get; init; } = DateTime.UtcNow;
    public double Opacity { get; set; } = 1.0; // For fade animation
    public double VerticalOffset { get; set; } = 0.0; // For floating animation
}
