using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        LoadDashboardData();
    }

    private void LoadDashboardData()
    {
        TaskRepository.SeedIfEmpty();
        MemberRepository.SeedIfEmpty();
        EquipmentRepository.SeedIfEmpty();
        AiRecordRepository.SeedIfEmpty();

        var projects = MockDataService.GetProjects();
        var tasks = TaskRepository.GetAll();
        var members = MemberRepository.GetAll();
        var equipment = EquipmentRepository.GetAll();
        var aiRecords = AiRecordRepository.GetAll();

        var riskTasks = tasks
            .Where(task =>
                task.RiskLevel == "高风险" ||
                task.Status == "延期" ||
                task.Status == "滞后")
            .ToList();

        var attentionMembers = members
            .Where(member =>
                member.WorkloadStatus == "关注" ||
                member.WorkloadStatus == "过载")
            .ToList();

        var abnormalEquipment = equipment
            .Where(item =>
                item.Status == "待检查" ||
                item.Status == "损坏" ||
                item.Status == "维修中" ||
                item.Status == "报废")
            .ToList();

        var riskCount = riskTasks.Count + attentionMembers.Count + abnormalEquipment.Count;

        ProjectCountText.Text = projects.Count.ToString();
        TaskCountText.Text = tasks.Count.ToString();
        MemberCountText.Text = members.Count.ToString();
        EquipmentCountText.Text = equipment.Count.ToString();
        RiskCountText.Text = riskCount.ToString();
        AiRecordCountText.Text = aiRecords.Count.ToString();

        DashboardStatusText.Text = BuildDashboardStatusText(
            tasks.Count,
            members.Count,
            equipment.Count,
            riskCount,
            aiRecords.Count);

        DashboardSuggestionText.Text = BuildDashboardSuggestionText(riskCount);

        RiskListView.ItemsSource = BuildRiskItems(riskTasks, attentionMembers, abnormalEquipment);

        TasksListView.ItemsSource = tasks
            .Take(5)
            .ToList();

        MembersListView.ItemsSource = members
            .OrderByDescending(member => member.CurrentTaskCount)
            .Take(5)
            .ToList();

        EquipmentListView.ItemsSource = equipment
            .Take(5)
            .ToList();

        AiRecordsListView.ItemsSource = aiRecords
            .Take(4)
            .ToList();
    }

    private static string BuildDashboardStatusText(
        int taskCount,
        int memberCount,
        int equipmentCount,
        int riskCount,
        int aiRecordCount)
    {
        return
            $"当前系统已记录 {taskCount} 项任务、{memberCount} 名成员、{equipmentCount} 件器材，并沉淀 {aiRecordCount} 条 AI 协作记录。当前需要关注的风险项数量为 {riskCount}。";
    }

    private static string BuildDashboardSuggestionText(int riskCount)
    {
        if (riskCount == 0)
        {
            return "当前团队运行状态较平稳，可以继续推进任务闭环、周报复盘和项目阶段总结。";
        }

        if (riskCount <= 2)
        {
            return "当前存在少量风险项，建议负责人优先查看风险与阻塞雷达，并及时完成任务调整或资源协调。";
        }

        return "当前风险项较多，建议优先处理高风险任务、过载成员和异常器材，避免后续影响项目整体进度。";
    }

    private static List<RiskInsightItem> BuildRiskItems(
        IEnumerable<Models.TaskItem> riskTasks,
        IEnumerable<Models.MemberItem> attentionMembers,
        IEnumerable<Models.EquipmentItem> abnormalEquipment)
    {
        var items = new List<RiskInsightItem>();

        foreach (var task in riskTasks.Take(3))
        {
            items.Add(new RiskInsightItem
            {
                Title = $"任务风险：{task.Title}",
                Description = $"负责人：{task.OwnerName}；状态：{task.Status}；风险等级：{task.RiskLevel}。"
            });
        }

        foreach (var member in attentionMembers.Take(3))
        {
            items.Add(new RiskInsightItem
            {
                Title = $"人员负载：{member.Name}",
                Description = $"方向：{member.Direction}；当前任务数：{member.CurrentTaskCount}；负载状态：{member.WorkloadStatus}。"
            });
        }

        foreach (var item in abnormalEquipment.Take(3))
        {
            items.Add(new RiskInsightItem
            {
                Title = $"器材异常：{item.Name}",
                Description = $"编号：{item.Code}；状态：{item.Status}；位置：{item.Location}。"
            });
        }

        if (items.Count == 0)
        {
            items.Add(new RiskInsightItem
            {
                Title = "暂无明显风险项",
                Description = "当前任务、人员负载和器材状态未发现明显异常，可以继续按照既定计划推进。"
            });
        }

        return items;
    }

    private class RiskInsightItem
    {
        public string Title { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;
    }
}