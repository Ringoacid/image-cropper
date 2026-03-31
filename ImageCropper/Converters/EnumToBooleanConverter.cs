using System.Globalization;
using System.Windows.Data;

namespace ImageCropper.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    // ViewModel -> View (プロパティの値がラジオボタンと一致するか判定)
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || parameter == null)
            return false;

        // 現在のViewModelの値(value)と、パラメーターで渡された値が一致したらチェック状態(true)にする
        return value.Equals(parameter);
    }

    // View -> ViewModel (チェックされたラジオボタンのパラメーター値をプロパティに設定)
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // チェックが入った(trueになった)場合のみ値を更新する
        if (value is bool isChecked && isChecked)
        {
            return parameter;
        }
        // チェックが外れた場合は何もしない (Binding.DoNothing)
        return Binding.DoNothing;
    }
}