using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ImageCropper.Models;
using ImageCropper.Views.UserControls;
using System.Windows.Shapes;

namespace ImageCropper.ViewModels.Windows;

public partial class CropRangeManualSettingsWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private ImageInformation imageInformation;

    [ObservableProperty]
    private Rectangle originalRectangle;

    [ObservableProperty]
    private string originalCropRange = string.Empty;

    public EditableImage EditableImage { get; set; }

    public CropRangeManualSettingsWindowViewModel(ImageInformation imageinfo, Rectangle rectangle, EditableImage editableImage)
    {
        this.imageInformation = imageinfo;
        this.OriginalRectangle = rectangle;
        this.EditableImage = editableImage;

        (var lefttop, var rightbottom) = EditableImage.GetRectangleCoordinates(rectangle);
        OriginalCropRange = $"({lefttop.X:f2}, {lefttop.Y:f2}) - ({rightbottom.X:f2}, {rightbottom.Y:f2})";
        CurrentRange = new RectRange(lefttop.X, lefttop.Y, rightbottom.X, rightbottom.Y);
    }

    [ObservableProperty]
    private RangeUnit unit;

    partial void OnUnitChanged(RangeUnit value)
    {
        NotifyAllCoordinatesChanged();
    }

    public event EventHandler<RectRange>? CurrentRangeChanged;

    public RectRange CurrentRange
    {
        get;
        private set
        {
            if (!_suppressCurrentRangeEvent)
            {
                CurrentRangeChanged?.Invoke(this, value);
            }
            field = value;
        }
    }

    private bool _suppressCurrentRangeEvent = false;

    /// <summary>
    /// 外部（EditableImage）から CurrentRange を更新する（イベントなし）。
    /// UIプロパティのみ通知する。
    /// </summary>
    internal void UpdateCurrentRangeWithoutEvent(RectRange range)
    {
        _suppressCurrentRangeEvent = true;
        CurrentRange = range;
        _suppressCurrentRangeEvent = false;
        NotifyAllCoordinatesChanged();
    }

    public double MaxLeftTopX => RightBottomX;

    public double LeftTopX
    {
        get
        {
            return GetValueInCurrentUnitFromPixelValue(CurrentRange.X1, ImageInformation.Width);
        }
        set
        {
            var newX1 = GetPixelValueFromCurrentUnit(value, ImageInformation.Width);
            CurrentRange = CurrentRange with { X1 = newX1 };
            OnPropertyChanged(nameof(MinRightBottomX));
            OnPropertyChanged(nameof(LeftTopX));
            OnPropertyChanged(nameof(Width));
        }
    }

    public double MaxLeftTopY => RightBottomY;

    public double LeftTopY
    {
        get
        {
            return GetValueInCurrentUnitFromPixelValue(CurrentRange.Y1, ImageInformation.Height);
        }
        set
        {
            var newY1 = GetPixelValueFromCurrentUnit(value, ImageInformation.Height);
            CurrentRange = CurrentRange with { Y1 = newY1 };
            OnPropertyChanged(nameof(MinRightBottomY));
            OnPropertyChanged(nameof(LeftTopY));
            OnPropertyChanged(nameof(Height));
        }
    }

    public double MinRightBottomX => LeftTopX;

    public double RightBottomX
    {
        get
        {
            return GetValueInCurrentUnitFromPixelValue(CurrentRange.X2, ImageInformation.Width);
        }
        set
        {
            var newX2 = GetPixelValueFromCurrentUnit(value, ImageInformation.Width);
            CurrentRange = CurrentRange with { X2 = newX2 };
            OnPropertyChanged(nameof(MinRightBottomX));
            OnPropertyChanged(nameof(RightBottomX));
            OnPropertyChanged(nameof(Width));
        }
    }

    public double MinRightBottomY => LeftTopY;

    public double RightBottomY
    {
        get
        {
            return GetValueInCurrentUnitFromPixelValue(CurrentRange.Y2, ImageInformation.Height);
        }
        set
        {
            var newY2 = GetPixelValueFromCurrentUnit(value, ImageInformation.Height);
            CurrentRange = CurrentRange with { Y2 = newY2 };
            OnPropertyChanged(nameof(MinRightBottomY));
            OnPropertyChanged(nameof(RightBottomY));
            OnPropertyChanged(nameof(Height));
        }
    }

    public double Width
    {
        get
        {
            return GetValueInCurrentUnitFromPixelValue(CurrentRange.X2 - CurrentRange.X1, ImageInformation.Width);
        }
        set
        {
            var newWidth = GetPixelValueFromCurrentUnit(value, ImageInformation.Width);
            CurrentRange = CurrentRange with { X2 = CurrentRange.X1 + newWidth };
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(RightBottomX));
        }
    }

    public double Height
    {
        get
        {
            return GetValueInCurrentUnitFromPixelValue(CurrentRange.Y2 - CurrentRange.Y1, ImageInformation.Height);
        }
        set
        {
            var newHeight = GetPixelValueFromCurrentUnit(value, ImageInformation.Height);
            CurrentRange = CurrentRange with { Y2 = CurrentRange.Y1 + newHeight };
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(RightBottomY));
        }
    }

    // ヘルパーメソッド
    private double GetValueInCurrentUnitFromPixelValue(double pixelValue, double imagePixelValue)
    {
        return Unit switch
        {
            RangeUnit.Pixel => Math.Round(pixelValue, 2, MidpointRounding.AwayFromZero),
            RangeUnit.Percent => Math.Round(pixelValue / imagePixelValue * 100, 2, MidpointRounding.AwayFromZero),
            _ => throw new Exception($"Unknown unit type : {Unit}"),
        };
    }

    private double GetPixelValueFromCurrentUnit(double valueInCurrentUnit, double imagePixelValue)
    {
        return Unit switch
        {
            RangeUnit.Pixel => Math.Round(valueInCurrentUnit, 2, MidpointRounding.AwayFromZero),
            RangeUnit.Percent => Math.Round(valueInCurrentUnit / 100 * imagePixelValue, 2, MidpointRounding.AwayFromZero),
            _ => throw new Exception($"Unknown unit type : {Unit}"),
        };
    }

    private void NotifyAllCoordinatesChanged()
    {
        OnPropertyChanged(nameof(MaxLeftTopX));
        OnPropertyChanged(nameof(MaxLeftTopY));
        OnPropertyChanged(nameof(MinRightBottomX));
        OnPropertyChanged(nameof(MinRightBottomY));

        OnPropertyChanged(nameof(LeftTopX));
        OnPropertyChanged(nameof(LeftTopY));
        OnPropertyChanged(nameof(RightBottomX));
        OnPropertyChanged(nameof(RightBottomY));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
    }

    public event EventHandler<bool>? DialogResultRequested;

    [RelayCommand]
    private void OnOk()
    {
        DialogResultRequested?.Invoke(this, true);
    }

    [RelayCommand]
    private void OnCancel()
    {
        DialogResultRequested?.Invoke(this, false);
    }
}
