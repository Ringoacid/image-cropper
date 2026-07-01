using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ImageCropper.ViewModels.Windows;

/// <summary>
/// 補助レコード: バッチ処理における1ファイルの処理結果
/// </summary>
/// <param name="FileName">ファイル名</param>
/// <param name="Success">処理が成功したかどうか</param>
/// <param name="ErrorMessage">処理に失敗した場合のエラーメッセージ</param>
public record BatchResultItem(string FileName, bool Success, string ErrorMessage);

/// <summary>
/// 処理結果レポートウィンドウのViewModel。
/// </summary>
public partial class BatchResultWindowViewModel : ObservableObject
{
    /// <summary>
    /// 処理対象の総数
    /// </summary>
    public int TotalCount { get; }

    /// <summary>
    /// 成功した件数
    /// </summary>
    public int SuccessCount { get; }

    /// <summary>
    /// 失敗した件数
    /// </summary>
    public int FailureCount => TotalCount - SuccessCount;

    /// <summary>
    /// 失敗したファイルの一覧
    /// </summary>
    public ObservableCollection<BatchResultItem> FailedItems { get; }

    /// <summary>
    /// サマリーテキスト
    /// </summary>
    public string SummaryText => $"{TotalCount}件中 {SuccessCount}件成功、{FailureCount}件失敗";

    /// <summary>
    /// 失敗したファイルが1件以上あるかどうか
    /// </summary>
    public bool HasFailures => FailedItems.Count > 0;

    public BatchResultWindowViewModel(int totalCount, int successCount, IEnumerable<BatchResultItem> failedItems)
    {
        TotalCount = totalCount;
        SuccessCount = successCount;
        FailedItems = new ObservableCollection<BatchResultItem>(failedItems);
    }
}
