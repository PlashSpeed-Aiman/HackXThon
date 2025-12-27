using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaApplication1.Converters.Common;

public class AngleToArcConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not double angle)
            return null;

        // Clamp angle to 0-360 range
        angle = Math.Max(0, Math.Min(360, angle));

        var radius = 90.0; // Ring radius
        var centerX = 200.0;
        var centerY = 200.0;

        // Start at top (12 o'clock position)
        var startX = centerX;
        var startY = centerY - radius;

        // Calculate end point based on angle
        // Subtract 90 to start from top instead of right
        var angleRad = (angle - 90) * Math.PI / 180.0;
        var endX = centerX + radius * Math.Cos(angleRad);
        var endY = centerY + radius * Math.Sin(angleRad);

        // Use large arc if angle > 180 degrees
        var isLargeArc = angle > 180;

        // Create path geometry
        var pathGeometry = new PathGeometry();
        var pathFigure = new PathFigure
        {
            StartPoint = new Point(startX, startY),
            IsClosed = false
        };

        // Only add arc if angle > 0
        if (angle > 0)
        {
            pathFigure.Segments.Add(new ArcSegment
            {
                Point = new Point(endX, endY),
                Size = new Size(radius, radius),
                SweepDirection = SweepDirection.Clockwise,
                IsLargeArc = isLargeArc
            });
        }

        pathGeometry.Figures.Add(pathFigure);

        return pathGeometry;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
