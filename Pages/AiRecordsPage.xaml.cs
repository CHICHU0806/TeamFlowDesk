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
        AiRecordFormMessageText.Text = "正在生成 AI 建议，请稍等...";

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