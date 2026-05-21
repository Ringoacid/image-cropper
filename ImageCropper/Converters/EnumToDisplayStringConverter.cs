using ImageCropper.Models;
using System;
using System.Globalization;
using System.Windows.Data;

namespace ImageCropper.Converters;

public class EnumToDisplayStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is RangeDisplayMode mode)
        {
            return mode switch
            {
                RangeDisplayMode.XYWH_Pixel => "(X, Y, W, H) - ピクセル",
                RangeDisplayMode.XYWH_Percent => "(X, Y, W, H) - パーセント",
                RangeDisplayMode.X1Y1X2Y2_Pixel => "(X1, Y1, X2, Y2) - ピクセル",
                RangeDisplayMode.X1Y1X2Y2_Percent => "(X1, Y1, X2, Y2) - パーセント",
                _ => value.ToString() ?? ""
            };
        }
        return value?.ToString() ?? "";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string str)
        {
            return str switch
            {
                "(X, Y, W, H) - ピクセル" => RangeDisplayMode.XYWH_Pixel,
                "(X, Y, W, H) - パーセント" => RangeDisplayMode.XYWH_Percent,
                "(X1, Y1, X2, Y2) - ピクセル" => RangeDisplayMode.X1Y1X2Y2_Pixel,
                "(X1, Y1, X2, Y2) - パーセント" => RangeDisplayMode.X1Y1X2Y2_Percent,
                _ => Enum.TryParse<RangeDisplayMode>(str, out var result) ? result : Binding.DoNothing
            };
        }
        if (value is RangeDisplayMode mode)
        {
            return mode;
        }
        return Binding.DoNothing;
    }
}
