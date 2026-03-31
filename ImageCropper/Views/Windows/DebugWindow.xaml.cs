using ImageCropper.ViewModels.Windows;
using System.Windows;

namespace ImageCropper.Views.Windows;

/// <summary>
/// DebugWindow.xaml の相互作用ロジック
/// </summary>
public partial class DebugWindow : Window
{
    public DebugViewModel ViewModel { get; }

    public DebugWindow()
    {
        ViewModel = new DebugViewModel();
        DataContext = this;

        InitializeComponent();
    }
}
