using System.Text.Json.Serialization;

namespace ImageCropper.Models;

/// <summary>
/// テンプレート（切り取り範囲と任意の出力設定）を表すクラス
/// </summary>
public class CropPreset
{
    /// <summary>
    /// テンプレート名
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 切り取り範囲（必須）
    /// </summary>
    public RectRange CropRange { get; set; } = new(0, 0, 0, 0);

    /// <summary>
    /// 出力画像形式（任意。nullの場合は適用しない）
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputExtension { get; set; }

    /// <summary>
    /// 出力先フォルダパス（任意。nullの場合は適用しない）
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? OutputFolderPath { get; set; }

    /// <summary>
    /// デフォルトコンストラクタ（JSONデシリアライズ用）
    /// </summary>
    public CropPreset() { }

    /// <summary>
    /// コピーコンストラクタ
    /// </summary>
    public CropPreset(CropPreset other)
    {
        Name = other.Name;
        CropRange = other.CropRange;
        OutputExtension = other.OutputExtension;
        OutputFolderPath = other.OutputFolderPath;
    }
}
