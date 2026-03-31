using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ImageCropper.ViewModels.Windows;

public partial class DebugViewModel : ObservableObject
{
    [ObservableProperty]
    private double value;

    [ObservableProperty]
    private bool isIntegerOnly;

    [ObservableProperty]
    private bool isWarning;

    Random rnd = new();

    [RelayCommand]
    private void OnSetRandomValue()
    {
        Value = rnd.Next(0, 100);
    }
}
