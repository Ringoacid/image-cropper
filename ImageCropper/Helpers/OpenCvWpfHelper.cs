using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OpenCvSharp;

namespace ImageCropper.Helpers;

/// <summary>
/// OpenCVのMatとWPFのBitmapSource間の相互変換をサポートするヘルパークラス。
/// </summary>
public static class OpenCvWpfHelper
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetShortPathName(
        [MarshalAs(UnmanagedType.LPWStr)] string lpszLongPath,
        [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszShortPath,
        uint cchBuffer);

    /// <summary>
    /// 指定されたロングパスからショートパス（8.3形式）を取得します。
    /// </summary>
    /// <param name="longPath">変換元のロングパス</param>
    /// <returns>ショートパス。失敗した場合は元のパス</returns>
    public static string GetShortPath(string longPath)
    {
        if (string.IsNullOrEmpty(longPath))
            return longPath;

        var builder = new StringBuilder(260);
        uint result = GetShortPathName(longPath, builder, (uint)builder.Capacity);
        if (result > builder.Capacity)
        {
            builder.EnsureCapacity((int)result);
            result = GetShortPathName(longPath, builder, result);
        }

        return result > 0 ? builder.ToString() : longPath;
    }

    /// <summary>
    /// ファイルパスから画像ファイルをMatとしてロードします。
    /// ショートパスを使用して読み込みを試み、失敗した場合はバイト配列として読み込んでデコードします。
    /// </summary>
    /// <param name="filePath">画像ファイルのパス</param>
    /// <returns>読み込まれたMatオブジェクト</returns>
    public static Mat LoadImage(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            throw new ArgumentNullException(nameof(filePath));

        string shortPath = GetShortPath(filePath);
        Mat mat = Cv2.ImRead(shortPath, ImreadModes.Unchanged);
        if (mat == null || mat.Empty())
        {
            mat?.Dispose();
            byte[] bytes = File.ReadAllBytes(filePath);
            mat = Cv2.ImDecode(bytes, ImreadModes.Unchanged);
        }
        return mat;
    }

    /// <summary>
    /// OpenCVのMatからWPFのBitmapSourceを生成します。
    /// 浮動小数点や16bitなどのピクセル深度、および1/3/4チャンネル以外のチャンネル数の画像も、
    /// WPFで表示可能な8bit（Gray8, Bgr24, Bgra32）に自動で正規化・変換してコピーします。
    /// </summary>
    /// <param name="mat">変換元のMatオブジェクト</param>
    /// <returns>生成されたBitmapSource</returns>
    public static BitmapSource ToBitmapSource(Mat mat)
    {
        if (mat == null)
            throw new ArgumentNullException(nameof(mat));
        if (mat.IsDisposed)
            throw new ObjectDisposedException(nameof(mat));

        // 浮動小数点数(32F, 64F)や16ビットの画像を8ビットにスケール変換してコピー
        using Mat tempMat = new Mat();
        double scale = 1.0;
        if (mat.Depth() == MatType.CV_16U) scale = 255.0 / 65535.0;
        else if (mat.Depth() == MatType.CV_16S) scale = 255.0 / 32767.0;
        else if (mat.Depth() == MatType.CV_32F || mat.Depth() == MatType.CV_64F) scale = 255.0;

        mat.ConvertTo(tempMat, MatType.CV_8U, scale);

        // チャンネル数をチェックし、必要に応じて変換
        using Mat finalMat = new Mat();
        int channels = tempMat.Channels();
        if (channels == 1 || channels == 3 || channels == 4)
        {
            tempMat.CopyTo(finalMat);
        }
        else
        {
            // サポートされていない特殊なチャンネル数 (e.g. 2 channels) の場合は
            // チャンネル0を抽出してからGRAY2BGRに変換する
            using Mat singleChannel = new Mat();
            Cv2.ExtractChannel(tempMat, singleChannel, 0);
            Cv2.CvtColor(singleChannel, finalMat, ColorConversionCodes.GRAY2BGR);
        }

        // WPFのピクセルフォーマットにマッピング
        PixelFormat pixelFormat;
        channels = finalMat.Channels();
        if (channels == 1)
            pixelFormat = PixelFormats.Gray8;
        else if (channels == 3)
            pixelFormat = PixelFormats.Bgr24;
        else if (channels == 4)
            pixelFormat = PixelFormats.Bgra32;
        else
            throw new NotSupportedException($"サポートされていないチャンネル数です: {channels}");

        int stride = (int)finalMat.Step();

        // BitmapSource.Createを使用してピクセルデータをコピーしてBitmapSourceを生成
        var bitmapSource = BitmapSource.Create(
            finalMat.Width,
            finalMat.Height,
            96, // DPI X
            96, // DPI Y
            pixelFormat,
            null, // パレット
            finalMat.Data,
            stride * finalMat.Height,
            stride
        );
        bitmapSource.Freeze();
        return bitmapSource;
    }
}
