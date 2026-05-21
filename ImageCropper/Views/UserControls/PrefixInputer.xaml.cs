using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageCropper.Views.UserControls;

public readonly struct Templete(string displayName, SolidColorBrush colorBrush)
{
    public string DisplayName { get; init; } = displayName;

    public SolidColorBrush ColorBrush { get; init; } = colorBrush;
}

public struct StringOrTemplete()
{
    public string? String { get; set; }

    public Templete? Templete { get; set; }

    public StringOrTemplete(string? @string, Templete? templete) : this()
    {
        if (@string is null && templete is null)
            throw new ArgumentNullException("@stringもしくはtemplete");

        this.String = @string;
        this.Templete = templete;
    }
}


/// <summary>
/// PrefixInputer.xaml の相互作用ロジック
/// </summary>
public partial class PrefixInputer : UserControl
{
    public static Templete[] SampleTemplete { get; } =
        [
            new ("元のファイル名", new SolidColorBrush(Colors.Red)),
            new ("日付（yyyy/mm/dd）", new SolidColorBrush(Colors.Blue)),
        ];

    public Templete[] Templetes { get; } =
        [
            new ("元のファイル名", new SolidColorBrush(Colors.Red)),
            new ("日付（yyyy/mm/dd）", new SolidColorBrush(Colors.Blue)),
        ];

    public List<StringOrTemplete> Prefixes { get; set; } = [
        new StringOrTemplete("テスト", null),
        new StringOrTemplete(null, SampleTemplete[0]),
        new StringOrTemplete("おわり", null),
        ];

    public PrefixInputer()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        foreach(var prefix in Prefixes)
        {
            if (prefix.String is not null)
            {
                var textBlock = new TextBlock()
                {
                    Text = prefix.String,
                    Margin = new Thickness(5),
                };
                MainStackPanel.Children.Add(textBlock);
            }
            else if (prefix.Templete is not null)
            {
                var border = new Border()
                {
                    Background = prefix.Templete.Value.ColorBrush,
                };

                var textBlock = new TextBlock()
                {
                    Text = prefix.Templete.Value.DisplayName,
                };
                border.Child = textBlock;
                MainStackPanel.Children.Add(border);
            }
        }
    }
}
