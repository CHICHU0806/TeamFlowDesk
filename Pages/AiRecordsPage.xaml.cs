using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class AiRecordsPage : Page
{
    private readonly ObservableCollection<AiRecordItem> _records;
    private readonly IAiAssistantService _aiAssistantService = new OpenAiAssistantService();

    public AiRecordsPage()
    {
        InitializeComponent();

        AiRecordRepository.SeedIfEmpty();
        _records = new ObservableCollection<AiRecordItem>(AiRecordRepository.GetAll());

        AiRecordsListView.ItemsSource = _records;

        RefreshStatistics();
        RefreshInsightPanel();
    }

    private async void GenerateAiSuggestionButton_Click(object sender, RoutedEventArgs e)
    {
        var relatedModule = RelatedModuleTextBox.Text.Trim();
        var question = QuestionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(relatedModule))
        {
            AiRecordFormMessageText.Text = "生成 AI 建议前，请先填写关联模块。";
            return;
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            AiRecordFormMessageText.Text = "生成 AI 建议前，请先填写问题描述。";
            return;
        }

        GenerateAiSuggestionButton.IsEnabled = false;
        AiRecordFormMessageText.Text = "正在调用用户配置的 API 生成 AI 建议，请稍等...";

        var result = await _aiAssistantService.GenerateManagementSuggestionAsync(
            relatedModule,
            question);

        GenerateAiSuggestionButton.IsEnabled = true;

        if (!result.IsSuccess)
        {
            AiRecordFormMessageText.Text = result.ErrorMessage;
            return;
        }

        AiSuggestionTextBox.Text = result.Content;
        AiRecordFormMessageText.Text = "AI 建议已生成，请继续填写人工判断和最终决策。";
    }

    private void AddAiRecordButton_Click(object sender, RoutedEventArgs e)
    {
        var relatedModule = RelatedModuleTextBox.Text.Trim();
        var question = QuestionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(relatedModule))
        {
            AiRecordFormMessageText.Text = "关联模块不能为空。";
            return;
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            AiRecordFormMessageText.Text = "问题描述不能为空。";
            return;
        }

        var newRecord = new AiRecordItem
        {
            RelatedModule = relatedModule,
            Question = question,
            AiSuggestion = AiSuggestionTextBox.Text.Trim(),
            HumanJudgement = HumanJudgementTextBox.Text.Trim(),
            FinalDecision = FinalDecisionTextBox.Text.Trim(),
            AdoptionStatus = GetComboBoxText(AdoptionStatusComboBox, "部分采纳"),
            CreatedAt = DateTimeOffset.Now
        };

        newRecord.Id = AiRecordRepository.Add(newRecord);
        _records.Insert(0, newRecord);

        RefreshStatistics();
        RefreshInsightPanel();
        ClearAiRecordForm();

        AiRecordFormMessageText.Text = $"已新增 AI 协作记录：{newRecord.RelatedModule}";
    }

    private void ClearAiRecordFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearAiRecordForm();
        AiRecordFormMessageText.Text = "输入内容已清空。";
    }

    private void SetAcceptedButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateAdoptionStatus(sender, "采纳", "记录已标记为采纳");
    }

    private void SetPartAcceptedButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateAdoptionStatus(sender, "部分采纳", "记录已标记为部分采纳");
    }

    private void SetRejectedButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateAdoptionStatus(sender, "未采纳", "记录已标记为未采纳");
    }

    private async void ShowAiRecordDetailButton_Click(object sender, RoutedEventArgs e)
    {
        var record = GetRecordFromButton(sender);

        if (record is null)
        {
            return;
        }

        var detailPanel = new StackPanel
        {
            Spacing = 16,
            MaxWidth = 920
        };

        detailPanel.Children.Add(CreateDetailBlock("关联模块", record.RelatedModule));
        detailPanel.Children.Add(CreateDetailBlock("问题描述", record.Question));
        detailPanel.Children.Add(CreateDetailBlock("AI 建议", record.AiSuggestion));
        detailPanel.Children.Add(CreateDetailBlock("人工判断", record.HumanJudgement));
        detailPanel.Children.Add(CreateDetailBlock("最终决策", record.FinalDecision));
        detailPanel.Children.Add(CreateDetailBlock("采纳状态", record.AdoptionStatus));
        detailPanel.Children.Add(CreateDetailBlock("创建时间", record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss")));

        var scrollViewer = new ScrollViewer
        {
            Content = detailPanel,
            MaxHeight = 680,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        };

        var dialog = new ContentDialog
        {
            Title = "AI 协作记录详情",
            Content = scrollViewer,
            CloseButtonText = "关闭",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    private void DeleteAiRecordButton_Click(object sender, RoutedEventArgs e)
    {
        var record = GetRecordFromButton(sender);

        if (record is null)
        {
            return;
        }

        AiRecordRepository.Delete(record.Id);
        _records.Remove(record);

        RefreshStatistics();
        RefreshInsightPanel();

        AiRecordFormMessageText.Text = $"AI 协作记录已删除：{record.RelatedModule}";
    }

    private void UpdateAdoptionStatus(object sender, string adoptionStatus, string message)
    {
        var record = GetRecordFromButton(sender);

        if (record is null)
        {
            return;
        }

        record.AdoptionStatus = adoptionStatus;

        AiRecordRepository.Update(record);
        RefreshRecordList();
        RefreshStatistics();
        RefreshInsightPanel();

        AiRecordFormMessageText.Text = $"{message}：{record.RelatedModule}";
    }

    private AiRecordItem? GetRecordFromButton(object sender)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return null;
        }

        if (!int.TryParse(button.Tag.ToString(), out var recordId))
        {
            return null;
        }

        return _records.FirstOrDefault(record => record.Id == recordId);
    }

    private void RefreshStatistics()
    {
        RecordCountText.Text = _records.Count.ToString();

        AcceptedCountText.Text = _records
            .Count(record => record.AdoptionStatus == "采纳")
            .ToString();

        PartAcceptedCountText.Text = _records
            .Count(record => record.AdoptionStatus == "部分采纳")
            .ToString();

        ModuleCountText.Text = _records
            .Select(record => record.RelatedModule)
            .Where(module => !string.IsNullOrWhiteSpace(module))
            .Distinct()
            .Count()
            .ToString();
    }

    private void RefreshInsightPanel()
    {
        AiConfigStatusText.Text = AiSettingsService.HasValidApiSettings()
            ? "AI API 已配置。当前页面可以调用用户自己的 API 生成管理建议。"
            : "AI API 尚未配置。请先在系统设置中填写 API Base URL、模型名称和 API Key。";

        var latestRecord = _records.FirstOrDefault();

        RecentModuleText.Text = latestRecord is null
            ? "暂无协作记录。"
            : latestRecord.RelatedModule;

        var acceptedCount = _records.Count(record => record.AdoptionStatus == "采纳");
        var partAcceptedCount = _records.Count(record => record.AdoptionStatus == "部分采纳");
        var rejectedCount = _records.Count(record => record.AdoptionStatus == "未采纳");
        var pendingCount = _records.Count(record => record.AdoptionStatus == "待判断");

        AdoptionSummaryText.Text =
            $"采纳 {acceptedCount} 条，部分采纳 {partAcceptedCount} 条，未采纳 {rejectedCount} 条，待判断 {pendingCount} 条。";

        if (_records.Count == 0)
        {
            AiValueText.Text = "当前暂无 AI 协作数据。建议先围绕任务拆解、风险判断或周报生成记录一条完整协作过程。";
            return;
        }

        if (acceptedCount + partAcceptedCount == 0)
        {
            AiValueText.Text = "当前 AI 建议尚未被采纳。建议重点记录人工判断原因，让系统体现“人负责最终决策”。";
            return;
        }

        AiValueText.Text = "AI 已经参与部分团队管理环节。后续可以继续沉淀人工判断和最终决策，用于周报复盘和答辩展示。";
    }

    private void RefreshRecordList()
    {
        AiRecordsListView.ItemsSource = null;
        AiRecordsListView.ItemsSource = _records;
    }

    private void ClearAiRecordForm()
    {
        RelatedModuleTextBox.Text = string.Empty;
        QuestionTextBox.Text = string.Empty;
        AiSuggestionTextBox.Text = string.Empty;
        HumanJudgementTextBox.Text = string.Empty;
        FinalDecisionTextBox.Text = string.Empty;

        AdoptionStatusComboBox.SelectedIndex = 1;
    }

    private static StackPanel CreateDetailBlock(string title, string content)
    {
        var panel = new StackPanel
        {
            Spacing = 6
        };

        panel.Children.Add(new TextBlock
        {
            Text = title,
            FontSize = 15,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            TextWrapping = TextWrapping.Wrap
        });

        panel.Children.Add(new TextBlock
        {
            Text = string.IsNullOrWhiteSpace(content) ? "暂无内容" : content,
            FontSize = 14,
            TextWrapping = TextWrapping.Wrap,
            IsTextSelectionEnabled = true
        });

        return panel;
    }

    private static string GetComboBoxText(ComboBox comboBox, string fallback)
    {
        if (comboBox.SelectedItem is ComboBoxItem selectedItem &&
            selectedItem.Content is not null)
        {
            return selectedItem.Content.ToString() ?? fallback;
        }

        return fallback;
    }
}