using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageCropper.Views.UserControls;

/// <summary>
/// PrefixTextBox.xaml の相互作用ロジック
/// </summary>
public partial class PrefixTextBox : UserControl
{
    public static readonly DependencyProperty TextProperty =
        DependencyProperty.Register(
            nameof(Text),
            typeof(string),
            typeof(PrefixTextBox),
            new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextChanged));

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private bool _isUpdating = false;

    public PrefixTextBox()
    {
        InitializeComponent();
    }

    private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not PrefixTextBox ctrl) return;
        if (ctrl._isUpdating) return;

        ctrl.UpdateDocumentFromText(e.NewValue as string ?? string.Empty);
    }

    /// <summary>
    /// 文字列パターンからFlowDocument（リッチテキスト構造）を構築する
    /// </summary>
    private void UpdateDocumentFromText(string text)
    {
        _isUpdating = true;
        try
        {
            var doc = MainRichTextBox.Document;
            doc.Blocks.Clear();
            var paragraph = new Paragraph { Margin = new Thickness(0) };
            doc.Blocks.Add(paragraph);

            // プレースホルダー文字列で分割
            var regex = new Regex(@"(\{FileName\}|\{DateTime\}|\{Index\})");
            var matches = regex.Split(text);

            foreach (var part in matches)
            {
                if (string.IsNullOrEmpty(part)) continue;

                if (part == "{FileName}")
                {
                    var border = CreatePlaceholderBorder("ファイル名", "{FileName}", "#3182CE", "#EBF8FF", "#BEE3F8");
                    paragraph.Inlines.Add(new InlineUIContainer(border) { BaselineAlignment = BaselineAlignment.Center });
                }
                else if (part == "{DateTime}")
                {
                    var border = CreatePlaceholderBorder("作成日時", "{DateTime}", "#38A169", "#E6FFFA", "#B2F5EA");
                    paragraph.Inlines.Add(new InlineUIContainer(border) { BaselineAlignment = BaselineAlignment.Center });
                }
                else if (part == "{Index}")
                {
                    var border = CreatePlaceholderBorder("インデックス", "{Index}", "#DD6B20", "#FFFAF0", "#FEEBC8");
                    paragraph.Inlines.Add(new InlineUIContainer(border) { BaselineAlignment = BaselineAlignment.Center });
                }
                else
                {
                    paragraph.Inlines.Add(new Run(part));
                }
            }
        }
        finally
        {
            _isUpdating = false;
        }
    }

    /// <summary>
    /// プレースホルダーチップ用のBorder要素を作成する
    /// </summary>
    private Border CreatePlaceholderBorder(string text, string tagValue, string foregroundHex, string backgroundHex, string borderHex)
    {
        var converter = new BrushConverter();
        var foreground = (Brush)converter.ConvertFromString(foregroundHex)!;
        var background = (Brush)converter.ConvertFromString(backgroundHex)!;
        var borderBrush = (Brush)converter.ConvertFromString(borderHex)!;

        var textBlock = new TextBlock
        {
            Text = text,
            Foreground = foreground,
            FontSize = 11,
            FontWeight = FontWeights.Bold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0)
        };

        var border = new Border
        {
            CornerRadius = new CornerRadius(4),
            Background = background,
            BorderBrush = borderBrush,
            BorderThickness = new Thickness(1),
            Padding = new Thickness(6, 2, 6, 2),
            Margin = new Thickness(2, 0, 2, 0),
            Child = textBlock,
            Tag = tagValue, // パース用の識別タグ
            Cursor = Cursors.Arrow,
            Focusable = false
        };

        return border;
    }

    private void MainRichTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_isUpdating) return;

        UpdateTextFromDocument();
    }

    /// <summary>
    /// FlowDocument（リッチテキスト構造）から文字列パターンを抽出してTextプロパティを更新する
    /// </summary>
    private void UpdateTextFromDocument()
    {
        _isUpdating = true;
        try
        {
            var parts = new List<string>();

            foreach (var block in MainRichTextBox.Document.Blocks)
            {
                if (block is Paragraph paragraph)
                {
                    foreach (var inline in paragraph.Inlines)
                    {
                        if (inline is Run run)
                        {
                          parts.Add(run.Text);
                        }
                        else if (inline is InlineUIContainer container && container.Child is Border border && border.Tag is string tag)
                        {
                          parts.Add(tag);
                        }
                    }
                }
            }

            Text = string.Concat(parts);
        }
        finally
        {
            _isUpdating = false;
        }
    }

    /// <summary>
    /// キャレット位置にプレースホルダーチップを挿入する
    /// </summary>
    private void InsertPlaceholder(string text, string tagValue, string foregroundHex, string backgroundHex, string borderHex)
    {
        var border = CreatePlaceholderBorder(text, tagValue, foregroundHex, backgroundHex, borderHex);
        var caret = MainRichTextBox.CaretPosition;

        try
        {
            // キャレット位置にInlineUIContainerを挿入
            var container = new InlineUIContainer(border, caret)
            {
                BaselineAlignment = BaselineAlignment.Center
            };

            MainRichTextBox.Focus();
            // カーソル位置を挿入したチップの直後に設定する
            MainRichTextBox.CaretPosition = container.ElementEnd;
        }
        catch
        {
            // キャレット取得失敗時のフォールバック処理 (末尾に追加)
            var paragraph = MainRichTextBox.CaretPosition.Paragraph;
            if (paragraph == null)
            {
                paragraph = new Paragraph { Margin = new Thickness(0) };
                MainRichTextBox.Document.Blocks.Add(paragraph);
            }
            var container = new InlineUIContainer(border) { BaselineAlignment = BaselineAlignment.Center };
            paragraph.Inlines.Add(container);
            MainRichTextBox.Focus();
            MainRichTextBox.CaretPosition = MainRichTextBox.Document.ContentEnd;
        }

        UpdateTextFromDocument();
    }

    private void InsertFileName_Click(object sender, RoutedEventArgs e)
    {
        InsertPlaceholder("ファイル名", "{FileName}", "#3182CE", "#EBF8FF", "#BEE3F8");
    }

    private void InsertDateTime_Click(object sender, RoutedEventArgs e)
    {
        InsertPlaceholder("作成日時", "{DateTime}", "#38A169", "#E6FFFA", "#B2F5EA");
    }

    private void InsertIndex_Click(object sender, RoutedEventArgs e)
    {
        InsertPlaceholder("インデックス", "{Index}", "#DD6B20", "#FFFAF0", "#FEEBC8");
    }

    private void MainRichTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        // Enterキーを無効化して改行を防ぐ
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
        }
    }
}
