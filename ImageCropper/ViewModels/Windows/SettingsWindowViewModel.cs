using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageCropper.Helpers;
using ImageCropper.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace ImageCropper.ViewModels.Windows;

/// <summary>
/// 設定ウィンドウのViewModel。設定の変更追跡とOK/キャンセル処理を担当する。
/// </summary>
public partial class SettingsWindowViewModel : ObservableObject
{
    /// <summary>
    /// バインド用の出力拡張子リスト
    /// </summary>
    public IList<string> SupportedOutputExtensions { get; } = MainViewModel.SupportedOutputImageExtensions.ToList();

    /// <summary>
    /// 切り取り範囲表示モードの全リスト
    /// </summary>
    public static IEnumerable<RangeDisplayMode> RangeDisplayModes => Enum.GetValues(typeof(RangeDisplayMode)).Cast<RangeDisplayMode>();

    /// <summary>
    /// ウィンドウを開いた時点での設定（キャンセル時に復元するため）
    /// </summary>
    private OutputSettings OriginalOutputSettings { get; set; }

    /// <summary>
    /// ウィンドウを開いた時点でのUI設定
    /// </summary>
    private UISettings OriginalUISettings { get; set; }

    /// <summary>
    /// ダイアログ結果が確定したときに発生するイベント
    /// </summary>
    public event EventHandler<bool>? DialogResultRequested;

    #region 出力設定

    /// <summary>
    /// 編集中の出力設定
    /// </summary>
    [ObservableProperty]
    private OutputSettings editingOutputSettings = new();

    #endregion

    #region UI設定

    /// <summary>
    /// 編集中のUI設定
    /// </summary>
    [ObservableProperty]
    private UISettings editingUISettings = new();

    #endregion

    #region 変更追跡プロパティ

    /// <summary>
    /// 出力拡張子が変更されたかどうか
    /// </summary>
    public bool IsOutputExtensionChanged => EditingOutputSettings.Extension != OriginalOutputSettings.Extension;

    /// <summary>
    /// 出力フォルダパスが変更されたかどうか
    /// </summary>
    public bool IsOutputFolderPathChanged => EditingOutputSettings.FolderPath != OriginalOutputSettings.FolderPath;

    /// <summary>
    /// マルチスレッド処理の設定が変更されたかどうか
    /// </summary>
    public bool IsUseMultiThreadingChanged => EditingOutputSettings.IsUseMultiThreading != OriginalOutputSettings.IsUseMultiThreading;

    /// <summary>
    /// 「切り取り範囲が画像外でも処理するか」という設定が変更されたかどうか
    /// </summary>
    public bool IsCropOutsideImageAllowedChanged => EditingOutputSettings.IsCropOutsideImageAllowed != OriginalOutputSettings.IsCropOutsideImageAllowed;

    /// <summary>
    /// いずれかの出力設定が変更されたかどうか
    /// </summary>
    public bool IsAnyOutputSettingChanged => IsOutputExtensionChanged || IsOutputFolderPathChanged || IsUseMultiThreadingChanged || IsCropOutsideImageAllowedChanged;

    /// <summary>
    /// 表示モードの設定が変更されたかどうか
    /// </summary>
    public bool IsRangeDisplayModeChanged => EditingUISettings.RangeDisplayMode != OriginalUISettings.RangeDisplayMode;

    /// <summary>
    /// いずれかのUI設定が変更されたかどうか
    /// </summary>
    public bool IsAnyUISettingChanged => IsRangeDisplayModeChanged;

    /// <summary>
    /// いずれかの設定が変更されたかどうか
    /// </summary>
    public bool IsAnySettingChanged => IsAnyOutputSettingChanged || IsAnyUISettingChanged;

    #endregion

    /// <summary>
    /// MainViewModelへの参照
    /// </summary>
    private MainViewModel MainViewModel { get; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="mainViewModel">MainViewModelへの参照</param>
    public SettingsWindowViewModel(MainViewModel mainViewModel)
    {
        MainViewModel = mainViewModel;

        // 現在の設定を取得してバックアップ
        OriginalOutputSettings = new OutputSettings(mainViewModel.OutputSettings);
        OriginalUISettings = new UISettings(mainViewModel.UISettings);

        // 編集用の設定をコピー
        EditingOutputSettings = new OutputSettings(OriginalOutputSettings);
        EditingUISettings = new UISettings(OriginalUISettings);

        // 変更追跡のためにイベント購読
        EditingOutputSettings.PropertyChanged += OnEditingOutputSettingsPropertyChanged;
        EditingUISettings.PropertyChanged += OnEditingUISettingsPropertyChanged;
    }

    /// <summary>
    /// EditingOutputSettingsのプロパティ変更時に変更追跡プロパティを通知する
    /// </summary>
    private void OnEditingOutputSettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsOutputExtensionChanged));
        OnPropertyChanged(nameof(IsOutputFolderPathChanged));
        OnPropertyChanged(nameof(IsUseMultiThreadingChanged));
        OnPropertyChanged(nameof(IsCropOutsideImageAllowedChanged));
        OnPropertyChanged(nameof(IsAnyOutputSettingChanged));
        OnPropertyChanged(nameof(IsAnySettingChanged));
    }

    /// <summary>
    /// EditingUISettingsのプロパティ変更時に変更追跡プロパティを通知する
    /// </summary>
    private void OnEditingUISettingsPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        OnPropertyChanged(nameof(IsRangeDisplayModeChanged));
        OnPropertyChanged(nameof(IsAnyUISettingChanged));
        OnPropertyChanged(nameof(IsAnySettingChanged));
    }

    /// <summary>
    /// OutputSettingsの値をMainViewModelに適用する
    /// </summary>
    private void ApplyOutputSettingsToMainViewModel(OutputSettings settings)
    {
        MainViewModel.OutputSettings.CopyFrom(settings);
    }

    /// <summary>
    /// UISettingsの値をMainViewModelに適用する
    /// </summary>
    private void ApplyUISettingsToMainViewModel(UISettings settings)
    {
        MainViewModel.UISettings.CopyFrom(settings);
    }

    /// <summary>
    /// 出力フォルダを選択するダイアログを表示する
    /// </summary>
    [RelayCommand]
    private void SelectOutputFolder()
    {
        var folderDialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "出力フォルダを選んでください",
            Multiselect = false
        };

        if (folderDialog.ShowDialog() == true)
        {
            EditingOutputSettings.FolderPath = folderDialog.FolderName;
        }
    }

    /// <summary>
    /// 出力フォルダをエクスプローラーで開く
    /// </summary>
    [RelayCommand]
    private void OutputFolderClick()
    {
        if (string.IsNullOrWhiteSpace(EditingOutputSettings.FolderPath)) return;
        if (!Directory.Exists(EditingOutputSettings.FolderPath))
        {
            MessageBox.Show("出力フォルダが存在しません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        // エクスプローラーで出力フォルダを開く
        System.Diagnostics.Process.Start("explorer.exe", $"\"{EditingOutputSettings.FolderPath}\"");
    }

    /// <summary>
    /// 設定をエクスポートする
    /// </summary>
    [RelayCommand]
    private async Task ExportSettings()
    {
        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "設定をエクスポート",
            Filter = "JSON Files|*.json|All Files|*.*",
            DefaultExt = ".json",
            FileName = "settings.json"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                var settings = new AppSettings
                {
                    Output = new OutputSettings(EditingOutputSettings),
                    UI = new UISettings(EditingUISettings),
                    Presets = Presets.Select(p => new CropPreset(p)).ToList()
                };
                await SettingsHelper.ExportSettings(settings, saveFileDialog.FileName);
                MessageBox.Show("設定をエクスポートしました。", "エクスポート完了", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定のエクスポート中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// 設定をインポートする
    /// </summary>
    [RelayCommand]
    private async Task ImportSettings()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "設定をインポート",
            Filter = "JSON Files|*.json|All Files|*.*",
            DefaultExt = ".json"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var settings = await SettingsHelper.ImportSettings(openFileDialog.FileName);
                if (settings != null)
                {
                    EditingOutputSettings.CopyFrom(settings.Output);
                    if (settings.UI != null)
                    {
                        EditingUISettings.CopyFrom(settings.UI);
                    }
                    if (settings.Presets != null)
                    {
                        Presets.Clear();
                        foreach (var preset in settings.Presets)
                        {
                            Presets.Add(preset);
                        }
                    }
                    MessageBox.Show("設定をインポートしました。", "インポート完了", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"設定のインポート中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// OKボタンが押されたときの処理。設定を保存してダイアログを閉じる。
    /// </summary>
    [RelayCommand]
    private async Task OnOk()
    {
        // MainViewModelに設定を反映
        ApplyOutputSettingsToMainViewModel(EditingOutputSettings);
        ApplyUISettingsToMainViewModel(EditingUISettings);

        // 設定をファイルに保存
        try
        {
            await SettingsHelper.SaveSettings(MainViewModel);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定の保存中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        DialogResultRequested?.Invoke(this, true);
    }

    #region テンプレート管理

    /// <summary>
    /// テンプレートのコレクション（MainViewModelのコレクションへの参照）
    /// </summary>
    public ObservableCollection<CropPreset> Presets => MainViewModel.Presets;

    /// <summary>
    /// 設定ウィンドウで選択中のテンプレート
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeletePresetCommand))]
    private CropPreset? selectedPreset;

    /// <summary>
    /// テンプレートを削除できるかどうか
    /// </summary>
    private bool CanDeletePreset() => SelectedPreset != null;

    /// <summary>
    /// 現在の切り取り範囲・設定をテンプレートとして保存する
    /// </summary>
    [RelayCommand]
    private async Task SaveCurrentAsPreset()
    {
        if (MainViewModel.CropRangePixelCoordinates is null)
        {
            MessageBox.Show("切り取り範囲が設定されていません。", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var preset = await ImageCropper.Views.Windows.PresetSaveDialog.ShowDialogAsync(
            MainViewModel.CropRangePixelCoordinates,
            MainViewModel.OutputSettings.Extension,
            MainViewModel.OutputSettings.FolderPath,
            Application.Current.MainWindow);

        if (preset is null) return;

        MainViewModel.Presets.Add(preset);

        try
        {
            await SettingsHelper.SaveSettings(MainViewModel);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定の保存中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 選択中のテンプレートを削除する
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeletePreset))]
    private async Task DeletePreset()
    {
        if (SelectedPreset is null) return;

        var result = MessageBox.Show(
            $"「{SelectedPreset.Name}」を削除しますか？",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        MainViewModel.Presets.Remove(SelectedPreset);
        SelectedPreset = null;

        try
        {
            await SettingsHelper.SaveSettings(MainViewModel);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定の保存中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    #endregion

    /// <summary>
    /// キャンセルボタンが押されたときの処理。元の設定に戻してダイアログを閉じる。
    /// </summary>
    [RelayCommand]
    private void OnCancel()
    {
        // 設定が変更されていた場合、確認メッセージを表示
        if (IsAnySettingChanged)
        {
            var result = MessageBox.Show("設定が変更されています。変更を破棄してもよろしいですか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Cancel)
            {
                return; // ユーザーがキャンセルした場合、処理を中断
            }
        }

        // 元の設定をMainViewModelに復元（ウィンドウを開いた時点の設定に戻す）
        ApplyOutputSettingsToMainViewModel(OriginalOutputSettings);
        ApplyUISettingsToMainViewModel(OriginalUISettings);

        DialogResultRequested?.Invoke(this, false);
    }
}
