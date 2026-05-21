using System.Windows;
using System.Windows.Input;
using ImageCropper.ViewModels.Windows;

namespace ImageCropper.Views.Windows;

/// <summary>
/// ProgressWindow.xaml の相互作用ロジック
/// </summary>
public partial class ProgressWindow : Window
{
    public bool IsClosedProgrammatically { get; set; } = false;

    public ProgressWindow()
    {
        InitializeComponent();
    }

    public new void Close()
    {
        IsClosedProgrammatically = true;
        base.Close();
    }

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (e.Key == Key.Escape)
        {
            base.Close();
            e.Handled = true;
        }
    }

    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        base.OnClosing(e);
        if (!IsClosedProgrammatically)
        {
            TriggerCancellation();
        }
    }

    private void TriggerCancellation()
    {
        if (DataContext is ProgressViewModel vm)
        {
            if (vm.CancelCommand != null && vm.CancelCommand.CanExecute(null))
            {
                vm.CancelCommand.Execute(null);
            }
        }
    }
}
