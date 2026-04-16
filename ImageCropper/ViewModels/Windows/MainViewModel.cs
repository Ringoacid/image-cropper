using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageCropper.Helpers;
using ImageCropper.Models;
using ImageCropper.Views.UserControls;
using ImageCropper.Views.Windows;
using OpenCvSharp;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Windows.Shapes;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using Window = System.Windows.Window;

namespace ImageCropper.ViewModels.Windows;

/// <summary>
/// メイン画面のViewModel。画像の読み込み、設定の保存・読み込み、切り抜き処理などを担当する。
/// </summary>
public partial class MainViewModel : ObservableObject
{
    #region ファイル・設定の保存・読み込み

    /// <summary>
    /// 切り取り範囲のピクセル座標（元画像基準）
    /// 異なる解像度の画像間でも一貫した座標を保持
    /// </summary>
    [ObservableProperty]
    private RectRange? cropRangePixelCoordinates;
    /// <summary>
    /// サポートされている出力画像拡張子一覧
    /// </summary>
    public static readonly HashSet<string> SupportedOutputImageExtensions = [".bmp", ".dib", ".jpeg", ".jpg", ".jpe", ".jp2", ".png", ".pbm", ".pgm", ".ppm", ".sr", ".ras", ".tiff", ".tif"];

    /// <summary>
    /// サポートされている入力画像拡張子一覧
    /// </summary>
    private static readonly HashSet<string> SupportedInputImageExtensions = [".jpg", ".jpeg", ".jpe", ".png", ".bmp", ".dib", ".gif", ".tiff", ".tif", ".webp", ".jp2", ".pbm", ".pgm", ".ppm", ".sr", ".ras", ".exr", ".hdr"];

    /// <summary>
    /// バインド用の出力拡張子リスト
    /// </summary>
    [ObservableProperty]
    private IList<string> supportedOutputExtensions = SupportedOutputImageExtensions.ToList();

    /// <summary>
    /// 出力設定
    /// </summary>
    [ObservableProperty]
    private OutputSettings outputSettings = new();

    /// <summary>
    /// UI設定
    /// </summary>
    [ObservableProperty]
    private UISettings uISettings = new();

    /// <summary>
    /// 画像ファイルパスのコレクション
    /// </summary>
    [ObservableProperty]
    private ObservableHashSet<SelectionItem<string>> imagePaths = new();

    /// <summary>
    /// 画像が選択されていない場合に表示するデフォルトのプレビュー画像パス
    /// </summary>
    private static string DefaultPreviewImagePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "DefaultPreviewImage.png");

    /// <summary>
    /// 選択中でプレビューを表示している画像
    /// </summary>
    private SelectionItem<string>? selectedImage = new(DefaultPreviewImagePath, Path.GetFileName(DefaultPreviewImagePath));

    /// <summary>
    /// SelectedImageの変更を許可するかどうか
    /// </summary>
    private bool IsSelectedImageChangeable { get; set; } = true;

    public SelectionItem<string>? SelectedImage
    {
        get => selectedImage;
        set
        {
            if (!IsSelectedImageChangeable) return;

            if (!EqualityComparer<SelectionItem<string>?>.Default.Equals(selectedImage, value))
            {
                // 変更があった
                OnSelectedImageChanging(value);
                OnSelectedImageChanging(default, value);
                OnPropertyChanged(nameof(SelectedImage));
                selectedImage = value;
                OnSelectedImageChanged(value);
                OnSelectedImageChanged(default, value);
                OnPropertyChanged(nameof(SelectedImage));
            }
        }
    }

    partial void OnSelectedImageChanging(SelectionItem<string>? value);
    partial void OnSelectedImageChanging(SelectionItem<string>? oldValue, SelectionItem<string>? newValue);
    partial void OnSelectedImageChanged(SelectionItem<string>? value);
    partial void OnSelectedImageChanged(SelectionItem<string>? oldValue, SelectionItem<string>? newValue);



    /// <summary>
    /// 選択中の画像の情報
    /// </summary>
    [ObservableProperty]
    private ImageInformation selectedImageInformation = new(DefaultPreviewImagePath);


    /// <summary>
    /// 選択中の画像が変更されたときの処理。ファイルが存在しない場合はリストから削除する。
    /// </summary>
    /// <param name="value">新しく選択された画像</param>
    partial void OnSelectedImageChanged(SelectionItem<string>? value)
    {
        if (value is null)
        {
            var filename = Path.GetFileName(DefaultPreviewImagePath);
            SelectedImage = new(DefaultPreviewImagePath, filename);
            return;
        }

        // 選択された画像が削除されていないか確認
        string path = value.Item;

        if (!File.Exists(path))
        {
            var result = ShowWarningMessageBox("ファイルが見つかりません", $"選択された画像ファイル({path})が存在しません。\nリストから削除しますか？", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                ImagePaths.Remove(value);
            }
        }
        else
        {
            _ = LoadSelectedImageInformationAsync(path);
        }
    }

    private async Task LoadSelectedImageInformationAsync(string path)
    {
        try
        {
            var info = await Task.Run(() => new ImageInformation(path));
            SelectedImageInformation = info;
        }
        catch (Exception ex)
        {
            ShowErrorMessageBox("エラー", $"画像情報の取得中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public MainViewModel()
    {
    }

    /// <summary>
    /// EditableImageの参照（座標変換に使用）
    /// </summary>
    public EditableImage? EditableImageControl { get; set; }

    /// <summary>
    /// ウィンドウ読み込み時の処理。設定を読み込む。
    /// </summary>
    [RelayCommand]
    private void OnLoaded()
    {
        try
        {
            SettingsHelper.LoadSettings(this, false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShowErrorMessageBox("エラー", $"設定の読み込み中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    /// ウィンドウ終了時の処理。設定を保存する。
    /// </summary>
    [RelayCommand]
    private void OnClosed()
    {
        try
        {
            SettingsHelper.SaveSettings(this).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            ShowErrorMessageBox("エラー", $"設定の保存中にエラーが発生しました: {ex.Message}");
        }
    }

    /// <summary>
    ///画像ファイルを開くダイアログを表示し、選択された画像を追加する。
    /// </summary>
    [RelayCommand]
    private async Task OnOpenImageClick()
    {
        // (複数の)画像を開く処理
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "画像を選んでください（複数選択可）",
            Filter = "Image Files|*.jpg;*.jpeg;*.jpe;*.png;*.bmp;*.dib;*.gif;*.tiff;*.tif;*.webp;*.jp2;*.pbm;*.pgm;*.ppm;*.sr;*.ras;*.exr;*.hdr|All Files|*.*",
            Multiselect = true
        };

        if (openFileDialog.ShowDialog() == true)
        {
            await AddImageFilesAsync(openFileDialog.FileNames, "画像追加中");
        }
    }

    /// <summary>
    /// フォルダを選択し、その中の画像ファイルを追加する。
    /// </summary>
    [RelayCommand]
    private async Task OnOpenFolderClick()
    {
        // フォルダ内の画像を開く処理
        var folderDialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "フォルダを選んでください（複数選択可）",
            Multiselect = true
        };

        if (folderDialog.ShowDialog() == true)
        {
            var files = folderDialog.FolderNames
                .SelectMany(folder => Directory.GetFiles(folder, "*.*"))
                .Where(file => SupportedInputImageExtensions.Contains(Path.GetExtension(file).ToLower()));

            await AddImageFilesAsync(files, "画像追加中");
        }
    }

    /// <summary>
    /// フォルダを選択し、その中を再帰的に検索し、画像ファイルを追加する。
    /// </summary>
    [RelayCommand]
    private async Task OnOpenFolderRecursivelyClick()
    {
        // フォルダ内の画像を再帰的に開く処理
        var folderDialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "フォルダを選んでください（複数選択可）",
            Multiselect = true
        };
        if (folderDialog.ShowDialog() == true)
        {
            string rootPath = folderDialog.FolderName;

            try
            {
                using var cts = new CancellationTokenSource();
                var msgWindow = new MessageWindow("ファイルを再帰的に検索しています", "この処理には長い時間がかかる場合があります。", cts);

                var skippedFolders = new List<string>();

                // アクセス権エラーをスキップしながら再帰的にファイルを検索する
                var searchTask = Task.Run(() => EnumerateFilesSafe(rootPath, skippedFolders, cts.Token), cts.Token);

                // 非モーダルで表示
                msgWindow.Show();

                IEnumerable<string>? allFiles;
                try
                {
                    allFiles = await searchTask;
                }
                catch (OperationCanceledException)
                {
                    msgWindow.Close();
                    return; // キャンセルされた場合
                }

                if (cts.Token.IsCancellationRequested)
                {
                    msgWindow.Close();
                    return;
                }

                msgWindow.TitleText.Text = "画像ファイルをフィルタリングしています";
                msgWindow.Show();

                // 拡張子でフィルタリング
                var filterTask = Task.Run(() => allFiles.Where(path =>
                    {
                        string extension = Path.GetExtension(path).ToLower();
                        return SupportedInputImageExtensions.Contains(extension);
                    }).ToList(), cts.Token
                );

                List<string>? supportedFiles;
                try
                {
                    supportedFiles = await filterTask;
                }
                catch (OperationCanceledException)
                {
                    msgWindow.Close();
                    return; // キャンセルされた場合
                }

                msgWindow.Close();

                if (cts.Token.IsCancellationRequested)
                {
                    return;
                }

                // スキップされたフォルダがあれば警告を表示
                if (skippedFolders.Count > 0)
                {
                    var skippedMessage = $"以下の {skippedFolders.Count} 件のフォルダはアクセス権がないためスキップされました:\n" +
                                         string.Join("\n", skippedFolders.Take(5));
                    if (skippedFolders.Count > 5)
                    {
                        skippedMessage += $"\n...他 {skippedFolders.Count - 5} 件";
                    }
                    ShowWarningMessageBox("アクセス拒否", skippedMessage, MessageBoxButton.OK);
                }

                await AddImageFilesAsync(supportedFiles, "画像追加中");
            }
            catch (UnauthorizedAccessException ex)
            {
                // アクセス権がないフォルダがあった場合の処理
                ShowErrorMessageBox("エラー", $"アクセス権がないフォルダがあります: {ex.Message}");
            }
            catch (DirectoryNotFoundException ex)
            {
                // ルートフォルダが見つからなかった場合の処理
                ShowErrorMessageBox("エラー", $"指定されたフォルダが見つかりません: {ex.Message}");
            }
            catch (Exception ex)
            {
                // その他の例外処理
                ShowErrorMessageBox("エラー", $"フォルダ内の画像の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }
    }


    /// <summary>
    /// 指定されたファイルパスの画像をImagePathsコレクションに追加します。
    /// </summary>
    /// <param name="filePaths">追加するファイルパスの列挙</param>
    private async Task AddImageFilesAsync(IEnumerable<string> filePaths, string progressTitle)
    {
        var items = await Task.Run(() => filePaths
            .Where(File.Exists)
            .Where(file => SupportedInputImageExtensions.Contains(Path.GetExtension(file).ToLower()))
            .Select(filepath => new SelectionItem<string>(filepath, Path.GetFileName(filepath)))
            .ToList());

        if (items.Count == 0)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var progressVm = CreateProgressViewModel(items.Count, "追加を開始しています...");
        progressVm.IsCancelButtonVisible = true;
        progressVm.CancellationTokenSource = cts;
        var progressWindow = CreateProgressWindow(progressVm, progressTitle);

        DisableMainWindow();
        progressWindow.Show();

        try
        {
            await Task.Run(() =>
            {
                for (int i = 0; i < items.Count; i++)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    var index = i;
                    var item = items[index];

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ImagePaths.Add(item);

                        progressVm.Current = index + 1;
                        progressVm.FileName = item.DisplayName;
                        progressVm.Progress = (int)((index + 1) * 100.0 / items.Count);
                        progressVm.Message = $"{progressVm.Current}/{items.Count} 追加中...";
                    });
                }
            }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            progressVm.Message = "キャンセルされました";
        }
        finally
        {
            await FinalizeProcessing(progressWindow);
        }
    }

    /// <summary>
    /// ファイルやフォルダがドロップされたときの処理。
    /// </summary>
    [RelayCommand]
    private async Task OnDrop(DragEventArgs e)
    {
        // 単体または複数のファイル/フォルダがドロップされたときの処理
        try
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            var droppedItems = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (droppedItems == null || droppedItems.Length == 0)
            {
                return;
            }

            var filesToAdd = new List<string>();

            foreach (var item in droppedItems)
            {
                // ファイルかフォルダかを判定
                if (File.Exists(item))
                {
                    // ファイルの場合、拡張子がサポートされている場合のみ追加
                    if (SupportedInputImageExtensions.Contains(Path.GetExtension(item).ToLower()))
                        filesToAdd.Add(item);
                }
                else if (Directory.Exists(item))
                {
                    // フォルダの場合、フォルダ内の画像ファイルを取得
                    var imageFiles = Directory.GetFiles(item, "*.*")
                    .Where(file => SupportedInputImageExtensions.Contains(Path.GetExtension(file).ToLower()));

                    filesToAdd.AddRange(imageFiles);
                }
            }

            if (filesToAdd.Count > 0)
            {
                e.Handled = true;
                await AddImageFilesAsync(filesToAdd, "画像追加中");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"ドロップ中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }
    }

    /// <summary>
    /// 出力フォルダを選択するダイアログを表示し、パスを設定する。
    /// </summary>
    [RelayCommand]
    private void OnSetOutputFolderClick()
    {
        var folderDialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "出力フォルダを選んでください",
            Multiselect = false
        };
        if (folderDialog.ShowDialog() == true)
        {
            OutputSettings.FolderPath = folderDialog.FolderName;
        }
    }

    [RelayCommand]
    private void OnOutputFolderClick()
    {
        if (string.IsNullOrWhiteSpace(OutputSettings.FolderPath)) return;
        if (!Directory.Exists(OutputSettings.FolderPath))
        {
            ShowErrorMessageBox("エラー", "出力フォルダが存在しません。");
            return;
        }
        // エクスプローラーで出力フォルダを開く
        System.Diagnostics.Process.Start("explorer.exe", $"\"{OutputSettings.FolderPath}\"");
    }

    /// <summary>
    /// 設定を保存する（手動保存）。
    /// </summary>
    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            await SettingsHelper.SaveSettings(this);
            MessageBox.Show("設定を保存しました。", "保存完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定の保存中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 設定を読み込む（手動読み込み）。
    /// </summary>
    [RelayCommand]
    private async Task LoadSettings()
    {
        try
        {
            await SettingsHelper.LoadSettings(this, true);
            MessageBox.Show("設定を読み込みました。", "読み込み完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"設定の読み込み中にエラーが発生しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 設定をデフォルト値にリセットする。
    /// </summary>
    [RelayCommand]
    private void ResetSettings()
    {
        var result = MessageBox.Show("設定をデフォルト値にリセットしますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            OutputSettings = new OutputSettings();

            MessageBox.Show("設定をリセットしました。", "リセット完了", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
    #endregion

    #region リストの操作
    /// <summary>
    /// 選択された複数の画像ファイルパスのコレクション
    /// SelectedImageはクリックで選択された単体の画像を表すが、SelectedImagesはShiftやCtrlキーで複数選択された画像を扱う。
    /// ShiftやCtrlキーで画像を複数選択しても、SelectedImageは変化しない。
    /// </summary>
    [ObservableProperty]
    private ObservableCollection<SelectionItem<string>> selectedImages = [];

    /// <summary>
    /// リストでキーが押下されたときの処理。
    /// </summary>
    /// <param name="e">イベント引数</param>
    [RelayCommand]
    private async Task OnListKeyDown(KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Delete:
                e.Handled = true;
                await RemoveSelectedImagesAsync();
                break;
        }
    }

    private async Task RemoveSelectedImagesAsync()
    {
        var targets = SelectedImages.ToList();

        if (targets.Count == 0)
        {
            return;
        }

        using var cts = new CancellationTokenSource();
        var progressVm = CreateProgressViewModel(targets.Count, "削除を開始しています...");
        progressVm.IsCancelButtonVisible = true;
        progressVm.CancellationTokenSource = cts;
        var progressWindow = CreateProgressWindow(progressVm, "画像削除中");

        DisableMainWindow();
        progressWindow.Show();

        // 表示中の画像が削除対象に含まれる場合は事前に選択を外して描画切替を抑止
        var currentSelected = SelectedImage;
        if (currentSelected is not null && targets.Contains(currentSelected))
        {
            Application.Current.Dispatcher.Invoke(() => SelectedImage = null);
        }

        try
        {
            // SelectedImageの変更を一時的に抑止。ObservableHashSetの変更通知でSelectedImageが変わるのを防ぐため。
            IsSelectedImageChangeable = false;

            await Task.Run(() =>
            {
                for (int i = 0; i < targets.Count; i++)
                {
                    cts.Token.ThrowIfCancellationRequested();

                    var index = i;
                    var target = targets[index];

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ImagePaths.Remove(target);

                        progressVm.Current = index + 1;
                        progressVm.FileName = target.DisplayName;
                        progressVm.Progress = (int)((index + 1) * 100.0 / targets.Count);
                        progressVm.Message = $"{progressVm.Current}/{targets.Count} 削除中...";
                    });
                }

                Application.Current.Dispatcher.Invoke(() => SelectedImages.Clear());
            }, cts.Token);
        }
        catch (OperationCanceledException)
        {
            progressVm.Message = "キャンセルされました";
        }
        finally
        {
            IsSelectedImageChangeable = true;
            await FinalizeProcessing(progressWindow);
        }
    }

    [RelayCommand]
    private void OnOpenImageFolderInExplorer(object sender)
    {
        if (SelectedImage is null)
        {
            ShowErrorMessageBox("エラー", "画像が選択されていません。");
            return;
        }

        var imagePath = SelectedImage.Item;
        if (!File.Exists(imagePath))
        {
            ShowErrorMessageBox("エラー", $"画像ファイル({imagePath})が存在しません。");
            return;
        }

        // /select, オプションでファイルを選択した状態でエクスプローラーを開く
        System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{imagePath}\"");
    }

    #endregion

    #region 切り抜き処理
    /// <summary>
    /// 手動設定ウィンドウが開いているかどうか
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(CropImageCommand))]
    private bool isManualSettingsWindowOpen = false;

    /// <summary>
    /// 切り抜き範囲のRectangle
    /// </summary>
    [ObservableProperty]
    private Rectangle? createdRectangle;

    /// <summary>
    /// 切り抜き処理キャンセル用トークンのソース
    /// </summary>
    private CancellationTokenSource? _cancellationTokenSource;

    /// <summary>
    /// 切り抜きコマンドが実行可能かどうかを判定する。
    /// </summary>
    /// <returns>手動設定ウィンドウが開いていない場合はtrue</returns>
    private bool CanCropImage() => !IsManualSettingsWindowOpen;

    /// <summary>
    /// 画像の切り抜き処理を実行するコマンド。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCropImage))]
    private async Task OnCropImage()
    {
        if (!ValidateInputs())
            return;

        var cropParameters = PrepareCropParameters(CreatedRectangle!); // ValidateInputsがtrueなら、CreatedRectangleはnullではない

        _cancellationTokenSource = new CancellationTokenSource();

        var progressVm = CreateProgressViewModel();
        progressVm.IsCancelButtonVisible = true;
        progressVm.CancellationTokenSource = _cancellationTokenSource;

        var progressWindow = CreateProgressWindow(progressVm);

        DisableMainWindow();
        progressWindow.Show();

        string messageBoxTitle = "処理完了";
        string messageBoxText = string.Empty;

        try
        {
            var errors = await ProcessImagesAsync(cropParameters, progressVm, _cancellationTokenSource.Token);

            // エラーがあった場合は最後にまとめて表示
            if (errors.IsEmpty)
            {
                progressVm.Message = "完了";

                messageBoxTitle = "処理完了";
                messageBoxText = "すべての画像の切り抜きが正常に完了しました。";
            }
            else
            {
                var errorSummary = $"切り抜きは完了しましたが、以下のファイルで処理エラーが発生しました:\n{string.Join("\n", errors.Take(5))}";
                if (errors.Count > 5)
                {
                    errorSummary += $"\n...他 {errors.Count - 5} 件のエラー";
                }

                progressVm.Message = errorSummary;

                messageBoxTitle = "処理完了（エラーあり）";
                messageBoxText = errorSummary;
            }
            progressVm.Progress = 100;
        }
        catch (OperationCanceledException)
        {
            progressVm.Message = "キャンセルされました";

            messageBoxTitle = "処理キャンセル";
            messageBoxText = "画像の切り抜き処理はキャンセルされました。";
        }
        catch (Exception ex)
        {
            progressVm.Message = $"エラーが発生しました: {ex.Message}";

            messageBoxTitle = "処理中のエラー";
            messageBoxText = $"画像の切り抜き処理中に以下のエラーが発生し、最後まで完了できませんでした。\n{ex.Message}";
        }
        finally
        {
            await FinalizeProcessing(progressWindow);
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        ShowInformationMessageBox(messageBoxTitle, messageBoxText);
    }

    /// <summary>
    /// アクセス権エラーをスキップしながら、フォルダ内のファイルを再帰的に列挙する。
    /// </summary>
    /// <param name="rootPath">検索開始フォルダ</param>
    /// <param name="skippedFolders">スキップされたフォルダのリスト（出力用）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>見つかったファイルパスの列挙</returns>
    private static IEnumerable<string> EnumerateFilesSafe(string rootPath, List<string> skippedFolders, CancellationToken cancellationToken)
    {
        var files = new List<string>();
        var directories = new Stack<string>();
        directories.Push(rootPath);

        while (directories.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentDir = directories.Pop();

            // 現在のディレクトリ内のファイルを取得
            try
            {
                files.AddRange(Directory.EnumerateFiles(currentDir));
            }
            catch (UnauthorizedAccessException)
            {
                lock (skippedFolders)
                {
                    skippedFolders.Add(currentDir);
                }
            }
            catch (DirectoryNotFoundException)
            {
                // ディレクトリが見つからない場合はスキップ
            }

            // サブディレクトリを取得してスタックに追加
            try
            {
                foreach (var subDir in Directory.EnumerateDirectories(currentDir))
                {
                    directories.Push(subDir);
                }
            }
            catch (UnauthorizedAccessException)
            {
                lock (skippedFolders)
                {
                    if (!skippedFolders.Contains(currentDir))
                    {
                        skippedFolders.Add(currentDir);
                    }
                }
            }
            catch (DirectoryNotFoundException)
            {
                // ディレクトリが見つからない場合はスキップ
            }
        }

        return files;
    }

    /// <summary>
    /// 入力値のバリデーションを行う。
    /// </summary>
    /// <returns>有効な場合はtrue</returns>
    private bool ValidateInputs()
    {
        if (ImagePaths.Count == 0)
        {
            ShowErrorMessageBox("エラー", "入力画像がありません。");
            return false;
        }

        if (!Directory.Exists(OutputSettings.FolderPath))
        {
            ShowErrorMessageBox("エラー", "出力フォルダが存在しません。");
            return false;
        }

        if (CreatedRectangle is null)
        {
            ShowErrorMessageBox("エラー", "切り抜く範囲を選択してください。");
            return false;
        }

        return true;
    }

    /// <summary>
    /// 切り抜きパラメータを作成する。
    /// </summary>
    /// <param name="createdRectangle">切り抜き範囲</param>
    /// <returns>切り抜きパラメータ</returns>
    private CropParameters PrepareCropParameters(Rectangle createdRectangle)
    {
        if (EditableImageControl is null)
        {
            throw new InvalidOperationException("EditableImageControl is not set.");
        }

        (Point leftTop, Point rightBottomOriginal) = EditableImageControl.GetRectangleCoordinates(createdRectangle);

        return new CropParameters
        {
            LeftTop = leftTop,
            RightBottom = rightBottomOriginal,
            OutputFolderPath = OutputSettings.FolderPath,
            OutputExtension = OutputSettings.Extension,
            ImagePaths = ImagePaths.ToList()
        };
    }

    /// <summary>
    /// 進捗表示用ViewModelを作成する。
    /// </summary>
    /// <returns>ProgressViewModel</returns>
    private ProgressViewModel CreateProgressViewModel(int? totalOverride = null, string? initialMessage = null)
    {
        return new ProgressViewModel
        {
            Total = totalOverride ?? ImagePaths.Count,
            Current = 0,
            FileName = string.Empty,
            Message = initialMessage ?? "処理を開始しています..."
        };
    }

    /// <summary>
    /// 進捗表示用ウィンドウを作成する。
    /// </summary>
    /// <param name="progressVm">バインドするViewModel</param>
    /// <returns>ProgressWindow</returns>
    private ProgressWindow CreateProgressWindow(ProgressViewModel progressVm, string? title = null)
    {
        var window = new ProgressWindow
        {
            DataContext = progressVm,
            Owner = Application.Current.MainWindow,
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };

        if (!string.IsNullOrWhiteSpace(title))
        {
            window.Title = title;
        }

        return window;
    }

    /// <summary>
    /// メインウィンドウを無効化する。
    /// </summary>
    private static void DisableMainWindow()
    {
        if (Application.Current.MainWindow is Window mainWindow)
        {
            mainWindow.IsEnabled = false;
        }
    }

    /// <summary>
    /// 画像の切り抜き処理を非同期で実行する。
    /// </summary>
    /// <param name="parameters">切り抜きパラメータ</param>
    /// <param name="progressVm">進捗ViewModel</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>エラーが発生したファイル名のリスト</returns>
    private async Task<ConcurrentBag<string>> ProcessImagesAsync(CropParameters parameters, ProgressViewModel progressVm, CancellationToken cancellationToken)
    {
        return await Task.Run(() => ProcessImages(parameters, OutputSettings.IsUseMultiThreading, progressVm, cancellationToken), cancellationToken);
    }

    /// <summary>
    /// 画像を並列で切り抜き処理する。
    /// </summary>
    private ConcurrentBag<string> ProcessImages(CropParameters parameters, bool isParallel, ProgressViewModel progressVm, CancellationToken cancellationToken)
    {
        var processedCount = 0;
        var errors = new ConcurrentBag<string>();

        var parallelOptions = new ParallelOptions
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = isParallel ? Math.Max(1, Environment.ProcessorCount - 1) : 1 // 並列処理ならCPUコア数-1、直列処理なら1
        };

        Parallel.ForEach(parameters.ImagePaths, parallelOptions, imagePath =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = ProcessSingleImage(imagePath, parameters);

            var currentCount = Interlocked.Increment(ref processedCount);

            if (!result.Success)
            {
                errors.Add($"{Path.GetFileName(imagePath.Item)}: {result.ErrorMessage}");
            }

            UpdateProgress(progressVm, currentCount, imagePath, result, parameters.ImagePaths.Count);
        });

        return errors;
    }

    /// <summary>
    /// 1枚の画像を切り抜き、保存する。
    /// </summary>
    /// <param name="imagePath">画像ファイルパス</param>
    /// <param name="parameters">切り抜きパラメータ</param>
    /// <returns>処理結果</returns>
    private ImageProcessResult ProcessSingleImage(SelectionItem<string> imagePath, CropParameters parameters)
    {
        try
        {
            using Mat mat = Cv2.ImRead(imagePath.Item);

            if (mat.Empty())
            {
                throw new InvalidDataException("画像を読み込めませんでした。");
            }

            // 座標を画像サイズに合わせて補正
            var errormessage = $"切り抜き範囲\"{PointToString(parameters.LeftTop)}-{PointToString(parameters.RightBottom)}\"が、画像サイズ\"{mat.Width}x{mat.Height}\"を超えています。\n" +
                                "切り取り範囲が画像外でも処理したい場合は、設定を変更してください。";
            var rightBottom = new Point(parameters.RightBottom.X, parameters.RightBottom.Y);
            if (rightBottom.X > mat.Width)
            {
                if (OutputSettings.IsCropOutsideImageAllowed)
                    rightBottom.X = mat.Width;
                else
                    throw new Exception(errormessage);
            }
            if (rightBottom.Y > mat.Height)
            {
                if (OutputSettings.IsCropOutsideImageAllowed)
                    rightBottom.Y = mat.Height;
                else
                    throw new Exception(errormessage);
            }

            int width = (int)(rightBottom.X - parameters.LeftTop.X);
            int height = (int)(rightBottom.Y - parameters.LeftTop.Y);

            if (width <= 0 || height <= 0)
                throw new InvalidDataException("切り抜き範囲が不正です。");

            using Mat cropped = new(mat, new OpenCvSharp.Rect((int)parameters.LeftTop.X, (int)parameters.LeftTop.Y, width, height));
            var outputFilePath = Path.Combine(parameters.OutputFolderPath, Path.GetFileNameWithoutExtension(imagePath.Item) + parameters.OutputExtension);

            // 出力ディレクトリが存在しない場合は作成
            var outputDir = Path.GetDirectoryName(outputFilePath);
            if (!string.IsNullOrEmpty(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            cropped.SaveImage(outputFilePath);

            return new ImageProcessResult(true);
        }
        catch (Exception ex)
        {
            return new ImageProcessResult(false, ex.Message);
        }

        string PointToString(Point point)
        {
            var roundedX = Math.Round(point.X, 2, MidpointRounding.AwayFromZero);
            var roundedY = Math.Round(point.Y, 2, MidpointRounding.AwayFromZero);
            return $"({roundedX}, {roundedY})";
        }
    }

    /// <summary>
    /// 進捗情報を更新する。
    /// </summary>
    private static void UpdateProgress(ProgressViewModel progressVm, int processedCount, SelectionItem<string> imagePath, ImageProcessResult result, int totalCount)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            progressVm.Current = processedCount;
            progressVm.FileName = Path.GetFileName(imagePath.Item);
            progressVm.Progress = (int)(processedCount * 100.0 / totalCount);

            if (result.Success)
            {
                progressVm.Message = $"{processedCount}/{totalCount} 処理中...";
            }
            else
            {
                progressVm.Message = $"エラー: {Path.GetFileName(imagePath.Item)} - {result.ErrorMessage}";
            }
        });
    }

    /// <summary>
    /// 処理終了時の後処理を行う。
    /// </summary>
    private static async Task FinalizeProcessing(ProgressWindow progressWindow)
    {
        await Task.Delay(300); // ユーザーに最終状態を少し見せる
        progressWindow.Close();

        if (Application.Current.MainWindow is Window mw)
        {
            mw.IsEnabled = true;
            mw.Activate();
        }
    }

    /// <summary>
    /// 補助レコード: 処理開始時に指定する切り抜きパラメータ
    /// </summary>
    private record CropParameters
    {
        /// <summary>
        /// 切り抜き範囲の左上の座標
        /// </summary>
        public Point LeftTop { get; init; }

        /// <summary>
        /// 切り抜き範囲の右下の座標
        /// </summary>
        public Point RightBottom { get; init; }

        /// <summary>
        /// 出力フォルダのパス
        /// </summary>
        public string OutputFolderPath { get; init; } = string.Empty;

        /// <summary>
        /// 出力画像の拡張子
        /// </summary>
        public string OutputExtension { get; init; } = string.Empty;

        /// <summary>
        /// 切り抜く画像ファイルのパスのコレクション
        /// </summary>
        public List<SelectionItem<string>> ImagePaths { get; init; } = [];
    }

    /// <summary>
    /// 補助レコード: 1つの画像に対する処理結果
    /// </summary>
    /// <param name="Success">処理が成功したかどうか</param>
    /// <param name="ErrorMessage">処理に失敗した場合のエラーメッセージ</param>
    private record ImageProcessResult(bool Success, string ErrorMessage = "");
    #endregion

    #region ヘルパーメソッド
    private void ShowInformationMessageBox(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private MessageBoxResult ShowWarningMessageBox(string title, string message, MessageBoxButton buttons)
    {
        return MessageBox.Show(message, title, buttons, MessageBoxImage.Warning);
    }

    private void ShowErrorMessageBox(string title, string message)
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    #endregion

    #region 設定ウィンドウ
    /// <summary>
    /// 設定ウィンドウを開く
    /// </summary>
    [RelayCommand]
    private void OpenSettingsWindow()
    {
        SettingsWindow.ShowWindow(this, Application.Current.MainWindow);
    }
    #endregion

    #region ヘルプメニュー
    private const string GitHubRepoUrl = "https://github.com/Ringoacid/image-cropper";
    private const string GitHubApiLatestReleaseUrl = "https://api.github.com/repos/Ringoacid/image-cropper/releases/latest";

    private static readonly HttpClient _httpClient = new()
    {
        DefaultRequestHeaders = { { "User-Agent", "ImageCropper" } }
    };

    [RelayCommand]
    private void OpenGitHubRepository()
    {
        Process.Start(new ProcessStartInfo(GitHubRepoUrl) { UseShellExecute = true });
    }

    [RelayCommand]
    private async Task CheckForUpdates()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<JsonElement>(GitHubApiLatestReleaseUrl);

            var tagName = response.GetProperty("tag_name").GetString() ?? string.Empty;
            var releasePageUrl = response.GetProperty("html_url").GetString() ?? GitHubRepoUrl;

            // "v1.2.3" -> "1.2.3"
            var versionString = tagName.TrimStart('v');

            if (!Version.TryParse(versionString, out var latestVersion))
            {
                ShowErrorMessageBox("更新エラー", $"バージョン情報のパースに失敗しました: {tagName}");
                return;
            }

            var currentVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);

            if (latestVersion <= currentVersion)
            {
                ShowInformationMessageBox("更新の確認", "最新バージョンです。更新はありません。");
                return;
            }

            var result = MessageBox.Show(
                $"新しいバージョン {latestVersion} が利用可能です。\nリリースページを開きますか？",
                "更新の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Process.Start(new ProcessStartInfo(releasePageUrl) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessageBox("更新エラー", $"更新の確認中にエラーが発生しました:\n{ex.Message}");
        }
    }

    [RelayCommand]
    private void ShowAbout()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version?.ToString() ?? "不明";
        var appName = assembly.GetName().Name ?? "ImageCropper";
        var execDir = AppDomain.CurrentDomain.BaseDirectory;

        MessageBox.Show(
            $"アプリケーション名: {appName}\nバージョン: {version}\n実行ディレクトリ: {execDir}",
            $"{appName}について",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    #endregion
}