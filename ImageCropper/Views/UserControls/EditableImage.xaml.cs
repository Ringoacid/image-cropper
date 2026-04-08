using ImageCropper.Models;
using ImageCropper.ViewModels.Windows;
using ImageCropper.Views.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Key = System.Windows.Input.Key;

namespace ImageCropper.Views.UserControls;

/// <summary>
/// 画像表示と矩形アノテーション編集機能を提供するユーザーコントロール
/// </summary>
public partial class EditableImage : UserControl
{
    #region 定数
    private const double MinZoom = 0.1;
    private const double MaxZoom = 10.0;
    private const double ZoomFactor = 1.1;
    #endregion

    #region 依存関係プロパティ - 画像表示

    /// <summary>
    /// 表示する画像ソースの依存関係プロパティ
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty =
        DependencyProperty.Register(
            nameof(ImageSource),
            typeof(ImageSource),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnImageSourceChanged,
                null,
                false,
                UpdateSourceTrigger.LostFocus));

    /// <summary>
    /// 表示する画像ソース
    /// </summary>
    public ImageSource ImageSource
    {
        get => (ImageSource)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EditableImage control) return;
        if (e.NewValue is not ImageSource newImageSource) return;
        control.image.Source = newImageSource;
    }

    #endregion

    #region 依存関係プロパティ - 矩形編集

    /// <summary>
    /// 矩形の枠線の太さ
    /// </summary>
    public static readonly DependencyProperty EdgeThicknessProperty =
        DependencyProperty.Register(
            nameof(EdgeThickness),
            typeof(double),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEdgeThicknessChanged));

    public double EdgeThickness
    {
        get => (double)GetValue(EdgeThicknessProperty);
        set => SetValue(EdgeThicknessProperty, value);
    }

    private static void OnEdgeThicknessChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EditableImage control)
            control.ReSetResizeHandles();
    }

    /// <summary>
    /// リサイズハンドル（楕円）のサイズ
    /// </summary>
    public static readonly DependencyProperty EllipseSizeProperty =
        DependencyProperty.Register(
            nameof(EllipseSize),
            typeof(double),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnEllipseSizeChanged));

    public double EllipseSize
    {
        get => (double)GetValue(EllipseSizeProperty);
        set => SetValue(EllipseSizeProperty, value);
    }

    private static void OnEllipseSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EditableImage control)
            control.ReSetResizeHandles();
    }

    /// <summary>
    /// 新規作成する矩形の枠線色
    /// </summary>
    public static readonly DependencyProperty NewRectangleStrokeProperty =
        DependencyProperty.Register(
            nameof(NewRectangleStroke),
            typeof(Brush),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush NewRectangleStroke
    {
        get => (Brush)GetValue(NewRectangleStrokeProperty);
        set => SetValue(NewRectangleStrokeProperty, value);
    }

    /// <summary>
    /// 新規作成する矩形の塗りつぶし色
    /// </summary>
    public static readonly DependencyProperty NewRectangleFillProperty =
        DependencyProperty.Register(
            nameof(NewRectangleFill),
            typeof(Brush),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromArgb(50, 255, 0, 0)), FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush NewRectangleFill
    {
        get => (Brush)GetValue(NewRectangleFillProperty);
        set => SetValue(NewRectangleFillProperty, value);
    }

    /// <summary>
    /// 選択中の矩形のインデックス（読み取り専用）
    /// </summary>
    private static readonly DependencyPropertyKey SelectedRectangleIndexPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SelectedRectangleIndex),
            typeof(int),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.None, OnSelectedRectangleIndexChanged));

    public static readonly DependencyProperty SelectedRectangleIndexProperty = SelectedRectangleIndexPropertyKey.DependencyProperty;

    public int SelectedRectangleIndex
    {
        get => (int)GetValue(SelectedRectangleIndexProperty);
        protected set => SetValue(SelectedRectangleIndexPropertyKey, value);
    }

    private static void OnSelectedRectangleIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is EditableImage control)
            control.UpdateSelectedRectangle();
    }

    /// <summary>
    /// 選択中の矩形オブジェクト（読み取り専用）
    /// </summary>
    private static readonly DependencyPropertyKey SelectedRectanglePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(SelectedRectangle),
            typeof(Rectangle),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty SelectedRectangleProperty = SelectedRectanglePropertyKey.DependencyProperty;

    public Rectangle? SelectedRectangle
    {
        get => (Rectangle?)GetValue(SelectedRectangleProperty);
        private set => SetValue(SelectedRectanglePropertyKey, value);
    }

    /// <summary>
    /// 新規作成する矩形のラベルクラス
    /// </summary>
    public static readonly DependencyProperty LabelClassProperty =
        DependencyProperty.Register(
            nameof(LabelClass),
            typeof(int),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(0));

    public int LabelClass
    {
        get => (int)GetValue(LabelClassProperty);
        set => SetValue(LabelClassProperty, value);
    }

    /// <summary>
    /// 画像情報（CropRangeManualSettingsWindowで使用）
    /// </summary>
    public static readonly DependencyProperty ImageInformationProperty =
        DependencyProperty.Register(
            nameof(ImageInformation),
            typeof(ImageInformation),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(null, OnImageInformationChanged));

    public ImageInformation? ImageInformation
    {
        get => (ImageInformation?)GetValue(ImageInformationProperty);
        set => SetValue(ImageInformationProperty, value);
    }

    private static void OnImageInformationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EditableImage control) return;
        if (e.NewValue is null) return;

        // 画像情報が更新されたら、レイアウト更新後に矩形を再作成
        control.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
        {
            control.RecreateRectangleFromCropRange();
        }));
    }

    /// <summary>
    /// 切り取り範囲のピクセル座標（元画像基準）
    /// 外部からピクセル座標を設定すると、表示座標に変換して矩形を再作成
    /// </summary>
    public static readonly DependencyProperty CropRangeProperty =
        DependencyProperty.Register(
            nameof(CropRange),
            typeof(RectRange),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnCropRangeChanged));

    public RectRange? CropRange
    {
        get => (RectRange?)GetValue(CropRangeProperty);
        set => SetValue(CropRangeProperty, value);
    }

    private static void OnCropRangeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EditableImage control) return;

        // 内部での再作成中や、ユーザーが操作中の場合はスキップ
        if (control.isRecreatingRectangle || control.isDragging_Left || control.IsResizing || control.isMakingRectangle) return;

        // 外部からピクセル座標が設定された場合に矩形を再作成
        control.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Loaded, new Action(() =>
        {
            control.RecreateRectangleFromCropRange();
        }));
    }

    /// <summary>
    /// 手動設定ウィンドウが開いているかどうか
    /// </summary>
    public static readonly DependencyProperty IsManualSettingsWindowOpenProperty =
        DependencyProperty.Register(
            nameof(IsManualSettingsWindowOpen),
            typeof(bool),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsManualSettingsWindowOpen
    {
        get => (bool)GetValue(IsManualSettingsWindowOpenProperty);
        set => SetValue(IsManualSettingsWindowOpenProperty, value);
    }

    /// <summary>
    /// UI設定
    /// </summary>
    public static readonly DependencyProperty UISettingsProperty =
        DependencyProperty.Register(
            nameof(UISettings),
            typeof(UISettings),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(null));

    public UISettings? UISettings
    {
        get => (UISettings?)GetValue(UISettingsProperty);
        set => SetValue(UISettingsProperty, value);
    }

    /// <summary>
    /// 作成された矩形（外部からの設定・取得用）
    /// </summary>
    public static readonly DependencyProperty CreatedRectangleProperty =
        DependencyProperty.Register(
            nameof(CreatedRectangle),
            typeof(Rectangle),
            typeof(EditableImage),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, CreatedRectangleChanged));

    public Rectangle? CreatedRectangle
    {
        get => (Rectangle)GetValue(CreatedRectangleProperty);
        set => SetValue(CreatedRectangleProperty, value);
    }

    private static void CreatedRectangleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not EditableImage control) return;
        if (e.NewValue is not Rectangle newRectangle) return;

        // 矩形再作成中は処理をスキップ（RecreateRectangleFromCropRangeで既に処理済み）
        if (control.isRecreatingRectangle) return;

        control.SetRectangleEventHandlers(newRectangle);

        control.UIElements.Clear();
        control.UIElements.Add(newRectangle);

        // 選択インデックスを更新し、SelectedRectangleが正しく新しい矩形を指すようにする
        control.SelectedRectangleIndex = 0;
        control.UpdateSelectedRectangle();

        control.ReSetResizeHandles();
    }

    #endregion

    #region プロパティ

    /// <summary>
    /// 矩形の最小サイズ（枠線の太さの3倍）
    /// </summary>
    private double RectangleMinSize => EdgeThickness * 3;

    /// <summary>
    /// オーバーレイキャンバス上のUI要素コレクション
    /// </summary>
    public UIElementCollection UIElements => overlayCanvas.Children;

    #endregion

    #region フィールド - ズーム・パン操作

    private Point? lastMousePosition_Right = null;
    private bool isDragging_Right = false;

    #endregion

    #region フィールド - 矩形編集操作

    private Rectangle? draggedRectangle = null;
    private Point? lastMousePosition_Left = null;
    private bool isDragging_Left = false;

    private readonly List<Ellipse> resizeHandles = new(8);
    private int selectedResizeHandleIndex = -1;
    private bool IsResizing = false;

    private bool isMakingRectangle = false;
    private Point? makingStartPoint = null;
    private Rectangle? creatingRectangle = null;

    /// <summary>
    /// 現在開いている手動設定ウィンドウ
    /// </summary>
    private CropRangeManualSettingsWindow? currentManualSettingsWindow = null;

    /// <summary>
    /// 矩形再作成中フラグ（CreatedRectangleChangedでの重複処理を防ぐ）
    /// </summary>
    private bool isRecreatingRectangle = false;

    #endregion

    #region イベント

    /// <summary>
    /// 矩形の座標が変更されたときに発生するイベント
    /// </summary>
    public event EventHandler<RectRange>? RectangleCoordinatesChanged;

    /// <summary>
    /// 矩形座標変更イベントを発火
    /// </summary>
    private void RaiseRectangleCoordinatesChanged(Rectangle rectangle)
    {
        var (leftTop, rightBottom) = GetRectangleCoordinates(rectangle);
        var range = new RectRange(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
        RectangleCoordinatesChanged?.Invoke(this, range);
    }

    #endregion

    #region コンストラクタ・初期化

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public EditableImage()
    {
        InitializeComponent();
        SetupKeyboardHandlers();
    }

    /// <summary>
    /// キーボードイベントハンドラの設定
    /// </summary>
    private void SetupKeyboardHandlers()
    {
        this.PreviewKeyDown += (sender, e) =>
        {
            if (IsManualSettingsWindowOpen) return; // 手動設定ウィンドウが開いている場合は無視

            if (e.Key == Key.Delete && SelectedRectangleIndex >= 0)
            {
                RemoveSelectedRectangle();
                e.Handled = true;
            }
        };
        this.Focusable = true;
        this.MouseDown += (sender, e) => this.Focus();
    }

    #endregion

    #region パブリックメソッド - 矩形管理

    /// <summary>
    /// すべての矩形をクリア
    /// </summary>
    public void ClearRectangles() => UIElements.Clear();

    /// <summary>
    /// 指定した枠線色の矩形をクリア
    /// </summary>
    /// <param name="brush">削除対象の枠線色</param>
    public void ClearRectangles(Brush brush)
    {
        UIElement[] clone = new UIElement[UIElements.Count];
        UIElements.CopyTo(clone, 0);
        foreach (UIElement element in clone)
        {
            if (element is Rectangle rectangle && rectangle.Stroke == brush)
            {
                UIElements.Remove(rectangle);
            }
        }
    }

    /// <summary>
    /// 選択中の矩形を削除
    /// </summary>
    internal void RemoveSelectedRectangle()
    {
        if (SelectedRectangleIndex >= 0 && SelectedRectangleIndex < UIElements.Count && UIElements[SelectedRectangleIndex] is Rectangle rect)
        {
            ClearHandles();
            UIElements.Remove(rect);
            CreatedRectangle = null;
            CropRange = null;
            SelectedRectangleIndex = -1;
        }
    }

    #endregion

    #region ズーム・パン機能

    /// <summary>
    /// ズーム処理を適用
    /// </summary>
    /// <param name="center">ズームの中心点</param>
    /// <param name="newScaleX">新しいX軸スケール</param>
    /// <param name="newScaleY">新しいY軸スケール</param>
    private void ApplyZoom(Point center, double newScaleX, double newScaleY)
    {
        // スケールを範囲内に制限
        newScaleX = Math.Max(MinZoom, Math.Min(MaxZoom, newScaleX));
        newScaleY = Math.Max(MinZoom, Math.Min(MaxZoom, newScaleY));

        double previousScaleX = scaleTransform.ScaleX;
        double previousScaleY = scaleTransform.ScaleY;

        scaleTransform.ScaleX = newScaleX;
        scaleTransform.ScaleY = newScaleY;

        // スクロール位置を調整してズーム中心を保持
        if (scrollViewer != null)
        {
            double deltaX = center.X * (newScaleX - previousScaleX);
            double deltaY = center.Y * (newScaleY - previousScaleY);
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + deltaX);
            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + deltaY);
        }
    }

    /// <summary>
    /// マウスホイールによるズームとスクロール処理
    /// </summary>
    private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // Ctrlキー押下時：ズーム
            Point mousePosition = e.GetPosition(image);
            double newScale = e.Delta > 0 ? scaleTransform.ScaleX * ZoomFactor : scaleTransform.ScaleX / ZoomFactor;
            ApplyZoom(mousePosition, newScale, newScale);

            // 選択中の矩形のリサイズハンドルを更新
            if (SelectedRectangleIndex != -1)
            {
                ClearHandles();
                SetResizeHandles((Rectangle)UIElements[SelectedRectangleIndex]);
            }
            e.Handled = true;
        }
        else if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
        {
            // Shiftキー押下時：横スクロール
            scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - (e.Delta / 2));
            e.Handled = true;
        }
    }

    /// <summary>
    /// 右クリックドラッグによるパン開始
    /// </summary>
    private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (scrollViewer != null)
        {
            _ = image.CaptureMouse();
            lastMousePosition_Right = e.GetPosition(scrollViewer);
            isDragging_Right = true;
            e.Handled = true;
        }
    }

    /// <summary>
    /// 右クリックドラッグによるパン終了
    /// </summary>
    private void Image_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isDragging_Right)
        {
            image.ReleaseMouseCapture();
            lastMousePosition_Right = null;
            isDragging_Right = false;
            e.Handled = true;
        }
    }

    #endregion

    #region マウス移動処理

    /// <summary>
    /// 画像上でのマウス移動処理（パン・矩形作成・リサイズ・ドラッグ）
    /// </summary>
    private void Image_MouseMove(object sender, MouseEventArgs e)
    {
        // 右クリックドラッグによるパン処理
        if (isDragging_Right && lastMousePosition_Right.HasValue && scrollViewer != null)
        {
            HandlePanDrag(e);
            return;
        }

        OnImageMouseMove(sender, e);
    }

    /// <summary>
    /// パンドラッグ処理
    /// </summary>
    private void HandlePanDrag(MouseEventArgs e)
    {
        // lastMousePosition_Right.HasValue == true は呼び出し元でチェック済み
        Point currentPosition = e.GetPosition(scrollViewer);
        Vector delta = lastMousePosition_Right!.Value - currentPosition;
        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + delta.X);
        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + delta.Y);
        lastMousePosition_Right = currentPosition;
    }

    /// <summary>
    /// 画像上でのマウス移動処理（矩形操作）
    /// </summary>
    private void OnImageMouseMove(object sender, MouseEventArgs e)
    {
        if (isMakingRectangle)
        {
            HandleRectangleCreation(e);
        }
        else if (IsResizing)
        {
            HandleRectangleResize(e);
        }
        else if (isDragging_Left)
        {
            HandleRectangleDrag(e);
        }
    }

    #endregion

    #region 矩形作成処理

    /// <summary>
    /// 画像上での左クリックで矩形作成開始
    /// </summary>
    private void OnImageMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        SelectedRectangleIndex = -1;
        ClearHandles();

        if (IsManualSettingsWindowOpen) return; // 手動設定ウィンドウが開いている場合は無視

        isMakingRectangle = true;
        makingStartPoint = e.GetPosition(image);
        overlayCanvas.IsHitTestVisible = false;
    }

    /// <summary>
    /// 矩形作成中の処理
    /// </summary>
    private void HandleRectangleCreation(MouseEventArgs e)
    {
        if (makingStartPoint is null) return;

        Point currentPosition = e.GetPosition(image);

        // 座標を画像範囲内に制限
        double maxW = image.ActualWidth;
        double maxH = image.ActualHeight;
        currentPosition.X = Math.Max(0, Math.Min(maxW, currentPosition.X));
        currentPosition.Y = Math.Max(0, Math.Min(maxH, currentPosition.Y));

        var distance = (currentPosition - makingStartPoint.Value).Length;

        // 最小サイズに達していない場合は処理しない
        if (distance <= RectangleMinSize)
        {
            rangeDisplayBorder.Visibility = Visibility.Collapsed;
            return;
        }

        // 既存の矩形を削除（1つのみ保持）
        RemoveExistingRectangleExceptCreating();

        // 矩形の範囲を計算（開始点も制限済み、現在位置も制限済み）
        Point start = makingStartPoint.Value;
        Point leftTop = new(Math.Min(start.X, currentPosition.X), Math.Min(start.Y, currentPosition.Y));
        Point rightBottom = new(Math.Max(start.X, currentPosition.X), Math.Max(start.Y, currentPosition.Y));

        double width = rightBottom.X - leftTop.X;
        double height = rightBottom.Y - leftTop.Y;

        // 最小サイズチェック
        if (width < RectangleMinSize || height < RectangleMinSize)
        {
            rangeDisplayBorder.Visibility = Visibility.Collapsed;
            return;
        }

        // 矩形の作成または更新
        if (creatingRectangle is null)
        {
            AddNewRectangle(leftTop, width, height);
        }
        else
        {
            UpdateCreatingRectangle(leftTop, width, height);
        }

        if (creatingRectangle is not null)
        {
            UpdateRangeDisplay(creatingRectangle);
        }
    }

    /// <summary>
    /// 既存の矩形を削除（作成中の矩形以外）
    /// </summary>
    private void RemoveExistingRectangleExceptCreating()
    {
        Rectangle? existingRectangle = UIElements.OfType<Rectangle>().FirstOrDefault();
        if (existingRectangle != null && existingRectangle != creatingRectangle)
        {
            UIElements.Remove(existingRectangle);
        }
    }

    public Rectangle CreateNewRectangle(Point leftTop, double width, double height)
    {
        return new Rectangle
        {
            Stroke = NewRectangleStroke,
            Fill = NewRectangleFill,
            StrokeThickness = EdgeThickness,
            Tag = LabelClass,
            RenderTransform = new TranslateTransform(leftTop.X, leftTop.Y),
            Width = width,
            Height = height,
            Cursor = Cursors.Hand
        };
    }

    /// <summary>
    /// 新しい矩形を作成
    /// </summary>
    private void AddNewRectangle(Point leftTop, double width, double height)
    {
        creatingRectangle = CreateNewRectangle(leftTop, width, height);
        _ = UIElements.Add(creatingRectangle);
    }

    /// <summary>
    /// 作成中の矩形を更新
    /// </summary>
    private void UpdateCreatingRectangle(Point leftTop, double width, double height)
    {
        // creatingRectangle is not null は呼び出し元でチェック済み
        if (creatingRectangle!.RenderTransform is TranslateTransform t)
        {
            t.X = leftTop.X;
            t.Y = leftTop.Y;
        }
        creatingRectangle.Width = width;
        creatingRectangle.Height = height;
    }

    #endregion

    #region 矩形リサイズ処理

    /// <summary>
    /// リサイズハンドルのマウスダウン処理
    /// </summary>
    private void Ellipse_MouseDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Ellipse ellipse) return;
        if (isMakingRectangle) return;

        if (scrollViewer != null)
        {
            _ = image.CaptureMouse();
            ClearHandles();
            Mouse.OverrideCursor = Cursors.Cross;
            IsResizing = true;
            selectedResizeHandleIndex = (int)ellipse.Tag;
            // リサイズ開始時の基準位置を保存
            lastMousePosition_Left = e.GetPosition(image);
            e.Handled = true;
        }
    }

    /// <summary>
    /// 矩形リサイズ中の処理
    /// </summary>
    private void HandleRectangleResize(MouseEventArgs e)
    {
        if (SelectedRectangle is null) return;
        if (lastMousePosition_Left is null) return;

        Point currentPosition = e.GetPosition(image);

        // 画像範囲内に制限
        double imageWidth = image.ActualWidth;
        double imageHeight = image.ActualHeight;
        currentPosition.X = Math.Max(0, Math.Min(imageWidth, currentPosition.X));
        currentPosition.Y = Math.Max(0, Math.Min(imageHeight, currentPosition.Y));

        // リサイズ処理は表示座標で行う
        (Point leftTop, Point rightBottom) = GetRectangleDisplayCoordinates(SelectedRectangle);

        // リサイズハンドルの位置に応じて矩形を変形
        (leftTop, rightBottom) = CalculateResizedBounds(leftTop, rightBottom, currentPosition);

        // 画像範囲内に制限
        leftTop.X = Math.Max(0, leftTop.X);
        leftTop.Y = Math.Max(0, leftTop.Y);
        rightBottom.X = Math.Min(imageWidth, rightBottom.X);
        rightBottom.Y = Math.Min(imageHeight, rightBottom.Y);

        // 矩形を更新
        UpdateRectangleGeometry(SelectedRectangle, leftTop, rightBottom);

        UpdateRangeDisplay(SelectedRectangle);
    }

    /// <summary>
    /// リサイズ後の矩形範囲を計算
    /// </summary>
    private (Point leftTop, Point rightBottom) CalculateResizedBounds(Point leftTop, Point rightBottom, Point currentPosition)
    {
        double delta = EdgeThickness * 2;
        double minSize = RectangleMinSize;

        switch (selectedResizeHandleIndex)
        {
            case 0: // 左上
                (leftTop, rightBottom) = ResizeTopLeft(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 1: // 上
                (leftTop, rightBottom) = ResizeTop(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 2: // 右上
                (leftTop, rightBottom) = ResizeTopRight(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 3: // 右
                (leftTop, rightBottom) = ResizeRight(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 4: // 右下
                (leftTop, rightBottom) = ResizeBottomRight(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 5: // 下
                (leftTop, rightBottom) = ResizeBottom(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 6: // 左下
                (leftTop, rightBottom) = ResizeBottomLeft(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
            case 7: // 左
                (leftTop, rightBottom) = ResizeLeft(leftTop, rightBottom, currentPosition, minSize, delta);
                break;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 左上ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeTopLeft(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.X < rightBottom.X - minSize) leftTop.X = current.X; else leftTop.X = rightBottom.X - minSize;
        if (current.Y < rightBottom.Y - minSize) leftTop.Y = current.Y; else leftTop.Y = rightBottom.Y - minSize;

        // 反転チェック
        if (current.X - minSize > rightBottom.X)
        {
            (leftTop.X, rightBottom.X) = (rightBottom.X + delta, leftTop.X + delta);
            selectedResizeHandleIndex = 2;
        }
        if (current.Y - minSize > rightBottom.Y)
        {
            (leftTop.Y, rightBottom.Y) = (rightBottom.Y + delta, leftTop.Y + delta);
            selectedResizeHandleIndex = 6;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 上ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeTop(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.Y < rightBottom.Y - minSize) leftTop.Y = current.Y; else leftTop.Y = rightBottom.Y - minSize;

        if (current.Y - minSize > rightBottom.Y)
        {
            (leftTop.Y, rightBottom.Y) = (rightBottom.Y + delta, leftTop.Y + delta);
            selectedResizeHandleIndex = 5;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 右上ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeTopRight(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.X > leftTop.X + minSize) rightBottom.X = current.X; else rightBottom.X = leftTop.X + minSize;
        if (current.Y < rightBottom.Y - minSize) leftTop.Y = current.Y; else leftTop.Y = rightBottom.Y - minSize;

        if (current.X < leftTop.X - minSize)
        {
            (leftTop.X, rightBottom.X) = (rightBottom.X - delta, leftTop.X - delta);
            selectedResizeHandleIndex = 0;
        }
        if (current.Y - minSize > rightBottom.Y)
        {
            (leftTop.Y, rightBottom.Y) = (rightBottom.Y + delta, leftTop.Y + delta);
            selectedResizeHandleIndex = 4;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 右ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeRight(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.X > leftTop.X + minSize) rightBottom.X = current.X; else rightBottom.X = leftTop.X + minSize;

        if (current.X < leftTop.X - minSize)
        {
            (leftTop.X, rightBottom.X) = (rightBottom.X - delta, leftTop.X - delta);
            selectedResizeHandleIndex = 7;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 右下ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeBottomRight(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.X > leftTop.X + minSize) rightBottom.X = current.X; else rightBottom.X = leftTop.X + minSize;
        if (current.Y > leftTop.Y + minSize) rightBottom.Y = current.Y; else rightBottom.Y = leftTop.Y + minSize;

        if (current.X < leftTop.X - minSize)
        {
            (leftTop.X, rightBottom.X) = (rightBottom.X - delta, leftTop.X - delta);
            selectedResizeHandleIndex = 6;
        }
        if (current.Y < leftTop.Y - minSize)
        {
            (leftTop.Y, rightBottom.Y) = (rightBottom.Y - delta, leftTop.Y - delta);
            selectedResizeHandleIndex = 2;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 下ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeBottom(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.Y > leftTop.Y + minSize) rightBottom.Y = current.Y; else rightBottom.Y = leftTop.Y + minSize;

        if (current.Y < leftTop.Y - minSize)
        {
            (leftTop.Y, rightBottom.Y) = (rightBottom.Y - delta, leftTop.Y - delta);
            selectedResizeHandleIndex = 1;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 左下ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeBottomLeft(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.X < rightBottom.X - minSize) leftTop.X = current.X; else leftTop.X = rightBottom.X - minSize;
        if (current.Y > leftTop.Y + minSize) rightBottom.Y = current.Y; else rightBottom.Y = leftTop.Y + minSize;

        if (current.X - minSize > rightBottom.X)
        {
            (leftTop.X, rightBottom.X) = (rightBottom.X + delta, leftTop.X + delta);
            selectedResizeHandleIndex = 4;
        }
        if (current.Y < leftTop.Y - minSize)
        {
            (leftTop.Y, rightBottom.Y) = (rightBottom.Y - delta, leftTop.Y - delta);
            selectedResizeHandleIndex = 0;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 左ハンドルでのリサイズ
    /// </summary>
    private (Point leftTop, Point rightBottom) ResizeLeft(Point leftTop, Point rightBottom, Point current, double minSize, double delta)
    {
        if (current.X < rightBottom.X - minSize) leftTop.X = current.X; else leftTop.X = rightBottom.X - minSize;

        if (current.X - minSize > rightBottom.X)
        {
            (leftTop.X, rightBottom.X) = (rightBottom.X + delta, leftTop.X + delta);
            selectedResizeHandleIndex = 3;
        }

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 矩形のジオメトリを更新
    /// </summary>
    private void UpdateRectangleGeometry(Rectangle rectangle, Point leftTop, Point rightBottom)
    {
        double width = Math.Abs(rightBottom.X - leftTop.X);
        double height = Math.Abs(rightBottom.Y - leftTop.Y);

        if (rectangle.RenderTransform is TranslateTransform tt)
        {
            tt.X = Math.Min(leftTop.X, rightBottom.X);
            tt.Y = Math.Min(leftTop.Y, rightBottom.Y);
        }

        rectangle.Width = width;
        rectangle.Height = height;
    }

    #endregion

    #region 矩形ドラッグ処理

    /// <summary>
    /// 矩形上での左クリックでドラッグ開始
    /// </summary>
    private void OnRectangleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Rectangle rectangle) return;
        if (isMakingRectangle) return;

        bool isEdge = IsRectangleEdge(rectangle, e);
        if (!isEdge)
        {
            _ = image.CaptureMouse();
            ClearHandles();
            lastMousePosition_Left = e.GetPosition(scrollViewer);
            draggedRectangle = rectangle;
            isDragging_Left = true;
            SelectedRectangleIndex = UIElements.IndexOf(rectangle);
            e.Handled = true;
        }
    }

    /// <summary>
    /// 矩形ドラッグ中の処理
    /// </summary>
    private void HandleRectangleDrag(MouseEventArgs e)
    {
        if (lastMousePosition_Left is null) return;
        if (draggedRectangle is not Rectangle rect) return;
        if (rect.RenderTransform is not TranslateTransform t) return;

        Point currentPosition = e.GetPosition(scrollViewer);
        Vector deltaVec = currentPosition - lastMousePosition_Left.Value;

        // スケールを考慮した移動量を計算
        double deltaX = deltaVec.X / scaleTransform.ScaleX;
        double deltaY = deltaVec.Y / scaleTransform.ScaleY;

        // 新しい位置を計算
        double newX = t.X + deltaX;
        double newY = t.Y + deltaY;

        // 画像範囲内に制限
        double imageWidth = image.ActualWidth;
        double imageHeight = image.ActualHeight;

        newX = Math.Max(0, Math.Min(imageWidth - rect.Width, newX));
        newY = Math.Max(0, Math.Min(imageHeight - rect.Height, newY));

        // 実際に移動した量を計算
        double actualDeltaX = newX - t.X;
        double actualDeltaY = newY - t.Y;

        // 矩形を移動
        t.X = newX;
        t.Y = newY;

        // 次回の基準位置を、実際に移動した量を考慮して更新
        // これにより、マウスが画像外に出ても矩形との位置関係が保たれる
        lastMousePosition_Left = new Point(
            lastMousePosition_Left.Value.X + actualDeltaX * scaleTransform.ScaleX,
            lastMousePosition_Left.Value.Y + actualDeltaY * scaleTransform.ScaleY
        );

        UpdateRangeDisplay(rect);

        e.Handled = true;
    }

    /// <summary>
    /// クリック位置が矩形の枠線上かどうかを判定
    /// </summary>
    private bool IsRectangleEdge(Rectangle rect, MouseButtonEventArgs e)
    {
        Point clickedPoint = e.GetPosition(rect);
        double width = rect.Width;
        double height = rect.Height;

        Point internalLeftTop = new(EdgeThickness, EdgeThickness);
        Point internalRightBottom = new(width - EdgeThickness, height - EdgeThickness);

        bool isInternal = clickedPoint.X > internalLeftTop.X && clickedPoint.X < internalRightBottom.X &&
                          clickedPoint.Y > internalLeftTop.Y && clickedPoint.Y < internalRightBottom.Y;

        return !isInternal;
    }

    #endregion

    #region ドラッグ終了処理

    /// <summary>
    /// 左クリック解放処理
    /// </summary>
    private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) => ReleaseDrag(e);

    /// <summary>
    /// ドラッグ操作の終了処理
    /// </summary>
    private void ReleaseDrag(MouseEventArgs? e)
    {
        image.ReleaseMouseCapture();

        rangeDisplayBorder.Visibility = Visibility.Collapsed;

        // 矩形作成終了
        if (isMakingRectangle)
        {
            FinishRectangleCreation();
        }

        // リサイズ終了
        if (IsResizing)
        {
            FinishRectangleResize();
        }

        // ドラッグ終了
        if (isDragging_Left)
        {
            FinishRectangleDrag();
        }
    }

    /// <summary>
    /// 矩形作成の終了処理
    /// </summary>
    private void FinishRectangleCreation()
    {
        if (creatingRectangle is not null)
        {
            ClampRectangle(creatingRectangle);
            SetRectangleEventHandlers(creatingRectangle);
            SelectedRectangleIndex = UIElements.IndexOf(creatingRectangle);
            CreatedRectangle = creatingRectangle;
            ReSetResizeHandles();
            UpdateCropRangeFromRectangle(creatingRectangle);
            creatingRectangle = null;
        }

        overlayCanvas.IsHitTestVisible = true;
        isMakingRectangle = false;
        makingStartPoint = null;
    }

    /// <summary>
    /// リサイズの終了処理
    /// </summary>
    private void FinishRectangleResize()
    {
        IsResizing = false;
        selectedResizeHandleIndex = -1;
        Mouse.OverrideCursor = null;
        ReSetResizeHandles();
        CreatedRectangle = SelectedRectangle;

        // 座標変更イベントを発火
        if (SelectedRectangle is not null)
        {
            RaiseRectangleCoordinatesChanged(SelectedRectangle);
            UpdateCropRangeFromRectangle(SelectedRectangle);
        }
    }

    /// <summary>
    /// ドラッグの終了処理
    /// </summary>
    private void FinishRectangleDrag()
    {
        var movedRectangle = draggedRectangle;

        if (draggedRectangle is not null)
            ClampRectangle(draggedRectangle);

        ReSetResizeHandles();
        lastMousePosition_Left = null;
        draggedRectangle = null;
        isDragging_Left = false;
        CreatedRectangle = SelectedRectangle;

        // 座標変更イベントを発火
        if (movedRectangle is not null)
        {
            RaiseRectangleCoordinatesChanged(movedRectangle);
            UpdateCropRangeFromRectangle(movedRectangle);
        }
    }

    #endregion

    #region リサイズハンドル管理

    /// <summary>
    /// リサイズハンドルを設定
    /// </summary>
    private void SetResizeHandles(Rectangle rect)
    {
        if (rect.RenderTransform is not TranslateTransform transform)
            throw new InvalidOperationException("Transform is not TranslateTransform");

        double width = rect.Width;
        double height = rect.Height;
        double left = transform.X;
        double top = transform.Y;

        // ハンドル位置を計算
        TranslateTransform[] handlePositions = CalculateHandlePositions(left, top, width, height);

        // カーソルの種類
        Cursor[] cursors = [Cursors.SizeNWSE, Cursors.SizeNS, Cursors.SizeNESW, Cursors.SizeWE];

        // ハンドルを作成
        for (int i = 0; i < 8; i++)
        {
            Ellipse ellipse = CreateResizeHandle(handlePositions[i], cursors[i % 4], i);
            resizeHandles.Add(ellipse);
            _ = UIElements.Add(ellipse);
        }
    }

    /// <summary>
    /// リサイズハンドルの位置を計算
    /// </summary>
    private TranslateTransform[] CalculateHandlePositions(double left, double top, double width, double height)
    {
        TranslateTransform[] points = new TranslateTransform[8];
        for (int i = 0; i < 8; i++)
            points[i] = new(left, top);

        double normalizedSizeX = EllipseSize / scaleTransform.ScaleX;
        double normalizedSizeY = EllipseSize / scaleTransform.ScaleY;

        // 各ハンドルの位置を設定
        points[0] = Add(points[0], new((EdgeThickness - normalizedSizeX) / 2, (EdgeThickness - normalizedSizeY) / 2));
        points[1] = Add(points[1], new((width - normalizedSizeX) / 2, (EdgeThickness - normalizedSizeY) / 2));
        points[2] = Add(points[2], new(width - ((EdgeThickness + normalizedSizeX) / 2), (EdgeThickness - normalizedSizeY) / 2));
        points[3] = Add(points[3], new(width - ((EdgeThickness + normalizedSizeX) / 2), (height - normalizedSizeY) / 2));
        points[4] = Add(points[4], new(width - ((EdgeThickness + normalizedSizeX) / 2), height - ((EdgeThickness + normalizedSizeY) / 2)));
        points[5] = Add(points[5], new((width - normalizedSizeX) / 2, height - ((EdgeThickness + normalizedSizeY) / 2)));
        points[6] = Add(points[6], new((EdgeThickness - normalizedSizeX) / 2, height - ((EdgeThickness + normalizedSizeY) / 2)));
        points[7] = Add(points[7], new((EdgeThickness - normalizedSizeX) / 2, (height - normalizedSizeY) / 2));

        return points;
    }

    /// <summary>
    /// リサイズハンドルを作成
    /// </summary>
    private Ellipse CreateResizeHandle(TranslateTransform position, Cursor cursor, int index)
    {
        double normalizedSizeX = EllipseSize / scaleTransform.ScaleX;
        double normalizedSizeY = EllipseSize / scaleTransform.ScaleY;

        Ellipse ellipse = new()
        {
            Width = normalizedSizeX,
            Height = normalizedSizeY,
            Fill = new SolidColorBrush(Color.FromArgb(200, 255, 255, 255)),
            RenderTransform = position,
            Cursor = cursor,
            Tag = index,
        };

        ellipse.MouseDown += Ellipse_MouseDown;
        return ellipse;
    }

    /// <summary>
    /// リサイズハンドルを再設定
    /// </summary>
    private void ReSetResizeHandles()
    {
        if (SelectedRectangleIndex == -1) return;
        if (SelectedRectangle is not Rectangle rectangle) return;

        ClearHandles();
        SetResizeHandles(rectangle);
    }

    /// <summary>
    /// リサイズハンドルをクリア
    /// </summary>
    private void ClearHandles()
    {
        foreach (Ellipse handle in resizeHandles)
            overlayCanvas.Children.Remove(handle);
        resizeHandles.Clear();
    }

    #endregion

    #region ヘルパーメソッド

    /// <summary>
    /// 選択矩形の更新
    /// </summary>
    private void UpdateSelectedRectangle()
    {
        if (SelectedRectangleIndex == -1 || UIElements == null || SelectedRectangleIndex >= UIElements.Count)
        {
            SelectedRectangle = null;
            return;
        }

        SelectedRectangle = UIElements[SelectedRectangleIndex] is Rectangle rectangle ? rectangle : null;
    }

    /// <summary>
    /// 矩形にイベントハンドラを設定
    /// </summary>
    private void SetRectangleEventHandlers(Rectangle rectangle)
    {
        rectangle.MouseLeftButtonDown += OnRectangleMouseLeftButtonDown;
        rectangle.MouseRightButtonDown += OnRectangleMouseRightButtonDown;
        rectangle.MouseWheel += Image_MouseWheel;
    }

    /// <summary>
    /// 矩形上での右クリックでCropRangeManualSettingsWindowを表示
    /// </summary>
    private async void OnRectangleMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (IsManualSettingsWindowOpen) return; // 既に手動設定ウィンドウが開いている場合は無視
        if (sender is not Rectangle rectangle) return;
        if (ImageInformation is null) return;

        e.Handled = true;

        IsManualSettingsWindowOpen = true;
        try
        {
            var result = await CropRangeManualSettingsWindow.ShowWindowAsync(
                ImageInformation,
                rectangle,
                this,
                window => currentManualSettingsWindow = window);

            if (result is not null)
            {
                CreatedRectangle = result;
            }
        }
        finally
        {
            currentManualSettingsWindow = null;
            IsManualSettingsWindowOpen = false;
        }
    }

    /// <summary>
    /// 手動設定ウィンドウが開いている場合は閉じる
    /// </summary>
    public void CloseManualSettingsWindow()
    {
        currentManualSettingsWindow?.Close();
    }

    /// <summary>
    /// 切り抜き範囲の表示を更新
    /// </summary>
    private void UpdateRangeDisplay(Rectangle rectangle)
    {
        if (UISettings == null || ImageInformation == null)
        {
            rangeDisplayBorder.Visibility = Visibility.Collapsed;
            return;
        }

        var (leftTop, rightBottom) = GetRectangleCoordinates(rectangle);
        var (displayLeftTop, displayRightBottom) = GetRectangleDisplayCoordinates(rectangle);

        double width = rightBottom.X - leftTop.X;
        double height = rightBottom.Y - leftTop.Y;

        string text = "";
        switch (UISettings.RangeDisplayMode)
        {
            case RangeDisplayMode.XYWH_Pixel:
                text = $"X:{leftTop.X:F0} Y:{leftTop.Y:F0} W:{width:F0} H:{height:F0}";
                break;
            case RangeDisplayMode.XYWH_Percent:
                text = $"X:{leftTop.X / ImageInformation.Width * 100:F1}% Y:{leftTop.Y / ImageInformation.Height * 100:F1}% W:{width / ImageInformation.Width * 100:F1}% H:{height / ImageInformation.Height * 100:F1}%";
                break;
            case RangeDisplayMode.X1Y1X2Y2_Pixel:
                text = $"X1:{leftTop.X:F0} Y1:{leftTop.Y:F0} X2:{rightBottom.X:F0} Y2:{rightBottom.Y:F0}";
                break;
            case RangeDisplayMode.X1Y1X2Y2_Percent:
                text = $"X1:{leftTop.X / ImageInformation.Width * 100:F1}% Y1:{leftTop.Y / ImageInformation.Height * 100:F1}% X2:{rightBottom.X / ImageInformation.Width * 100:F1}% Y2:{rightBottom.Y / ImageInformation.Height * 100:F1}%";
                break;
        }

        rangeDisplayTextBlock.Text = text;

        // 表示位置の計算
        rangeDisplayBorder.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        double borderWidth = rangeDisplayBorder.DesiredSize.Width;
        double borderHeight = rangeDisplayBorder.DesiredSize.Height;

        // 現在のズーム倍率を考慮
        double scaleX = scaleTransform.ScaleX;
        double scaleY = scaleTransform.ScaleY;

        // 表示場所は矩形の左上。ただし、矩形が左上に近くて表示できない場合は、矩形の右下に表示する
        double margin = 5;
        double x = displayLeftTop.X * scaleX;
        double y = displayLeftTop.Y * scaleY - borderHeight - margin;

        if (y < 0)
        {
            // 上にはみ出す場合は、矩形の右下に表示
            y = displayRightBottom.Y * scaleY + margin;
            x = displayRightBottom.X * scaleX - borderWidth;
        }
        else
        {
            // 右側にはみ出す場合の調整
            if (x + borderWidth > image.ActualWidth * scaleX)
            {
                x = image.ActualWidth * scaleX - borderWidth;
            }
        }

        Canvas.SetLeft(rangeDisplayBorder, Math.Max(0, x));
        Canvas.SetTop(rangeDisplayBorder, Math.Max(0, y));
        rangeDisplayBorder.Visibility = Visibility.Visible;
    }

    /// <summary>
    /// 表示サイズと元画像サイズの比率を取得（X, Y）
    /// 元画像座標 = 表示座標 × 比率
    /// </summary>
    private (double scaleX, double scaleY) GetScaleRatio()
    {
        if (ImageInformation is null || image is null)
            return (1.0, 1.0);

        double displayWidth = image.ActualWidth;
        double displayHeight = image.ActualHeight;

        if (displayWidth <= 0 || displayHeight <= 0)
            return (1.0, 1.0);

        double scaleX = (double)ImageInformation.Width / displayWidth;
        double scaleY = (double)ImageInformation.Height / displayHeight;

        return (scaleX, scaleY);
    }

    /// <summary>
    /// 矩形の表示座標を取得（左上と右下）- スケーリングなし
    /// </summary>
    private static (Point leftTop, Point rightBottom) GetRectangleDisplayCoordinates(Rectangle rectangle)
    {
        if (rectangle.RenderTransform is not TranslateTransform transform)
            throw new InvalidOperationException("Transform is not TranslateTransform");

        double left = transform.X;
        double top = transform.Y;
        double width = rectangle.Width;
        double height = rectangle.Height;

        return (new Point(left, top), new Point(left + width, top + height));
    }

    /// <summary>
    /// 矩形の座標を元画像の座標系で取得（左上と右下）
    /// 表示座標から元画像座標への変換を行う
    /// </summary>
    public (Point leftTop, Point rightBottom) GetRectangleCoordinates(Rectangle rectangle)
    {
        var (displayLeftTop, displayRightBottom) = GetRectangleDisplayCoordinates(rectangle);
        var (scaleX, scaleY) = GetScaleRatio();

        var leftTop = new Point(displayLeftTop.X * scaleX, displayLeftTop.Y * scaleY);
        var rightBottom = new Point(displayRightBottom.X * scaleX, displayRightBottom.Y * scaleY);

        return (leftTop, rightBottom);
    }

    /// <summary>
    /// 元画像座標から矩形を作成
    /// 元画像座標から表示座標への変換を行う
    /// </summary>
    public Rectangle CreateRectangleFromCoordinates(RectRange range)
    {
        var (scaleX, scaleY) = GetScaleRatio();

        // 元画像座標を表示座標に変換
        double left = Math.Min(range.X1, range.X2) / scaleX;
        double top = Math.Min(range.Y1, range.Y2) / scaleY;
        double width = Math.Abs(range.X2 - range.X1) / scaleX;
        double height = Math.Abs(range.Y2 - range.Y1) / scaleY;

        return CreateNewRectangle(new Point(left, top), width, height);
    }

    /// <summary>
    /// 矩形を画像範囲内に制限
    /// </summary>
    private void ClampRectangle(Rectangle rectangle)
    {
        if (image is null || rectangle.RenderTransform is not TranslateTransform t) return;

        double imageWidth = image.ActualWidth;
        double imageHeight = image.ActualHeight;

        if (imageWidth <= 0 || imageHeight <= 0) return;

        // 位置を制限
        if (t.X < 0) t.X = 0;
        if (t.Y < 0) t.Y = 0;

        // サイズを制限
        if (rectangle.Width > imageWidth)
        {
            rectangle.Width = imageWidth;
            t.X = 0;
        }
        if (rectangle.Height > imageHeight)
        {
            rectangle.Height = imageHeight;
            t.Y = 0;
        }

        // 右端・下端を制限
        if (t.X + rectangle.Width > imageWidth)
            t.X = imageWidth - rectangle.Width;
        if (t.Y + rectangle.Height > imageHeight)
            t.Y = imageHeight - rectangle.Height;
    }

    /// <summary>
    /// TranslateTransformの加算
    /// </summary>
    private static TranslateTransform Add(TranslateTransform a, TranslateTransform b)
        => new(a.X + b.X, a.Y + b.Y);

    internal void UpdateRectangle(RectRange range)
    {
        var rectangle = CreateRectangleFromCoordinates(range);
        CreatedRectangle = rectangle;
        UpdateCropRangeFromRectangle(rectangle);
    }

    /// <summary>
    /// 矩形からピクセル座標を取得してCropRangeプロパティを更新
    /// </summary>
    private void UpdateCropRangeFromRectangle(Rectangle rectangle)
    {
        var (leftTop, rightBottom) = GetRectangleCoordinates(rectangle);
        CropRange = new RectRange(leftTop.X, leftTop.Y, rightBottom.X, rightBottom.Y);
    }

    /// <summary>
    /// CropRange（ピクセル座標）から矩形を再作成
    /// </summary>
    private void RecreateRectangleFromCropRange()
    {
        // 既存の矩形とハンドルをクリア
        ClearRectangles();
        ClearHandles();
        SelectedRectangleIndex = -1;

        if (CropRange is null) return;
        if (ImageInformation is null) return;
        if (image.ActualWidth <= 0 || image.ActualHeight <= 0) return;

        try
        {
            isRecreatingRectangle = true;

            // ピクセル座標から矩形を作成
            var rectangle = CreateRectangleFromCoordinates(CropRange);
            SetRectangleEventHandlers(rectangle);
            UIElements.Add(rectangle);
            SelectedRectangleIndex = UIElements.IndexOf(rectangle);
            CreatedRectangle = rectangle;
            ReSetResizeHandles();
        }
        finally
        {
            isRecreatingRectangle = false;
        }
    }

    #endregion
}