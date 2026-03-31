using System.Globalization;
using System.Windows.Data;

namespace ImageCropper.Converters;

/// <summary>
/// bool値を反転するコンバーター
/// </summary>
public class InverseBooleanConverter : IValueConverter
{
    /// <summary>
    /// bool -> 反転したbool
    /// </summary>
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return true;
    }

    /// <summary>
    /// 反転したbool -> 元のbool
    /// </summary>
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return !boolValue;
        }
        return false;
    }
}
