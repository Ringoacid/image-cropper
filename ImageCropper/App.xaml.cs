using System.Windows;

namespace ImageCropper;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
    }

    /// <summary>
    /// アプリ全体のテーマ（ライト/ダーク）を切り替える。
    /// </summary>
    /// <param name="isDarkMode">ダークモードを有効にするかどうか</param>
    public static void ApplyTheme(bool isDarkMode)
    {
        var themeUri = new Uri(isDarkMode ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml", UriKind.Relative);
        var newTheme = new ResourceDictionary { Source = themeUri };

        // 1つ目のマージ済み辞書がテーマ辞書（App.xamlでLightTheme.xamlを既定として登録済み）
        Current.Resources.MergedDictionaries[0] = newTheme;
    }
}
