using System.Threading;
using System.Windows;
using System.Windows.Input;

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

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.Escape)
        {
            CancelAndClose();
            e.Handled = true;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        TriggerCancellation();
    }

    private void TriggerCancellation()
    {
        if (CancellationTokenSource is not null && !CancellationTokenSource.IsCancellationRequested)
        {
            try
            {
                CancellationTokenSource.Cancel();
            }
            catch (System.ObjectDisposedException)
            {
            }
        }
    }

    private void CancelAndClose()
    {
        TriggerCancellation();
        this.Close();
    }

    private void Cancel_Button_Click(object sender, RoutedEventArgs e)
    {
        CancelAndClose();
    }
}
