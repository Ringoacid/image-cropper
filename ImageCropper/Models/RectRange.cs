namespace ImageCropper.Models;

// 常にpx単位、左上と右下の座標を保持する範囲
public record RectRange(double X1, double Y1, double X2, double Y2);
