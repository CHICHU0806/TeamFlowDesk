using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
        LoadAiSettings();
    }

    private void SaveAiSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var baseUrl = AiBaseUrlTextBox.Text.Trim();
        var model = AiModelTextBox.Text.Trim();
        var apiKey = AiApiKeyPasswordBox.Password.Trim();

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            AiSettingsMessageText.Text = "API Base URL 不能为空。";
            return;
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            AiSettingsMessageText.Text = "模型名称不能为空。";
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            AiSettingsMessageText.Text = "API Key 不能为空。";
            return;
        }

        var settings = new AiProviderSettings
        {
            BaseUrl = baseUrl.TrimEnd('/'),
            Model = model,
            ApiKey = apiKey
        };

        AiSettingsService.Save(settings);

        AiSettingsMessageText.Text = "AI 配置已保存到本机。";
    }

    private void ReloadAiSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        LoadAiSettings();
        AiSettingsMessageText.Text = "AI 配置已重新读取。";
    }

    private void ClearAiSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settings = new AiProviderSettings
        {
            BaseUrl = "https://api.openai.com/v1",
            Model = string.Empty,
            ApiKey = string.Empty
        };

        AiSettingsService.Save(settings);

        LoadAiSettings();
        AiSettingsMessageText.Text = "AI 配置已清空。";
    }

    private void LoadAiSettings()
    {
        var settings = AiSettingsService.Load();

        AiBaseUrlTextBox.Text = string.IsNullOrWhiteSpace(settings.BaseUrl)
            ? "https://api.openai.com/v1"
            : settings.BaseUrl;

        AiModelTextBox.Text = settings.Model;
        AiApiKeyPasswordBox.Password = settings.ApiKey;
    }
}