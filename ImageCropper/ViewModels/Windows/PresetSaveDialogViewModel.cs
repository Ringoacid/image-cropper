using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageCropper.ViewModels.Windows;

/// <summary>
/// テンプレート保存ダイアログのViewModel
/// </summary>
public partial class PresetSaveDialogViewModel : ObservableObject
{
    /// <summary>
    /// ダイアログ結果が確定したときに発生するイベント
    /// </summary>
    public event EventHandler<bool>? DialogResultRequested;

    /// <summary>
    /// テンプレート名
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(OkCommand))]
    private string presetName = string.Empty;

    /// <summary>
    /// 出力形式を含めるかどうか
    /// </summary>
    [ObservableProperty]
    private bool includeOutputExtension;

    /// <summary>
    /// 現在の出力形式（参照表示用）
    /// </summary>
    [ObservableProperty]
    private string currentOutputExtension = string.Empty;

    /// <summary>
    /// 出力先フォルダを含めるかどうか
    /// </summary>
    [ObservableProperty]
    private bool includeOutputFolderPath;

    /// <summary>
    /// 現在の出力先フォルダパス（参照表示用）
    /// </summary>
    [ObservableProperty]
    private string currentOutputFolderPath = string.Empty;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="currentOutputExtension">現在の出力形式</param>
    /// <param name="currentOutputFolderPath">現在の出力先フォルダパス</param>
    public PresetSaveDialogViewModel(string currentOutputExtension, string currentOutputFolderPath)
    {
        CurrentOutputExtension = currentOutputExtension;
        CurrentOutputFolderPath = currentOutputFolderPath;
    }

    /// <summary>
    /// OKボタンを押せるかどうか（テンプレート名が空でない場合のみ）
    /// </summary>
    private bool CanOk() => !string.IsNullOrWhiteSpace(PresetName);

    /// <summary>
    /// OKボタンが押されたときの処理
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOk))]
    private void Ok() => DialogResultRequested?.Invoke(this, true);

    /// <summary>
    /// キャンセルボタンが押されたときの処理
    /// </summary>
    [RelayCommand]
    private void Cancel() => DialogResultRequested?.Invoke(this, false);
}
