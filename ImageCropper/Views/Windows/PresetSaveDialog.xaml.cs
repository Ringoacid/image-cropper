using ImageCropper.Models;
using ImageCropper.ViewModels.Windows;
using System.Windows;

namespace ImageCropper.Views.Windows;

/// <summary>
/// PresetSaveDialog.xaml の相互作用ロジック
/// </summary>
public partial class PresetSaveDialog : Window
{
    public PresetSaveDialogViewModel ViewModel { get; }

    public PresetSaveDialog(string currentOutputExtension, string currentOutputFolderPath)
    {
        ViewModel = new PresetSaveDialogViewModel(currentOutputExtension, currentOutputFolderPath);
        DataContext = this;

        InitializeComponent();
    }

    /// <summary>
    /// テンプレート保存ダイアログを表示する静的メソッド。
    /// キャンセルまたは×ボタンで閉じられた場合はnullを返す。
    /// </summary>
    /// <param name="cropRange">保存する切り取り範囲</param>
    /// <param name="currentOutputExtension">現在の出力形式</param>
    /// <param name="currentOutputFolderPath">現在の出力先フォルダパス</param>
    /// <param name="owner">親ウィンドウ</param>
    public static async Task<CropPreset?> ShowDialogAsync(
        RectRange cropRange,
        string currentOutputExtension,
        string currentOutputFolderPath,
        Window? owner = null)
    {
        var window = new PresetSaveDialog(currentOutputExtension, currentOutputFolderPath);
        var tcs = new TaskCompletionSource<CropPreset?>();

        window.ViewModel.DialogResultRequested += (sender, result) =>
        {
            if (result)
            {
                var vm = window.ViewModel;
                tcs.TrySetResult(new CropPreset
                {
                    Name = vm.PresetName.Trim(),
                    CropRange = cropRange,
                    OutputExtension = vm.IncludeOutputExtension ? vm.CurrentOutputExtension : null,
                    OutputFolderPath = vm.IncludeOutputFolderPath ? vm.CurrentOutputFolderPath : null,
                });
            }
            else
            {
                tcs.TrySetResult(null);
            }
            window.Close();
        };

        // ×ボタンで閉じた場合はnullを返す
        window.Closed += (sender, e) => tcs.TrySetResult(null);

        if (owner != null)
            window.Owner = owner;

        window.ShowDialog();
        return await tcs.Task;
    }
}
