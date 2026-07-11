using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;
using TeamFlowDesk.Services.Ui;

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
            AdoptionStatus = PageInteractionService.GetComboBoxText(
                AdoptionStatusComboBox,
                "部分采纳"),
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

        await PageInteractionService.ShowDetailDialogAsync(
            this,
            "AI 协作记录详情",
            new[]
            {
                new DetailSection("关联模块", record.RelatedModule),
                new DetailSection("问题描述", record.Question),
                new DetailSection("AI 建议", record.AiSuggestion),
                new DetailSection("人工判断", record.HumanJudgement),
                new DetailSection("最终决策", record.FinalDecision),
                new DetailSection("采纳状态", record.AdoptionStatus),
                new DetailSection("创建时间", record.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
            });
    }

    private void DeleteAiRecordButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
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
        });
    }

    private void UpdateAdoptionStatus(object sender, string adoptionStatus, string message)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var record = GetRecordFromButton(sender);

            if (record is null)
            {
                return;
            }

            var updatedRecord = CopyRecordWithNewAdoptionStatus(record, adoptionStatus);

            AiRecordRepository.Update(updatedRecord);

            PageInteractionService.ReplaceItem(
                _records,
                record,
                updatedRecord);

            RefreshStatistics();
            RefreshInsightPanel();

            AiRecordFormMessageText.Text = $"{message}：{updatedRecord.RelatedModule}";
        });
    }

    private AiRecordItem? GetRecordFromButton(object sender)
    {
        return PageInteractionService.GetItemFromButton(
            sender,
            _records,
            record => record.Id);
    }

    private static AiRecordItem CopyRecordWithNewAdoptionStatus(
        AiRecordItem source,
        string adoptionStatus)
    {
        return new AiRecordItem
        {
            Id = source.Id,
            RelatedModule = source.RelatedModule,
            Question = source.Question,
            AiSuggestion = source.AiSuggestion,
            HumanJudgement = source.HumanJudgement,
            FinalDecision = source.FinalDecision,
            AdoptionStatus = adoptionStatus,
            CreatedAt = source.CreatedAt
        };
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

        AiValueText.Text = "AI 已参与团队管理环节，人工判断和最终决策已沉淀为可追踪的复盘依据。";
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
}
