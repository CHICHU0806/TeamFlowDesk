using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;

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
        try
        {
            ProjectRepository.SeedIfEmpty();
            TaskRepository.SeedIfEmpty();
            MemberRepository.SeedIfEmpty();
            EquipmentRepository.SeedIfEmpty();
            AiRecordRepository.SeedIfEmpty();
            WeeklyReportRepository.SeedIfEmpty();

            var projects = ProjectRepository.GetAll();
            var tasks = TaskRepository.GetAll();
            var members = MemberRepository.GetAll();
            var equipment = EquipmentRepository.GetAll();
            var aiRecords = AiRecordRepository.GetAll();
            var reports = WeeklyReportRepository.GetAll();

            RefreshMetrics(projects, tasks, members, equipment, aiRecords, reports);
            RefreshDashboardStatus(projects, tasks, members, equipment, aiRecords, reports);
            RefreshPriorityQueue(projects, tasks, members, equipment, reports);
            RefreshRunningProjects(projects);
            RefreshRecentAiRecords(aiRecords);
            RefreshRecentReports(reports);
        }
        catch (System.Exception ex)
        {
            DashboardStatusText.Text = $"驾驶舱数据加载失败：{ex.Message}";
            PrioritySuggestionText.Text = "请检查数据库初始化状态，或确认各 Repository 是否已经创建。";
        }
    }

    private void RefreshMetrics(
        IReadOnlyCollection<ProjectItem> projects,
        IReadOnlyCollection<TaskItem> tasks,
        IReadOnlyCollection<MemberItem> members,
        IReadOnlyCollection<EquipmentItem> equipment,
        IReadOnlyCollection<AiRecordItem> aiRecords,
        IReadOnlyCollection<WeeklyReportItem> reports)
    {
        var riskProjectCount = projects.Count(IsRiskProject);
        var riskTaskCount = tasks.Count(IsRiskTask);
        var attentionMemberCount = members.Count(IsAttentionMember);
        var abnormalEquipmentCount = equipment.Count(IsAbnormalEquipment);

        ProjectCountText.Text = projects.Count.ToString();
        RiskProjectCountText.Text = $"风险项目 {riskProjectCount} 个";

        TaskCountText.Text = tasks.Count.ToString();
        RiskTaskCountText.Text = $"风险任务 {riskTaskCount} 项";

        MemberCountText.Text = members.Count.ToString();
        AttentionMemberCountText.Text = $"需关注成员 {attentionMemberCount} 人";

        EquipmentCountText.Text = equipment.Count.ToString();
        AbnormalEquipmentCountText.Text = $"异常器材 {abnormalEquipmentCount} 件";

        AiRecordCountText.Text = aiRecords.Count.ToString();
        ReportCountText.Text = reports.Count.ToString();

        var closureScore = CalculateClosureScore(
            projects,
            tasks,
            members,
            equipment,
            aiRecords,
            reports);

        ClosureScoreText.Text = $"{closureScore}%";
        ClosureScoreDescriptionText.Text = closureScore >= 80
            ? "项目、任务、人员、器材、AI 与复盘链路较完整。"
            : "仍有模块数据不足，建议补齐项目、任务、AI 协作和周报复盘记录。";
    }

    private void RefreshDashboardStatus(
        IReadOnlyCollection<ProjectItem> projects,
        IReadOnlyCollection<TaskItem> tasks,
        IReadOnlyCollection<MemberItem> members,
        IReadOnlyCollection<EquipmentItem> equipment,
        IReadOnlyCollection<AiRecordItem> aiRecords,
        IReadOnlyCollection<WeeklyReportItem> reports)
    {
        var riskProjectCount = projects.Count(IsRiskProject);
        var riskTaskCount = tasks.Count(IsRiskTask);
        var attentionMemberCount = members.Count(IsAttentionMember);
        var abnormalEquipmentCount = equipment.Count(IsAbnormalEquipment);

        DashboardStatusText.Text =
            $"当前纳入管理的项目 {projects.Count} 个，任务 {tasks.Count} 项，成员 {members.Count} 人，器材 {equipment.Count} 件。已沉淀 AI 协作记录 {aiRecords.Count} 条，周报复盘记录 {reports.Count} 条。";

        if (riskProjectCount > 0)
        {
            PrioritySuggestionText.Text = $"当前最优先处理的是 {riskProjectCount} 个风险项目。建议先确认项目阻塞点，再回到任务、人员和器材层面拆解解决。";
            return;
        }

        if (riskTaskCount > 0)
        {
            PrioritySuggestionText.Text = $"当前最优先处理的是 {riskTaskCount} 项风险任务。建议检查负责人、截止时间、输出要求和关联器材是否明确。";
            return;
        }

        if (attentionMemberCount > 0)
        {
            PrioritySuggestionText.Text = $"当前最优先处理的是 {attentionMemberCount} 名需要关注的成员。建议检查任务负载是否过高或培养任务是否合理。";
            return;
        }

        if (abnormalEquipmentCount > 0)
        {
            PrioritySuggestionText.Text = $"当前最优先处理的是 {abnormalEquipmentCount} 件异常器材。建议确认是否影响正在推进的任务。";
            return;
        }

        if (reports.Count == 0)
        {
            PrioritySuggestionText.Text = "当前暂无周报复盘记录。建议生成一次复盘草稿，把近期任务、人员、器材和 AI 协作情况沉淀下来。";
            return;
        }

        PrioritySuggestionText.Text = "当前团队运行态势整体稳定。建议继续保持项目、任务、人员、器材和复盘数据的同步更新。";
    }

    private void RefreshPriorityQueue(
        IReadOnlyCollection<ProjectItem> projects,
        IReadOnlyCollection<TaskItem> tasks,
        IReadOnlyCollection<MemberItem> members,
        IReadOnlyCollection<EquipmentItem> equipment,
        IReadOnlyCollection<WeeklyReportItem> reports)
    {
        var priorityItems = new List<DashboardPriorityItem>();

        priorityItems.AddRange(projects
            .Where(IsRiskProject)
            .Take(4)
            .Select(project => new DashboardPriorityItem
            {
                Title = project.Name,
                Source = "项目",
                Severity = project.RiskLevel,
                Description = $"项目状态：{project.Status}；当前阶段：{SafeText(project.CurrentStage)}；负责人：{SafeText(project.OwnerName)}。"
            }));

        priorityItems.AddRange(tasks
            .Where(IsRiskTask)
            .Take(4)
            .Select(task => new DashboardPriorityItem
            {
                Title = task.Title,
                Source = "任务",
                Severity = task.RiskLevel,
                Description = $"任务状态：{task.Status}；负责人：{SafeText(task.OwnerName)}；输出要求：{SafeText(task.OutputRequirement)}。"
            }));

        priorityItems.AddRange(members
            .Where(IsAttentionMember)
            .Take(4)
            .Select(member => new DashboardPriorityItem
            {
                Title = member.Name,
                Source = "成员",
                Severity = member.WorkloadStatus,
                Description = $"方向：{SafeText(member.Direction)}；当前任务数：{member.CurrentTaskCount}；培养计划：{SafeText(member.GrowthPlan)}。"
            }));

        priorityItems.AddRange(equipment
            .Where(IsAbnormalEquipment)
            .Take(4)
            .Select(item => new DashboardPriorityItem
            {
                Title = item.Name,
                Source = "器材",
                Severity = item.Status,
                Description = $"分类：{SafeText(item.Category)}；位置：{SafeText(item.Location)}；关联任务：{SafeText(item.RelatedTask)}。"
            }));

        if (reports.Count == 0)
        {
            priorityItems.Add(new DashboardPriorityItem
            {
                Title = "尚未形成周报复盘",
                Source = "复盘",
                Severity = "建议处理",
                Description = "当前系统中没有周报复盘记录，建议先生成一条复盘草稿，沉淀近期团队运行过程。"
            });
        }

        if (priorityItems.Count == 0)
        {
            priorityItems.Add(new DashboardPriorityItem
            {
                Title = "当前暂无明显阻塞项",
                Source = "系统",
                Severity = "稳定",
                Description = "项目、任务、人员和器材没有明显高风险项。建议继续保持数据更新，并按周形成复盘记录。"
            });
        }

        PriorityItemsControl.ItemsSource = priorityItems.Take(8).ToList();
    }

    private void RefreshRunningProjects(IReadOnlyCollection<ProjectItem> projects)
    {
        var runningProjects = projects
            .Where(project => project.Status != "已完成")
            .OrderByDescending(GetProjectSortWeight)
            .ThenBy(project => project.ProgressPercent)
            .Take(5)
            .ToList();

        if (runningProjects.Count == 0 && projects.Count > 0)
        {
            runningProjects = projects
                .OrderByDescending(project => project.EndDate)
                .Take(5)
                .ToList();
        }

        RunningProjectsItemsControl.ItemsSource = runningProjects;
    }

    private void RefreshRecentAiRecords(IReadOnlyCollection<AiRecordItem> aiRecords)
    {
        var items = aiRecords
            .OrderByDescending(record => record.CreatedAt)
            .Take(5)
            .Select(record => new DashboardActivityItem
            {
                Title = record.RelatedModule,
                Tag = record.AdoptionStatus,
                Description = string.IsNullOrWhiteSpace(record.FinalDecision)
                    ? SafeText(record.Question)
                    : record.FinalDecision
            })
            .ToList();

        if (items.Count == 0)
        {
            items.Add(new DashboardActivityItem
            {
                Title = "暂无 AI 协作记录",
                Tag = "AI",
                Description = "建议在任务拆解、风险判断或周报复盘时记录 AI 建议、人工判断和最终决策。"
            });
        }

        RecentAiRecordsItemsControl.ItemsSource = items;
    }

    private void RefreshRecentReports(IReadOnlyCollection<WeeklyReportItem> reports)
    {
        var items = reports
            .OrderByDescending(report => report.EndDate)
            .Take(5)
            .Select(report => new DashboardActivityItem
            {
                Title = report.Title,
                Tag = report.ProgressStatus,
                Description = string.IsNullOrWhiteSpace(report.ManagerReview)
                    ? SafeText(report.CompletedWork)
                    : report.ManagerReview
            })
            .ToList();

        if (items.Count == 0)
        {
            items.Add(new DashboardActivityItem
            {
                Title = "暂无周报复盘记录",
                Tag = "复盘",
                Description = "建议每周至少形成一条复盘记录，用于交接和团队管理沉淀。"
            });
        }

        RecentReportsItemsControl.ItemsSource = items;
    }

    private static int CalculateClosureScore(
        IReadOnlyCollection<ProjectItem> projects,
        IReadOnlyCollection<TaskItem> tasks,
        IReadOnlyCollection<MemberItem> members,
        IReadOnlyCollection<EquipmentItem> equipment,
        IReadOnlyCollection<AiRecordItem> aiRecords,
        IReadOnlyCollection<WeeklyReportItem> reports)
    {
        var score = 0;

        if (projects.Count > 0)
        {
            score += 18;
        }

        if (tasks.Count > 0)
        {
            score += 18;
        }

        if (members.Count > 0)
        {
            score += 16;
        }

        if (equipment.Count > 0)
        {
            score += 16;
        }

        if (aiRecords.Count > 0)
        {
            score += 16;
        }

        if (reports.Count > 0)
        {
            score += 16;
        }

        return score;
    }

    private static bool IsRiskProject(ProjectItem project)
    {
        return project.RiskLevel == "高风险" ||
               project.Status == "关注" ||
               project.Status == "滞后";
    }

    private static bool IsRiskTask(TaskItem task)
    {
        return task.RiskLevel == "高风险" ||
               task.Status == "延期" ||
               task.Status == "滞后";
    }

    private static bool IsAttentionMember(MemberItem member)
    {
        return member.WorkloadStatus == "关注" ||
               member.WorkloadStatus == "过载";
    }

    private static bool IsAbnormalEquipment(EquipmentItem item)
    {
        return item.Status == "待检查" ||
               item.Status == "维修中" ||
               item.Status == "损坏" ||
               item.Status == "报废";
    }

    private static int GetProjectSortWeight(ProjectItem project)
    {
        if (project.RiskLevel == "高风险")
        {
            return 4;
        }

        return project.Status switch
        {
            "滞后" => 4,
            "关注" => 3,
            "进行中" => 2,
            "已完成" => 1,
            _ => 0
        };
    }

    private static string SafeText(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? "暂无" : text;
    }

    public class DashboardPriorityItem
    {
        public string Title { get; set; } = string.Empty;

        public string Source { get; set; } = string.Empty;

        public string Severity { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public class DashboardActivityItem
    {
        public string Title { get; set; } = string.Empty;

        public string Tag { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
