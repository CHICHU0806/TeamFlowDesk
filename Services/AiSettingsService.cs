using System.Text.Json;
using TeamFlowDesk.Models;

namespace TeamFlowDesk.Services;

public static class AiSettingsService
{
    private const string SettingsFileName = "ai-settings.json";

    private static string SettingsFolder
    {
        get
        {
            var folder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TeamFlowDesk");

            Directory.CreateDirectory(folder);

            return folder;
        }
    }

    private static string SettingsPath => Path.Combine(SettingsFolder, SettingsFileName);

    public static AiProviderSettings Load()
    {
        if (!File.Exists(SettingsPath))
        {
            return new AiProviderSettings();
        }

        try
        {
            var json = File.ReadAllText(SettingsPath);
            var settings = JsonSerializer.Deserialize<AiProviderSettings>(json);

            return settings ?? new AiProviderSettings();
        }
        catch
        {
            return new AiProviderSettings();
        }
    }

    public static void Save(AiProviderSettings settings)
    {
        var json = JsonSerializer.Serialize(
            settings,
            new JsonSerializerOptions
            {
                WriteIndented = true
            });

        File.WriteAllText(SettingsPath, json);
    }

    public static bool HasValidApiSettings()
    {
        var settings = Load();

        return !string.IsNullOrWhiteSpace(settings.BaseUrl)
               && !string.IsNullOrWhiteSpace(settings.Model)
               && !string.IsNullOrWhiteSpace(settings.ApiKey);
    }
}