namespace ImageCropper.Models;

// 常にpx単位、左上と右下の座標を保持する範囲
public record RectRange
{
    public double X1 { get; init; }
    public double Y1 { get; init; }
    public double X2 { get; init; }
    public double Y2 { get; init; }

    public RectRange(double x1, double y1, double x2, double y2)
    {
        double clampedX1 = Math.Max(0, x1);
        double clampedY1 = Math.Max(0, y1);
        double clampedX2 = Math.Max(0, x2);
        double clampedY2 = Math.Max(0, y2);

        X1 = Math.Min(clampedX1, clampedX2);
        X2 = Math.Max(clampedX1, clampedX2);
        Y1 = Math.Min(clampedY1, clampedY2);
        Y2 = Math.Max(clampedY1, clampedY2);
    }

    public void Deconstruct(out double x1, out double y1, out double x2, out double y2)
    {
        x1 = X1;
        y1 = Y1;
        x2 = X2;
        y2 = Y2;
    }
}
