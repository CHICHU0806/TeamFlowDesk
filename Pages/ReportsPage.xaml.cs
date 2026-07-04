using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services.Ui;

namespace TeamFlowDesk.Pages;

public sealed partial class ReportsPage : Page
{
    private readonly ObservableCollection<WeeklyReportItem> _reports = new();

    public ReportsPage()
    {
        InitializeComponent();

        ReportStartDatePicker.Date = DateTimeOffset.Now.AddDays(-7);
        ReportEndDatePicker.Date = DateTimeOffset.Now;
        ProgressStatusComboBox.SelectedIndex = 0;

        try
        {
            WeeklyReportRepository.SeedIfEmpty();

            foreach (var report in WeeklyReportRepository.GetAll())
            {
                _reports.Add(report);
            }

            ReportsListView.ItemsSource = _reports;

            RefreshStatistics();
            RefreshDataSnapshot();
        }
        catch (Exception ex)
        {
            ReportsListView.ItemsSource = _reports;
            ReportFormMessageText.Text = $"复盘页面加载失败：{ex.Message}";
            ReviewStatusText.Text = "复盘数据暂时无法读取，请检查数据库初始化状态。";
        }
    }

    private void GenerateReportDraftButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var tasks = TaskRepository.GetAll();
            var members = MemberRepository.GetAll();
            var equipment = EquipmentRepository.GetAll();
            var aiRecords = AiRecordRepository.GetAll();

            ReportTitleTextBox.Text = $"TeamFlowDesk 周报复盘 {DateTimeOffset.Now:MM.dd}";
            ReportStartDatePicker.Date = DateTimeOffset.Now.AddDays(-7);
            ReportEndDatePicker.Date = DateTimeOffset.Now;

            CompletedWorkTextBox.Text = BuildCompletedWorkText(tasks);
            ProblemsTextBox.Text = BuildProblemsText(tasks, members, equipment);
            NextPlanTextBox.Text = BuildNextPlanText(tasks);
            AiCollaborationSummaryTextBox.Text = BuildAiSummaryText(aiRecords);
            ManagerReviewTextBox.Text = BuildManagerReviewText(tasks, members, equipment, aiRecords);

            var hasHighRisk = tasks.Any(task =>
                                  task.RiskLevel == "高风险" ||
                                  task.Status == "延期" ||
                                  task.Status == "滞后")
                              || members.Any(member =>
                                  member.WorkloadStatus == "关注" ||
                                  member.WorkloadStatus == "过载")
                              || equipment.Any(IsAbnormalEquipment);

            ProgressStatusComboBox.SelectedIndex = hasHighRisk ? 1 : 0;

            RefreshDataSnapshot();

            ReportFormMessageText.Text = "已根据当前任务、人员、器材和 AI 协作记录生成复盘草稿，请继续人工修正。";
        }
        catch (Exception ex)
        {
            ReportFormMessageText.Text = $"生成复盘草稿失败：{ex.Message}";
        }
    }

    private void AddReportButton_Click(object sender, RoutedEventArgs e)
    {
        var title = ReportTitleTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            ReportFormMessageText.Text = "周报标题不能为空。";
            return;
        }

        var newReport = new WeeklyReportItem
        {
            Title = title,
            StartDate = ReportStartDatePicker.Date,
            EndDate = ReportEndDatePicker.Date,
            CompletedWork = CompletedWorkTextBox.Text.Trim(),
            Problems = ProblemsTextBox.Text.Trim(),
            NextPlan = NextPlanTextBox.Text.Trim(),
            AiCollaborationSummary = AiCollaborationSummaryTextBox.Text.Trim(),
            ManagerReview = ManagerReviewTextBox.Text.Trim(),
            ProgressStatus = PageInteractionService.GetComboBoxText(
                ProgressStatusComboBox,
                "正常")
        };

        try
        {
            newReport.Id = WeeklyReportRepository.Add(newReport);
            _reports.Insert(0, newReport);

            RefreshStatistics();
            RefreshDataSnapshot();
            ClearReportForm();

            ReportFormMessageText.Text = $"已新增复盘记录：{newReport.Title}";
        }
        catch (Exception ex)
        {
            ReportFormMessageText.Text = $"保存复盘失败：{ex.Message}";
        }
    }

    private void ClearReportFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearReportForm();
        ReportFormMessageText.Text = "复盘编辑器已清空。";
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

    private async void ShowReportDetailButton_Click(object sender, RoutedEventArgs e)
    {
        var report = GetReportFromButton(sender);

        if (report is null)
        {
            return;
        }

        await PageInteractionService.ShowDetailDialogAsync(
            this,
            "复盘记录详情",
            new[]
            {
                new DetailSection("周报标题", report.Title),
                new DetailSection("复盘周期", $"{report.StartDate:yyyy-MM-dd} 至 {report.EndDate:yyyy-MM-dd}"),
                new DetailSection("进度状态", report.ProgressStatus),
                new DetailSection("本周完成工作", report.CompletedWork),
                new DetailSection("问题与风险", report.Problems),
                new DetailSection("下周计划", report.NextPlan),
                new DetailSection("AI 协作摘要", report.AiCollaborationSummary),
                new DetailSection("负责人复盘判断", report.ManagerReview)
            });
    }

    private void DeleteReportButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
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
        });
    }

    private void UpdateReportStatus(object sender, string progressStatus, string message)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var report = GetReportFromButton(sender);

            if (report is null)
            {
                return;
            }

            var updatedReport = CopyReportWithNewStatus(report, progressStatus);

            WeeklyReportRepository.Update(updatedReport);

            PageInteractionService.ReplaceItem(
                _reports,
                report,
                updatedReport);

            RefreshStatistics();
            RefreshDataSnapshot();

            ReportFormMessageText.Text = $"{message}：{updatedReport.Title}";
        });
    }

    private WeeklyReportItem? GetReportFromButton(object sender)
    {
        return PageInteractionService.GetItemFromButton(
            sender,
            _reports,
            report => report.Id);
    }

    private static WeeklyReportItem CopyReportWithNewStatus(
        WeeklyReportItem source,
        string progressStatus)
    {
        return new WeeklyReportItem
        {
            Id = source.Id,
            Title = source.Title,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            CompletedWork = source.CompletedWork,
            Problems = source.Problems,
            NextPlan = source.NextPlan,
            AiCollaborationSummary = source.AiCollaborationSummary,
            ManagerReview = source.ManagerReview,
            ProgressStatus = progressStatus
        };
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

        var riskCount = _reports.Count(report =>
            report.ProgressStatus == "关注" ||
            report.ProgressStatus == "滞后" ||
            report.ProgressStatus == "高风险");

        if (_reports.Count == 0)
        {
            ReviewStatusText.Text = "当前暂无复盘记录，建议先生成一条周报草稿并人工修正保存。";
        }
        else if (riskCount > 0)
        {
            ReviewStatusText.Text = $"当前已有 {_reports.Count} 条复盘记录，其中 {riskCount} 条需要负责人关注。";
        }
        else
        {
            ReviewStatusText.Text = $"当前已有 {_reports.Count} 条复盘记录，整体推进状态较稳定。";
        }
    }

    private void RefreshDataSnapshot()
    {
        try
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

            var abnormalEquipmentCount = equipment.Count(IsAbnormalEquipment);

            TaskSnapshotText.Text =
                $"共 {tasks.Count} 项任务，已完成 {completedTaskCount} 项，推进中 {doingTaskCount} 项，风险任务 {riskTaskCount} 项。";

            MemberSnapshotText.Text =
                $"共 {members.Count} 名成员，其中需要关注的负载状态有 {attentionMemberCount} 项。";

            EquipmentSnapshotText.Text =
                $"共 {equipment.Count} 件器材，其中异常或待检查器材 {abnormalEquipmentCount} 件。";

            AiSnapshotText.Text =
                $"共沉淀 {aiRecords.Count} 条 AI 协作记录，可用于生成复盘摘要和负责人判断参考。";
        }
        catch (Exception ex)
        {
            TaskSnapshotText.Text = $"数据快照读取失败：{ex.Message}";
            MemberSnapshotText.Text = "暂无有效数据。";
            EquipmentSnapshotText.Text = "暂无有效数据。";
            AiSnapshotText.Text = "暂无有效数据。";
        }
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

    private static string BuildCompletedWorkText(IReadOnlyCollection<TaskItem> tasks)
    {
        var completedTasks = tasks
            .Where(task => task.Status == "已完成")
            .Take(6)
            .Select(task => $"· {task.Title}（负责人：{SafeText(task.OwnerName)}）")
            .ToList();

        if (completedTasks.Count == 0)
        {
            return "本周暂无明确标记为已完成的任务，建议负责人检查任务状态是否及时更新。";
        }

        return string.Join(Environment.NewLine, completedTasks);
    }

    private static string BuildProblemsText(
        IReadOnlyCollection<TaskItem> tasks,
        IReadOnlyCollection<MemberItem> members,
        IReadOnlyCollection<EquipmentItem> equipment)
    {
        var problems = new List<string>();

        var riskTasks = tasks
            .Where(task =>
                task.RiskLevel == "高风险" ||
                task.Status == "延期" ||
                task.Status == "滞后")
            .Take(5)
            .Select(task => $"· 风险任务：{task.Title}（状态：{task.Status}，风险：{task.RiskLevel}）");

        problems.AddRange(riskTasks);

        var attentionMembers = members
            .Where(member =>
                member.WorkloadStatus == "关注" ||
                member.WorkloadStatus == "过载")
            .Take(5)
            .Select(member => $"· 成员负载需关注：{member.Name}（状态：{member.WorkloadStatus}，任务数：{member.CurrentTaskCount}）");

        problems.AddRange(attentionMembers);

        var abnormalEquipment = equipment
            .Where(IsAbnormalEquipment)
            .Take(5)
            .Select(item => $"· 器材异常：{item.Name}（状态：{item.Status}）");

        problems.AddRange(abnormalEquipment);

        if (problems.Count == 0)
        {
            return "当前未发现明显风险项，但仍建议负责人继续检查任务截止时间、成员负载和器材状态。";
        }

        return string.Join(Environment.NewLine, problems);
    }

    private static string BuildNextPlanText(IReadOnlyCollection<TaskItem> tasks)
    {
        var nextTasks = tasks
            .Where(task =>
                task.Status == "待处理" ||
                task.Status == "进行中" ||
                task.RiskLevel == "高风险")
            .Take(6)
            .Select(task => $"· 继续推进：{task.Title}（负责人：{SafeText(task.OwnerName)}，输出要求：{SafeText(task.OutputRequirement)}）")
            .ToList();

        if (nextTasks.Count == 0)
        {
            return "下周建议围绕项目下一阶段目标重新拆解任务，并明确负责人、截止时间和输出要求。";
        }

        return string.Join(Environment.NewLine, nextTasks);
    }

    private static string BuildAiSummaryText(IReadOnlyCollection<AiRecordItem> aiRecords)
    {
        var latestAiRecords = aiRecords
            .OrderByDescending(record => record.CreatedAt)
            .Take(4)
            .Select(record =>
                $"· {record.RelatedModule}：{SafeText(record.FinalDecision)}（采纳状态：{record.AdoptionStatus}）")
            .ToList();

        if (latestAiRecords.Count == 0)
        {
            return "本周暂无 AI 协作记录。建议后续在任务拆解、风险判断和复盘生成中记录 AI 建议与人工判断过程。";
        }

        return string.Join(Environment.NewLine, latestAiRecords);
    }

    private static string BuildManagerReviewText(
        IReadOnlyCollection<TaskItem> tasks,
        IReadOnlyCollection<MemberItem> members,
        IReadOnlyCollection<EquipmentItem> equipment,
        IReadOnlyCollection<AiRecordItem> aiRecords)
    {
        var riskTaskCount = tasks.Count(task =>
            task.RiskLevel == "高风险" ||
            task.Status == "延期" ||
            task.Status == "滞后");

        var attentionMemberCount = members.Count(member =>
            member.WorkloadStatus == "关注" ||
            member.WorkloadStatus == "过载");

        var abnormalEquipmentCount = equipment.Count(IsAbnormalEquipment);

        var aiRecordCount = aiRecords.Count;

        if (riskTaskCount == 0 && attentionMemberCount == 0 && abnormalEquipmentCount == 0)
        {
            return $"整体来看，本周团队运行状态较稳定。AI 协作记录共 {aiRecordCount} 条，后续可以继续沉淀关键决策过程，并保持任务、人员和器材数据及时更新。";
        }

        return
            $"本周需要重点关注：风险任务 {riskTaskCount} 项，成员负载异常 {attentionMemberCount} 项，器材异常 {abnormalEquipmentCount} 项。建议负责人优先处理会影响项目推进节奏的阻塞项，并在下周复盘中检查整改结果。AI 协作记录共 {aiRecordCount} 条，可作为辅助判断参考，但最终管理动作仍需负责人确认。";
    }

    private static bool IsAbnormalEquipment(EquipmentItem item)
    {
        return item.Status == "待检查" ||
               item.Status == "维修中" ||
               item.Status == "损坏" ||
               item.Status == "报废";
    }

    private static string SafeText(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "暂无" : text;
    }
}