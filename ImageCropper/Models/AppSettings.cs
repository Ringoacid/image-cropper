using System.Text.Json.Serialization;

namespace ImageCropper.Models;

/// <summary>
/// アプリケーション設定をJSONでシリアライズするためのクラス
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 出力に関する設定
    /// </summary>
    public OutputSettings Output { get; set; } = new();

    /// <summary>
    /// UIに関する設定
    /// </summary>
    public UISettings UI { get; set; } = new();

    /// <summary>
    /// テンプレートのリスト
    /// </summary>
    public List<CropPreset> Presets { get; set; } = [];

    #region 後方互換性のためのプロパティ（JSONデシリアライズ時に使用）

    /// <summary>
    /// 出力画像の拡張子（後方互換性のため）
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string OutputExtension
    {
        get => Output.Extension;
        set => Output.Extension = value;
    }

    /// <summary>
    /// 出力フォルダパス（後方互換性のため）
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string OutputFolderPath
    {
        get => Output.FolderPath;
        set => Output.FolderPath = value;
    }

    /// <summary>
    /// マルチスレッド処理を使用するかどうか（後方互換性のため）
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsUseMultiThreading
    {
        get => Output.IsUseMultiThreading;
        set => Output.IsUseMultiThreading = value;
    }

    #endregion
}
