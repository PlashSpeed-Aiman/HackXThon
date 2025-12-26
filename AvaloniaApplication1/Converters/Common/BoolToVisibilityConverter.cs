using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AvaloniaApplication1.Converters.Common;

/// <summary>
/// Converts a boolean value to visibility (IsVisible property).
/// This is a common converter used to show/hide UI elements based on boolean conditions.
/// </summary>
/// <remarks>
/// Usage in XAML:
/// 1. Add xmlns to your view: xmlns:converters="using:AvaloniaApplication1.Converters.Common"
/// 2. Add converter as resource:
///    <converters:BoolToVisibilityConverter x:Key="BoolToVisibility"/>
/// 3. Use in binding:
///    IsVisible="{Binding IsEnabled, Converter={StaticResource BoolToVisibility}}"
///
/// Parameter support:
/// - Pass "Invert" or "!" to invert the logic (true becomes hidden, false becomes visible)
/// </remarks>
public class BoolToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// Converts a boolean value to visibility.
    /// </summary>
    /// <param name="value">The boolean value to convert</param>
    /// <param name="targetType">The target type (not used)</param>
    /// <param name="parameter">Optional parameter: "Invert" or "!" to invert the logic</param>
    /// <param name="culture">The culture to use (not used)</param>
    /// <returns>True if visible, false if hidden</returns>
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle null values
        if (value == null)
            return false;

        // Type checking
        if (value is not bool boolValue)
            return false;

        // Check if invert parameter is provided
        bool invert = parameter is string param &&
                      (param.Equals("Invert", StringComparison.OrdinalIgnoreCase) || param == "!");

        // Return the result (inverted if requested)
        return invert ? !boolValue : boolValue;
    }

    /// <summary>
    /// Converts visibility back to boolean (for two-way binding).
    /// </summary>
    /// <param name="value">The visibility value</param>
    /// <param name="targetType">The target type (not used)</param>
    /// <param name="parameter">Optional parameter: "Invert" or "!" to invert the logic</param>
    /// <param name="culture">The culture to use (not used)</param>
    /// <returns>Boolean value</returns>
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Handle null values
        if (value == null)
            return false;

        // Type checking
        if (value is not bool boolValue)
            return false;

        // Check if invert parameter is provided
        bool invert = parameter is string param &&
                      (param.Equals("Invert", StringComparison.OrdinalIgnoreCase) || param == "!");

        // Return the result (inverted if requested)
        return invert ? !boolValue : boolValue;
    }
}
