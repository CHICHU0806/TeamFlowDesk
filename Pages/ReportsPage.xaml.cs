using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;

namespace TeamFlowDesk.Pages;

public sealed partial class ReportsPage : Page
{
    private readonly ObservableCollection<WeeklyReportItem> _reports;

    public ReportsPage()
    {
        InitializeComponent();

        try
        {
            WeeklyReportRepository.SeedIfEmpty();
            _reports = new ObservableCollection<WeeklyReportItem>(WeeklyReportRepository.GetAll());

            ReportsListView.ItemsSource = _reports;

            ReportStartDatePicker.Date = DateTimeOffset.Now.AddDays(-7);
            ReportEndDatePicker.Date = DateTimeOffset.Now;

            RefreshStatistics();
            RefreshDataSnapshot();
        }
        catch (Exception ex)
        {
            _reports = new ObservableCollection<WeeklyReportItem>();
            ReportsListView.ItemsSource = _reports;

            ReportStartDatePicker.Date = DateTimeOffset.Now.AddDays(-7);
            ReportEndDatePicker.Date = DateTimeOffset.Now;

            ReportFormMessageText.Text = $"复盘页面加载失败：{ex.Message}";
        }
    }

    private void GenerateReportDraftButton_Click(object sender, RoutedEventArgs e)
    {
        var tasks = TaskRepository.GetAll();
        var members = MemberRepository.GetAll();
        var equipment = EquipmentRepository.GetAll();
        var aiRecords = AiRecordRepository.GetAll();

        var completedTasks = tasks
            .Where(task => task.Status == "已完成")
            .Take(5)
            .ToList();

        var doingTasks = tasks
            .Where(task => task.Status == "进行中" || task.Status == "待处理")
            .Take(5)
            .ToList();

        var riskTasks = tasks
            .Where(task =>
                task.RiskLevel == "高风险" ||
                task.Status == "延期" ||
                task.Status == "滞后")
            .Take(5)
            .ToList();

        var attentionMembers = members
            .Where(member =>
                member.WorkloadStatus == "关注" ||
                member.WorkloadStatus == "过载")
            .Take(5)
            .ToList();

        var abnormalEquipment = equipment
            .Where(item =>
                item.Status == "待检查" ||
                item.Status == "损坏" ||
                item.Status == "维修中" ||
                item.Status == "报废")
            .Take(5)
            .ToList();

        var recentAiRecords = aiRecords
            .Take(5)
            .ToList();

        if (string.IsNullOrWhiteSpace(ReportTitleTextBox.Text))
        {
            ReportTitleTextBox.Text = $"TeamFlowDesk 复盘记录 {DateTimeOffset.Now:yyyy-MM-dd}";
        }

        CompletedWorkTextBox.Text = BuildCompletedWorkText(completedTasks, doingTasks);
        ProblemsTextBox.Text = BuildProblemsText(riskTasks, attentionMembers, abnormalEquipment);
        NextPlanTextBox.Text = BuildNextPlanText(doingTasks, riskTasks);
        AiCollaborationSummaryTextBox.Text = BuildAiSummaryText(recentAiRecords);
        ManagerReviewTextBox.Text = BuildManagerReviewText(riskTasks.Count, attentionMembers.Count, abnormalEquipment.Count);

        RefreshDataSnapshot();

        ReportFormMessageText.Text = "已根据当前任务、人员、器材和 AI 协作记录生成复盘草稿，可以继续手动修改后保存。";
    }

    private void AddReportButton_Click(object sender, RoutedEventArgs e)
    {
        var title = ReportTitleTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ReportFormMessageText.Text = "周报标题不能为空。";
            return;
        }

        var report = new WeeklyReportItem
        {
            Title = title,
            StartDate = ReportStartDatePicker.Date,
            EndDate = ReportEndDatePicker.Date,
            CompletedWork = CompletedWorkTextBox.Text.Trim(),
            Problems = ProblemsTextBox.Text.Trim(),
            NextPlan = NextPlanTextBox.Text.Trim(),
            AiCollaborationSummary = AiCollaborationSummaryTextBox.Text.Trim(),
            ManagerReview = ManagerReviewTextBox.Text.Trim(),
            ProgressStatus = GetComboBoxText(ProgressStatusComboBox, "正常")
        };

        report.Id = WeeklyReportRepository.Add(report);
        _reports.Insert(0, report);

        RefreshStatistics();
        RefreshDataSnapshot();
        ClearReportForm();

        ReportFormMessageText.Text = $"复盘记录已保存：{report.Title}";
    }

    private void ClearReportFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearReportForm();
        ReportFormMessageText.Text = "输入内容已清空。";
    }

    private void SetNormalReportButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateReportStatus(sender, "正常", "复盘记录已标记为正常");
    }

    private void SetAttentionReportButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateReportStatus(sender, "关注", "复盘记录已标记为关注");
    }

    private void SetHighRiskReportButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateReportStatus(sender, "高风险", "复盘记录已标记为高风险");
    }

    private void DeleteReportButton_Click(object sender, RoutedEventArgs e)
    {
        var report = GetReportFromButton(sender);

        if (report is null)
        {
            return;
        }

        WeeklyReportRepository.Delete(report.Id);
        _reports.Remove(report);

        RefreshStatistics();
        RefreshDataSnapshot();

        ReportFormMessageText.Text = $"复盘记录已删除：{report.Title}";
    }

    private void UpdateReportStatus(object sender, string status, string message)
    {
        var report = GetReportFromButton(sender);

        if (report is null)
        {
            return;
        }

        report.ProgressStatus = status;

        WeeklyReportRepository.Update(report);
        RefreshReportList();
        RefreshStatistics();
        RefreshDataSnapshot();

        ReportFormMessageText.Text = $"{message}：{report.Title}";
    }

    private WeeklyReportItem? GetReportFromButton(object sender)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return null;
        }

        if (!int.TryParse(button.Tag.ToString(), out var reportId))
        {
            return null;
        }

        return _reports.FirstOrDefault(report => report.Id == reportId);
    }

    private void RefreshStatistics()
    {
        ReportCountText.Text = _reports.Count.ToString();

        NormalProgressCountText.Text = _reports
            .Count(report =>
                report.ProgressStatus == "正常" ||
                report.ProgressStatus == "已完成")
            .ToString();

        RiskReportCountText.Text = _reports
            .Count(report =>
                report.ProgressStatus == "关注" ||
                report.ProgressStatus == "滞后" ||
                report.ProgressStatus == "高风险")
            .ToString();

        AiReportCountText.Text = _reports
            .Count(report => !string.IsNullOrWhiteSpace(report.AiCollaborationSummary))
            .ToString();

        ReviewStatusText.Text =
            $"当前已沉淀 {_reports.Count} 条复盘记录，其中包含 AI 协作摘要的记录有 {AiReportCountText.Text} 条。";
    }

    private void RefreshDataSnapshot()
    {
        var tasks = TaskRepository.GetAll();
        var members = MemberRepository.GetAll();
        var equipment = EquipmentRepository.GetAll();
        var aiRecords = AiRecordRepository.GetAll();

        var completedTaskCount = tasks.Count(task => task.Status == "已完成");
        var doingTaskCount = tasks.Count(task => task.Status == "进行中" || task.Status == "待处理");
        var riskTaskCount = tasks.Count(task =>
            task.RiskLevel == "高风险" ||
            task.Status == "延期" ||
            task.Status == "滞后");

        var attentionMemberCount = members.Count(member =>
            member.WorkloadStatus == "关注" ||
            member.WorkloadStatus == "过载");

        var abnormalEquipmentCount = equipment.Count(item =>
            item.Status == "待检查" ||
            item.Status == "损坏" ||
            item.Status == "维修中" ||
            item.Status == "报废");

        TaskSnapshotText.Text =
            $"共 {tasks.Count} 项任务，已完成 {completedTaskCount} 项，推进中 {doingTaskCount} 项，风险任务 {riskTaskCount} 项。";

        MemberSnapshotText.Text =
            $"共 {members.Count} 名成员，其中需要关注的负载状态有 {attentionMemberCount} 项。";

        EquipmentSnapshotText.Text =
            $"共 {equipment.Count} 件器材，其中异常或待检查器材 {abnormalEquipmentCount} 件。";

        AiSnapshotText.Text =
            $"共沉淀 {aiRecords.Count} 条 AI 协作记录，可用于生成复盘摘要和负责人判断参考。";
    }

    private void RefreshReportList()
    {
        ReportsListView.ItemsSource = null;
        ReportsListView.ItemsSource = _reports;
    }

    private void ClearReportForm()
    {
        ReportTitleTextBox.Text = string.Empty;
        CompletedWorkTextBox.Text = string.Empty;
        ProblemsTextBox.Text = string.Empty;
        NextPlanTextBox.Text = string.Empty;
        AiCollaborationSummaryTextBox.Text = string.Empty;
        ManagerReviewTextBox.Text = string.Empty;

        ReportStartDatePicker.Date = DateTimeOffset.Now.AddDays(-7);
        ReportEndDatePicker.Date = DateTimeOffset.Now;
        ProgressStatusComboBox.SelectedIndex = 0;
    }

    private static string BuildCompletedWorkText(
        System.Collections.Generic.IReadOnlyCollection<TaskItem> completedTasks,
        System.Collections.Generic.IReadOnlyCollection<TaskItem> doingTasks)
    {
        var text = "本阶段系统根据当前任务数据整理出以下完成情况：\n";

        if (completedTasks.Count == 0)
        {
            text += "1. 当前暂无已完成任务记录，需要负责人进一步更新任务状态。\n";
        }
        else
        {
            var index = 1;

            foreach (var task in completedTasks)
            {
                text += $"{index}. 已完成任务：{task.Title}，负责人：{task.OwnerName}。\n";
                index++;
            }
        }

        if (doingTasks.Count > 0)
        {
            text += "\n仍在推进中的任务包括：\n";

            foreach (var task in doingTasks)
            {
                text += $"- {task.Title}，当前状态：{task.Status}。\n";
            }
        }

        return text;
    }

    private static string BuildProblemsText(
        System.Collections.Generic.IReadOnlyCollection<TaskItem> riskTasks,
        System.Collections.Generic.IReadOnlyCollection<MemberItem> attentionMembers,
        System.Collections.Generic.IReadOnlyCollection<EquipmentItem> abnormalEquipment)
    {
        var text = "当前系统识别出的主要问题与风险如下：\n";

        if (riskTasks.Count == 0 && attentionMembers.Count == 0 && abnormalEquipment.Count == 0)
        {
            return text + "1. 当前暂无明显高风险任务、过载成员或异常器材，整体推进状态较平稳。";
        }

        var index = 1;

        foreach (var task in riskTasks)
        {
            text += $"{index}. 任务风险：{task.Title}，状态：{task.Status}，风险等级：{task.RiskLevel}。\n";
            index++;
        }

        foreach (var member in attentionMembers)
        {
            text += $"{index}. 人员负载风险：{member.Name}，当前任务数：{member.CurrentTaskCount}，负载状态：{member.WorkloadStatus}。\n";
            index++;
        }

        foreach (var item in abnormalEquipment)
        {
            text += $"{index}. 器材风险：{item.Name}，当前状态：{item.Status}，存放位置：{item.Location}。\n";
            index++;
        }

        return text;
    }

    private static string BuildNextPlanText(
        System.Collections.Generic.IReadOnlyCollection<TaskItem> doingTasks,
        System.Collections.Generic.IReadOnlyCollection<TaskItem> riskTasks)
    {
        var text = "下一阶段建议围绕以下方向推进：\n";

        var index = 1;

        foreach (var task in doingTasks.Take(3))
        {
            text += $"{index}. 继续推进任务：{task.Title}，明确负责人 {task.OwnerName} 的下一步输出要求。\n";
            index++;
        }

        foreach (var task in riskTasks.Take(3))
        {
            text += $"{index}. 优先处理风险任务：{task.Title}，需要重新评估任务拆分、截止时间和资源支持。\n";
            index++;
        }

        if (index == 1)
        {
            text += "1. 继续保持任务状态更新，围绕项目阶段目标形成新的任务拆解。\n";
        }

        text += $"{index}. 保持 AI 协作记录和人工判断记录同步沉淀，便于后续答辩和复盘。";

        return text;
    }

    private static string BuildAiSummaryText(
        System.Collections.Generic.IReadOnlyCollection<AiRecordItem> aiRecords)
    {
        if (aiRecords.Count == 0)
        {
            return "当前暂无 AI 协作记录。后续可以在 AI 协作记录页面中记录问题、AI 建议、人工判断和最终决策。";
        }

        var text = "本阶段已沉淀以下 AI 协作过程：\n";
        var index = 1;

        foreach (var record in aiRecords)
        {
            text += $"{index}. 关联模块：{record.RelatedModule}；问题：{record.Question}；采纳状态：{record.AdoptionStatus}。\n";
            index++;
        }

        return text;
    }

    private static string BuildManagerReviewText(
        int riskTaskCount,
        int attentionMemberCount,
        int abnormalEquipmentCount)
    {
        if (riskTaskCount == 0 && attentionMemberCount == 0 && abnormalEquipmentCount == 0)
        {
            return "从当前数据看，团队运行状态整体平稳。后续应继续保持任务状态、AI 协作记录和周报复盘的持续更新。";
        }

        return
            $"从当前数据看，系统识别到 {riskTaskCount} 项任务风险、{attentionMemberCount} 项人员负载风险和 {abnormalEquipmentCount} 项器材风险。后续需要负责人优先处理阻塞项，并根据实际情况调整任务分配和资源支持。";
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