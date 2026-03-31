using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ImageCropper.Converters;

/// <summary>
/// bool値をVisibilityに変換するコンバーター
/// </summary>
public class BooleanToVisibilityConverter : IValueConverter
{
    /// <summary>
    /// bool -> Visibility (trueならVisible、falseならCollapsed)
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    /// <summary>
    /// Visibility -> bool (VisibleならTrue、それ以外はFalse)
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}
