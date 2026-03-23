using System.Globalization;

namespace DulceRecetario.Helpers;

/// <summary>
/// Convierte bool (IsFavorite) a un glifo de corazón para mostrar en la UI.
/// </summary>
public class BoolToHeartConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? "♥" : "♡";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
