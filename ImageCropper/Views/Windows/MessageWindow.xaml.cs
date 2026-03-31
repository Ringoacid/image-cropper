using System.Windows;

namespace ImageCropper.Views.Windows;

/// <summary>
/// MessageWindow.xaml の相互作用ロジック
/// </summary>
public partial class MessageWindow : Window
{
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    public MessageWindow(string title = "ファイルを再帰的に検索しています",
                         string message = "この処理には長い時間がかかる場合があります。",
                         CancellationTokenSource? cancellationTokenSource = null)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
        CancellationTokenSource = cancellationTokenSource;
    }

    private void Cancel_Button_Click(object sender, RoutedEventArgs e)
    {
        if (CancellationTokenSource is not null)
            CancellationTokenSource.Cancel();

        this.Close();
    }
}
