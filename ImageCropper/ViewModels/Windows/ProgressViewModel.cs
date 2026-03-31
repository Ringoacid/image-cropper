using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageCropper.ViewModels.Windows;

public partial class ProgressViewModel : ObservableObject
{
    [ObservableProperty]
    private int total;

    [ObservableProperty]
    private int current;

    [ObservableProperty]
    private int progress; // 0-100

    [ObservableProperty]
    private string fileName = string.Empty;

    [ObservableProperty]
    private string message = string.Empty;

    /// <summary>
    /// キャンセルボタンを表示するかどうか
    /// </summary>
    [ObservableProperty]
    private bool isCancelButtonVisible = false;

    /// <summary>
    /// キャンセルが要求されたかどうか
    /// </summary>
    [ObservableProperty]
    private bool isCancellationRequested = false;

    /// <summary>
    /// キャンセル用のCancellationTokenSource
    /// </summary>
    public CancellationTokenSource? CancellationTokenSource { get; set; }

    /// <summary>
    /// キャンセルコマンド
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        if (CancellationTokenSource is not null && !CancellationTokenSource.IsCancellationRequested)
        {
            CancellationTokenSource.Cancel();
            IsCancellationRequested = true;
            Message = "キャンセル中...";
        }
    }
}
