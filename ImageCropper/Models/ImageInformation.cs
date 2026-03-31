using OpenCvSharp;
using System.IO;

namespace ImageCropper.Models;

public record ImageInformation
{
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Channels { get; private set; }

    public override string ToString()
    {
        return $"横 × 縦 = {Width} × {Height}";
    }

    public ImageInformation(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("ファイルが見つかりませんでした", filePath);
        }

        // ImReadで画像を読み込むと、特殊な文字("Ø"[U+00D8]など)が含まれるパスで失敗することがあるため、バイト配列として読み込んでからデコードする
        byte[] imageData = File.ReadAllBytes(filePath);
        using Mat img = Cv2.ImDecode(imageData, ImreadModes.Unchanged);
        if (img.Empty())
        {
            throw new ArgumentException("画像の読み込みに失敗しました");
        }

        Width = img.Width;
        Height = img.Height;
        Channels = img.Channels();
    }
}
