using ImageCropper.Models;
using ImageCropper.ViewModels.Windows;
using ImageCropper.Views.UserControls;
using System.Windows;
using System.Windows.Shapes;

namespace ImageCropper.Views.Windows;

public enum RangeUnit
{
    Pixel,
    Percent
}

/// <summary>
/// CropRangeManualSettingsWindow.xaml の相互作用ロジック
/// </summary>
public partial class CropRangeManualSettingsWindow : Window
{
    public CropRangeManualSettingsWindowViewModel ViewModel { get; set; }

    public CropRangeManualSettingsWindow(ImageInformation imageinfo, Rectangle rectangle, EditableImage editableImage)
    {
        ViewModel = new CropRangeManualSettingsWindowViewModel(imageinfo, rectangle, editableImage);
        DataContext = this;

        InitializeComponent();
    }

    public static async Task<Rectangle?> ShowWindowAsync(ImageInformation imageinfo, Rectangle rectangle, EditableImage editableImage, Action<CropRangeManualSettingsWindow>? onWindowCreated = null)
    {
        var window = new CropRangeManualSettingsWindow(imageinfo, rectangle, editableImage);
        var tcs = new TaskCompletionSource<Rectangle?>();

        // ウィンドウ参照を呼び出し元に通知
        onWindowCreated?.Invoke(window);

        // CurrentRangeが変更されたときにEditableImageの矩形を更新
        window.ViewModel.CurrentRangeChanged += (sender, range) =>
        {
            editableImage.UpdateRectangle(range);
        };

        // EditableImageの矩形座標が変更されたときにViewModelを更新
        EventHandler<RectRange> onRectangleCoordinatesChanged = (sender, range) =>
        {
            window.ViewModel.UpdateCurrentRangeWithoutEvent(range);
        };

        editableImage.RectangleCoordinatesChanged += onRectangleCoordinatesChanged;

        window.ViewModel.DialogResultRequested += (sender, result) =>
        {
            // イベントハンドラを解除
            editableImage.RectangleCoordinatesChanged -= onRectangleCoordinatesChanged;

            if (result)
            {
                // OKが押された場合、新しいRectangleを作成して返す
                var range = window.ViewModel.CurrentRange;
                var newRectangle = window.ViewModel.EditableImage.CreateRectangleFromCoordinates(range);
                tcs.TrySetResult(newRectangle);
            }
            else
            {
                // キャンセルが押された場合
                tcs.TrySetResult(window.ViewModel.OriginalRectangle);
            }
            window.Close();
        };

        // ×ボタンで閉じた場合の処理
        window.Closed += (sender, e) =>
        {
            // イベントハンドラを解除
            editableImage.RectangleCoordinatesChanged -= onRectangleCoordinatesChanged;
            // まだ完了していない場合は元の矩形を返す
            tcs.TrySetResult(window.ViewModel.OriginalRectangle);
        };

        window.Show();
        return await tcs.Task;
    }
}
