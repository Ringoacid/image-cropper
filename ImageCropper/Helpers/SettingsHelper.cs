using ImageCropper.Models;
using ImageCropper.ViewModels.Windows;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace ImageCropper.Helpers;

public static class SettingsHelper
{
    public static string SettingsFilePath = "settings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    public static async Task SaveSettings(MainViewModel vm)
    {
        var settings = new AppSettings
        {
            Output = new OutputSettings(vm.OutputSettings),
            UI = new UISettings(vm.UISettings),
            Presets = vm.Presets.Select(p => new CropPreset(p)).ToList()
        };

        // 設定ファイルのディレクトリが存在しない場合は作成
        var settingsDir = Path.GetDirectoryName(SettingsFilePath);
        if (!string.IsNullOrEmpty(settingsDir) && !Directory.Exists(settingsDir))
        {
            Directory.CreateDirectory(settingsDir);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(SettingsFilePath, json);
    }

    public static async Task LoadSettings(MainViewModel vm, bool warnIfNotExist = true)
    {
        if (!File.Exists(SettingsFilePath))
        {
            if (warnIfNotExist)
                MessageBox.Show("設定ファイルが見つかりません。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var json = await File.ReadAllTextAsync(SettingsFilePath);
        var settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);

        if (settings != null)
        {
            vm.OutputSettings.CopyFrom(settings.Output);
            if (settings.UI != null)
            {
                vm.UISettings.CopyFrom(settings.UI);
            }
            vm.Presets.Clear();
            foreach (var preset in settings.Presets)
            {
                vm.Presets.Add(preset);
            }
        }
    }

    /// <summary>
    /// 設定をエクスポートする
    /// </summary>
    /// <param name="settings">エクスポートする設定</param>
    /// <param name="filePath">エクスポート先のファイルパス</param>
    public static async Task ExportSettings(AppSettings settings, string filePath)
    {
        var settingsDir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(settingsDir) && !Directory.Exists(settingsDir))
        {
            Directory.CreateDirectory(settingsDir);
        }

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        await File.WriteAllTextAsync(filePath, json);
    }

    /// <summary>
    /// 設定をインポートする
    /// </summary>
    /// <param name="filePath">インポートするファイルパス</param>
    /// <returns>インポートした設定。失敗した場合はnull。</returns>
    public static async Task<AppSettings?> ImportSettings(string filePath)
    {
        if (!File.Exists(filePath))
        {
            MessageBox.Show("設定ファイルが見つかりません。", "警告", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        var json = await File.ReadAllTextAsync(filePath);
        return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions);
    }
}
