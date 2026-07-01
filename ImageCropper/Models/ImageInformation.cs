using OpenCvSharp;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageCropper.Models;

public record ImageInformation
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Channels { get; private set; }

    /// <summary>
    /// EXIFのOrientationタグの値（1が既定・無回転。取得できない場合も1）
    /// </summary>
    public int ExifOrientation { get; private set; } = 1;

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern uint GetShortPathName(
        string lpszLongPath,
        System.Text.StringBuilder lpszShortPath,
        uint cchBuffer);

    /// <summary>
    /// BitmapMetadataからEXIF Orientationタグ（ID 274）を読み取る。
    /// コーデックによってはクエリパスが異なる、または対応していない場合があるため、
    /// 取得できない場合は既定値（1: 無回転）を返す。
    /// </summary>
    private static int ReadExifOrientation(BitmapMetadata? metadata)
    {
        if (metadata is null)
            return 1;

        foreach (var query in new[] { "/app1/ifd/exif/{ushort=274}", "/app1/ifd/{ushort=274}" })
        {
            try
            {
                if (metadata.GetQuery(query) is { } value)
                {
                    int orientation = Convert.ToInt32(value);
                    if (orientation is >= 1 and <= 8)
                    {
                        return orientation;
                    }
                }
            }
            catch
            {
                // このクエリパスに対応していない、またはメタデータが存在しない場合は無視
            }
        }

        return 1;
    }

    private static string GetShortPath(string longPath)
    {
        var shortPath = new System.Text.StringBuilder(260);
        uint result = GetShortPathName(longPath, shortPath, (uint)shortPath.Capacity);
        if (result > shortPath.Capacity)
        {
            shortPath.EnsureCapacity((int)result);
            result = GetShortPathName(longPath, shortPath, result);
        }
        return result > 0 ? shortPath.ToString() : longPath;
    }

    public override string ToString()
    {
        return $"横 × 縦 = {Width} × {Height}";
    }

    public ImageInformation(int width, int height, int channels)
    {
        Width = width;
        Height = height;
        Channels = channels;
    }

    public ImageInformation(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("ファイルが見つかりませんでした", filePath);
        }

        try
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var decoder = BitmapDecoder.Create(
                    stream,
                    BitmapCreateOptions.DelayCreation,
                    BitmapCacheOption.None);

                var frame = decoder.Frames[0];
                Width = frame.PixelWidth;
                Height = frame.PixelHeight;

                int bitsPerPixel = frame.Format.BitsPerPixel;
                Channels = bitsPerPixel switch
                {
                    8 => 1,
                    16 => 2,
                    24 => 3,
                    32 => 4,
                    _ => Math.Max(1, bitsPerPixel / 8)
                };

                ExifOrientation = ReadExifOrientation(frame.Metadata as BitmapMetadata);
                return;
            }
        }
        catch
        {
            // BitmapDecoderが失敗した場合は、OpenCVのImReadにショートパスを渡してフォールバック
            string shortPath = GetShortPath(filePath);
            using Mat img = Cv2.ImRead(shortPath, ImreadModes.Unchanged);
            if (img.Empty())
            {
                throw new ArgumentException("画像の読み込みに失敗しました");
            }

            Width = img.Width;
            Height = img.Height;
            Channels = img.Channels();
        }
    }
}
