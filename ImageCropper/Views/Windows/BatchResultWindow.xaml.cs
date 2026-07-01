using ImageCropper.ViewModels.Windows;
using System.Windows;
using System.Windows.Input;

namespace ImageCropper.Views.Windows;

/// <summary>
/// BatchResultWindow.xaml の相互作用ロジック
/// </summary>
public partial class BatchResultWindow : Window
{
    public BatchResultWindowViewModel ViewModel { get; }

    public BatchResultWindow(BatchResultWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.Escape)
        {
            Close();
            e.Handled = true;
        }
    }

    private void Close_Button_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
