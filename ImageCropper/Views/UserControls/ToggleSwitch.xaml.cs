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
        Focusable = true;
        this.KeyDown += ToggleSwitch_KeyDown;
    }

    private void ToggleSwitch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Space || e.Key == Key.Enter)
        {
            if (isAnimating) return;
            IsChecked = !IsChecked;
            e.Handled = true;
        }
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

    private void Storyboard_Completed(object? sender, EventArgs e)
    {
        isAnimating = false;
    }
}
