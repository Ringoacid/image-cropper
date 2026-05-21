using OpenCvSharp;
using System.IO;
using System.Windows.Media.Imaging;

namespace ImageCropper.Models;

public record ImageInformation
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Channels { get; private set; }

    [System.Runtime.InteropServices.DllImport("kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode, SetLastError = true)]
    private static extern uint GetShortPathName(
        string lpszLongPath,
        System.Text.StringBuilder lpszShortPath,
        uint cchBuffer);

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
