using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageCropper.Models;

/// <summary>
/// UI表示に関する設定を保持する列挙型
/// </summary>
public enum RangeDisplayMode
{
    /// <summary>
    /// 左上(X, Y)と幅・高さ(W, H)をピクセルで表示
    /// </summary>
    XYWH_Pixel,
    /// <summary>
    /// 左上(X, Y)と幅・高さ(W, H)をパーセントで表示
    /// </summary>
    XYWH_Percent,
    /// <summary>
    /// 左上(X1, Y1)と右下(X2, Y2)をピクセルで表示
    /// </summary>
    X1Y1X2Y2_Pixel,
    /// <summary>
    /// 左上(X1, Y1)と右下(X2, Y2)をパーセントで表示
    /// </summary>
    X1Y1X2Y2_Percent
}

/// <summary>
/// UIに関する設定を保持するクラス
/// </summary>
public partial class UISettings : ObservableObject
{
    /// <summary>
    /// 切り取り範囲の表示モード
    /// </summary>
    [ObservableProperty]
    private RangeDisplayMode rangeDisplayMode = RangeDisplayMode.XYWH_Pixel;

    public UISettings()
    {
    }

    public UISettings(UISettings other)
    {
        RangeDisplayMode = other.RangeDisplayMode;
    }

    public void CopyFrom(UISettings other)
    {
        RangeDisplayMode = other.RangeDisplayMode;
    }
}
