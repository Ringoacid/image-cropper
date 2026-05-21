using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageCropper.Views.UserControls;

/// <summary>
/// NumberBox.xaml の相互作用ロジック
/// </summary>
public partial class NumberBox : UserControl
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public static readonly DependencyProperty MinProperty =
        DependencyProperty.Register(
            nameof(Min),
            typeof(double),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(double.MinValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRangeChanged));

    public double Min
    {
        get => (double)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public static readonly DependencyProperty MaxProperty =
        DependencyProperty.Register(
            nameof(Max),
            typeof(double),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(double.MaxValue, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRangeChanged));

    public double Max
    {
        get => (double)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public static readonly DependencyProperty IsIntegerOnlyProperty =
        DependencyProperty.Register(
            nameof(IsIntegerOnly),
            typeof(bool),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsIntegerOnlyChanged));

    public bool IsIntegerOnly
    {
        get => (bool)GetValue(IsIntegerOnlyProperty);
        set => SetValue(IsIntegerOnlyProperty, value);
    }

    public static readonly DependencyProperty IsWarningProperty =
        DependencyProperty.Register(
            nameof(IsWarning),
            typeof(bool),
            typeof(NumberBox),
            new FrameworkPropertyMetadata(false));

    public bool IsWarning
    {
        get => (bool)GetValue(IsWarningProperty);
        set => SetValue(IsWarningProperty, value);
    }

    private static readonly HashSet<char> _allowedChars = ['0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '.', '-'];

    public NumberBox()
    {
        InitializeComponent();
        DataObject.AddPastingHandler(MainTextBox, OnPaste);
    }

    /// <summary>
    /// 数値が変更されたときの処理。数値の範囲チェックとテキストボックスへの反映を行う。
    /// </summary>
    /// <param name="d">数値が変更されたNumberBox</param>
    /// <param name="e">イベント引数</param>
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox nb) return;
        if (e.OldValue == e.NewValue) return;

        double.TryParse(nb.MainTextBox.Text, out double textboxValue);
        if (e.NewValue is not double newValue) return;

        if (textboxValue != newValue)
            nb.MainTextBox.Text = e.NewValue.ToString();

        nb.SetWarningState(nb.MainTextBox.Text);
    }

    /// <summary>
    /// 入力の範囲が変更されたときの処理。
    /// </summary>
    /// <param name="d">範囲が変更されたNumberBox</param>
    /// <param name="e">イベント引数</param>
    private static void OnRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox nb) return;

        nb.SetWarningState(nb.MainTextBox.Text, false);
    }

    private static void OnIsIntegerOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NumberBox nb)
            return;

        nb.SetWarningState(nb.MainTextBox.Text, false);
    }

    private void MainTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
    {
        var input = e.Text;

        foreach (var ch in input)
        {
            if (!_allowedChars.Contains(ch))
            {
                e.Handled = true;
                return;
            }
        }
    }

    private void MainTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // テキストが変更されたときにValueプロパティを更新
        SetWarningState(MainTextBox.Text);
    }

    private void SetWarningState(string? text, bool isChangeValue = true)
    {
        bool isNormal = true;

        if (Min > Max)
        {
            isNormal = false;
        }

        double value = 0.0;
        bool parseSuccess = double.TryParse(text, out value);

        if (parseSuccess)
        {
            if (value > Max)
            {
                // 最大値を超える
                isNormal = false;
            }

            if (value < Min)
            {
                // 最小値を下回る
                isNormal = false;
            }

            if (IsIntegerOnly && !IsValueInteger(value))
            {
                // 整数のみ許可の場合で、小数が入力された
                isNormal = false;
            }
        }
        else
        {
            isNormal = false;
        }

        if (OverlayBorder is null) return;
        if (!isNormal)
        {
            OverlayBorder.BorderBrush = Brushes.Red;
            IsWarning = true;
        }
        else
        {
            OverlayBorder.BorderBrush = Brushes.Transparent;
            IsWarning = false;
        }

        if (isChangeValue && parseSuccess)
            Value = value;

        static bool IsValueInteger(double val)
        {
            return val % 1 == 0;
        }
    }

    private void OnPaste(object sender, DataObjectPastingEventArgs e)
    {
        if (e.DataObject.GetDataPresent(DataFormats.UnicodeText))
        {
            var text = e.DataObject.GetData(DataFormats.UnicodeText) as string;
            if (!string.IsNullOrEmpty(text))
            {
                string currentText = MainTextBox.Text;
                int selectionStart = MainTextBox.SelectionStart;
                int selectionLength = MainTextBox.SelectionLength;
                string newText = currentText.Remove(selectionStart, selectionLength).Insert(selectionStart, text);

                bool isValid = true;
                foreach (char c in text)
                {
                    if (!_allowedChars.Contains(c))
                    {
                        isValid = false;
                        break;
                    }
                }

                if (!isValid)
                {
                    e.CancelCommand();
                    return;
                }

                bool canParse = false;
                if (IsIntegerOnly)
                {
                    canParse = int.TryParse(newText, out _) || newText == "-" || newText == "";
                }
                else
                {
                    canParse = double.TryParse(newText, out _) || newText == "-" || newText == "." || newText == "-." || newText == "";
                }

                if (!canParse)
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
        else
        {
            e.CancelCommand();
        }
    }
}