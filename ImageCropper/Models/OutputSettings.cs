using CommunityToolkit.Mvvm.ComponentModel;

namespace ImageCropper.Models;

/// <summary>
/// 出力に関する設定を保持するクラス
/// </summary>
public partial class OutputSettings : ObservableObject
{
    /// <summary>
    /// 出力画像の拡張子
    /// </summary>
    [ObservableProperty]
    private string extension = ".png";

    /// <summary>
    /// 出力フォルダパス
    /// </summary>
    [ObservableProperty]
    private string folderPath = string.Empty;

    /// <summary>
    /// マルチスレッド処理を使用するかどうか
    /// </summary>
    [ObservableProperty]
    private bool isUseMultiThreading = true;

    /// <summary>
    /// 切り取り範囲が画像の外にまたがっているときも、その画像を処理するか
    /// </summary>
    [ObservableProperty]
    private bool isCropOutsideImageAllowed = true;

    /// <summary>
    /// 出力ファイル名パターン（プレースホルダーを含む）
    /// </summary>
    [ObservableProperty]
    private string fileNamePattern = "{FileName}_cropped";

    /// <summary>
    /// JPEG出力時の品質（0-100）
    /// </summary>
    [ObservableProperty]
    private int jpegQuality = 90;

    /// <summary>
    /// PNG出力時の圧縮レベル（0-9）
    /// </summary>
    [ObservableProperty]
    private int pngCompressionLevel = 1;


    /// <summary>
    /// デフォルト値で初期化された新しいインスタンスを作成する
    /// </summary>
    public OutputSettings()
    {
    }

    /// <summary>
    /// 別のOutputSettingsからコピーして新しいインスタンスを作成する
    /// </summary>
    /// <param name="other">コピー元</param>
    public OutputSettings(OutputSettings other)
    {
        Extension = other.Extension;
        FolderPath = other.FolderPath;
        IsUseMultiThreading = other.IsUseMultiThreading;
        IsCropOutsideImageAllowed = other.IsCropOutsideImageAllowed;
        FileNamePattern = other.FileNamePattern;
        JpegQuality = other.JpegQuality;
        PngCompressionLevel = other.PngCompressionLevel;
    }

    /// <summary>
    /// 別のOutputSettingsの値をこのインスタンスにコピーする
    /// </summary>
    /// <param name="other">コピー元</param>
    public void CopyFrom(OutputSettings other)
    {
        Extension = other.Extension;
        FolderPath = other.FolderPath;
        IsUseMultiThreading = other.IsUseMultiThreading;
        IsCropOutsideImageAllowed = other.IsCropOutsideImageAllowed;
        FileNamePattern = other.FileNamePattern;
        JpegQuality = other.JpegQuality;
        PngCompressionLevel = other.PngCompressionLevel;
    }
}
