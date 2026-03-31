using ImageCropper.ViewModels.Windows;
using System.Windows;

namespace ImageCropper.Views.Windows;

/// <summary>
/// SettingsWindow.xaml の相互作用ロジック
/// </summary>
public partial class SettingsWindow : Window
{
    public SettingsWindowViewModel ViewModel { get; }

    public SettingsWindow(MainViewModel mainViewModel)
    {
        ViewModel = new SettingsWindowViewModel(mainViewModel);
        DataContext = this;

        InitializeComponent();

        // ダイアログ結果が確定したときにウィンドウを閉じる
        ViewModel.DialogResultRequested += (sender, result) =>
        {
            Close();
        };
    }

    /// <summary>
    /// 設定ウィンドウを表示する静的メソッド
    /// </summary>
    /// <param name="mainViewModel">MainViewModelへの参照</param>
    /// <param name="owner">親ウィンドウ</param>
    public static void ShowWindow(MainViewModel mainViewModel, Window? owner = null)
    {
        var window = new SettingsWindow(mainViewModel);

        if (owner != null)
        {
            window.Owner = owner;
        }

        window.ShowDialog();
    }
}
