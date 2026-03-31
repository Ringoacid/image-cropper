using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace ImageCropper.Views.UserControls;


/// <summary>
/// ToggleSwitch.xaml の相互作用ロジック
/// </summary>
public partial class ToggleSwitch : UserControl
{
    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register("IsChecked", typeof(bool), typeof(ToggleSwitch), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnIsCheckedChanged));

    // 変更通知用ルーティングイベント
    public static readonly RoutedEvent IsCheckedChangedEvent = EventManager.RegisterRoutedEvent(
        "IsCheckedChanged",
        RoutingStrategy.Bubble,
        typeof(RoutedPropertyChangedEventHandler<bool>),
        typeof(ToggleSwitch));

    public event RoutedPropertyChangedEventHandler<bool> IsCheckedChanged
    {
        add { AddHandler(IsCheckedChangedEvent, value); }
        remove { RemoveHandler(IsCheckedChangedEvent, value); }
    }

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set { SetValue(IsCheckedProperty, value); }
    }


    public ToggleSwitch()
    {
        InitializeComponent();
    }


    bool isAnimating = false;
    bool isClicking = false;

    private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        isClicking = true;
    }

    private void Grid_MouseLeave(object sender, MouseEventArgs e)
    {
        isClicking = false;
    }

    private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isAnimating) return;
        if (!isClicking) return;
        isClicking = false;

        IsChecked = !IsChecked;
    }

    private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ToggleSwitch tb)
        {
            tb.PlayToggleAnimation((bool)e.NewValue);
            tb.PlayColorAnimation((bool)e.NewValue);
            // ルーティングイベント発火
            var args = new RoutedPropertyChangedEventArgs<bool>((bool)e.OldValue, (bool)e.NewValue, IsCheckedChangedEvent);
            tb.RaiseEvent(args);
        }
    }

    private void PlayToggleAnimation(bool toOn)
    {
        var storyboard = (Storyboard)Resources[toOn ? "ToOn" : "ToOff"];
        if (storyboard == null) return;

        storyboard.Completed -= Storyboard_Completed; // 念のため解除
        storyboard.Completed += Storyboard_Completed;

        isAnimating = true;
        storyboard.Begin();
    }

    private void PlayColorAnimation(bool toOn)
    {
        Color from = toOn ? Colors.Gray : Colors.Cyan;
        Color to = toOn ? Colors.Cyan : Colors.Gray;
        var duration = new Duration(TimeSpan.FromMilliseconds(200));
        ApplyColorAnimation(LeftEllipse, from, to, duration);
        ApplyColorAnimation(MidRect, from, to, duration);
        ApplyColorAnimation(RightEllipse, from, to, duration);
    }

    private void ApplyColorAnimation(Shape shape, Color from, Color to, Duration duration)
    {
        if (shape.Fill is SolidColorBrush brush)
        {
            // Freeze されている場合があるので新しいブラシに差し替え
            if (brush.IsFrozen || brush.CanFreeze)
            {
                brush = new SolidColorBrush(((SolidColorBrush)shape.Fill).Color);
                shape.Fill = brush;
            }

            var animation = new ColorAnimation
            {
                From = from,
                To = to,
                Duration = duration,
                FillBehavior = FillBehavior.HoldEnd
            };
            brush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
        }
    }

    private void Storyboard_Completed(object? sender, EventArgs e)
    {
        isAnimating = false;
    }
}
