using ImageCropper.ViewModels.Windows;
using System.Windows;

namespace ImageCropper.Views.Windows;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainViewModel ViewModel { get; }

    public MainWindow()
    {
        ViewModel = new MainViewModel();
        DataContext = this;

        InitializeComponent();

        // EditableImageControlをViewModelに設定
        ViewModel.EditableImageControl = CropRangeEditableImage;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {

    }

    private void Window_Closed(object sender, EventArgs e)
    {
        // 手動設定ウィンドウが開いていれば閉じる
        CropRangeEditableImage.CloseManualSettingsWindow();
    }
}