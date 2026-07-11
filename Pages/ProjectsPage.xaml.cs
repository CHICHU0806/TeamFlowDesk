using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services.Ui;

namespace TeamFlowDesk.Pages;

public sealed partial class ProjectsPage : Page
{
    private readonly ObservableCollection<ProjectItem> _projects;

    public ProjectsPage()
    {
        InitializeComponent();

        ProjectRepository.SeedIfEmpty();
        _projects = new ObservableCollection<ProjectItem>(ProjectRepository.GetAll());

        ProjectStartDatePicker.Date = DateTimeOffset.Now;
        ProjectEndDatePicker.Date = DateTimeOffset.Now.AddDays(14);

        RefreshAll();
    }

    private void AddProjectButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ProjectNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ProjectFormMessageText.Text = "项目名称不能为空。";
            return;
        }

        var ownerName = ProjectOwnerTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            ProjectFormMessageText.Text = "项目负责人不能为空。";
            return;
        }

        var progressPercent = ParseProgressPercent(ProjectProgressTextBox.Text.Trim());

        var newProject = new ProjectItem
        {
            Name = name,
            Description = ProjectDescriptionTextBox.Text.Trim(),
            OwnerName = ownerName,
            Status = PageInteractionService.GetComboBoxText(
                ProjectStatusComboBox,
                "进行中"),
            CurrentStage = ProjectStageTextBox.Text.Trim(),
            RiskLevel = PageInteractionService.GetComboBoxText(
                ProjectRiskComboBox,
                "正常"),
            StartDate = ProjectStartDatePicker.Date,
            EndDate = ProjectEndDatePicker.Date,
            ProgressPercent = progressPercent
        };

        newProject.Id = ProjectRepository.Add(newProject);
        _projects.Insert(0, newProject);

        RefreshAll();
        ClearProjectForm();

        ProjectFormMessageText.Text = $"已新增项目：{newProject.Name}";
    }

    private void ClearProjectFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearProjectForm();
        ProjectFormMessageText.Text = "输入内容已清空。";
    }

    private void IncreaseProjectProgressButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var project = GetProjectFromButton(sender);

            if (project is null)
            {
                return;
            }

            var newProgress = Math.Min(project.ProgressPercent + 10, 100);
            var newStatus = newProgress >= 100 ? "已完成" : project.Status;
            var newRiskLevel = newProgress >= 100 ? "正常" : project.RiskLevel;

            var updatedProject = CopyProjectWithNewState(
                project,
                newStatus,
                newRiskLevel,
                newProgress);

            ProjectRepository.Update(updatedProject);

            PageInteractionService.ReplaceItem(
                _projects,
                project,
                updatedProject);

            RefreshAll();

            ProjectFormMessageText.Text = $"项目进度已推进：{updatedProject.Name}";
        });
    }

    private void SetProjectRunningButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateProjectState(sender, "进行中", "正常", null, "项目已标记为进行中");
    }

    private void SetProjectAttentionButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateProjectState(sender, "关注", "中风险", null, "项目已标记为关注");
    }

    private void SetProjectHighRiskButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateProjectState(sender, "滞后", "高风险", null, "项目已标记为高风险");
    }

    private void SetProjectCompletedButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateProjectState(sender, "已完成", "正常", 100, "项目已标记为完成");
    }

    private void DeleteProjectButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var project = GetProjectFromButton(sender);

            if (project is null)
            {
                return;
            }

            ProjectRepository.Delete(project.Id);
            _projects.Remove(project);

            RefreshAll();

            ProjectFormMessageText.Text = $"项目已删除：{project.Name}";
        });
    }

    private void UpdateProjectState(
        object sender,
        string status,
        string riskLevel,
        int? progressPercent,
        string message)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var project = GetProjectFromButton(sender);

            if (project is null)
            {
                return;
            }

            var updatedProject = CopyProjectWithNewState(
                project,
                status,
                riskLevel,
                progressPercent ?? project.ProgressPercent);

            ProjectRepository.Update(updatedProject);

            PageInteractionService.ReplaceItem(
                _projects,
                project,
                updatedProject);

            RefreshAll();

            ProjectFormMessageText.Text = $"{message}：{updatedProject.Name}";
        });
    }

    private ProjectItem? GetProjectFromButton(object sender)
    {
        return PageInteractionService.GetItemFromButton(
            sender,
            _projects,
            project => project.Id);
    }

    private static ProjectItem CopyProjectWithNewState(
        ProjectItem source,
        string status,
        string riskLevel,
        int progressPercent)
    {
        return new ProjectItem
        {
            Id = source.Id,
            Name = source.Name,
            Description = source.Description,
            OwnerName = source.OwnerName,
            Status = status,
            CurrentStage = source.CurrentStage,
            RiskLevel = riskLevel,
            StartDate = source.StartDate,
            EndDate = source.EndDate,
            ProgressPercent = Math.Clamp(progressPercent, 0, 100)
        };
    }

    private void RefreshAll()
    {
        RefreshStatistics();
        RefreshInsights();
        RefreshProjectCards();
    }

    private void RefreshStatistics()
    {
        ProjectCountText.Text = _projects.Count.ToString();

        RunningProjectCountText.Text = _projects
            .Count(project => project.Status == "进行中")
            .ToString();

        RiskProjectCountText.Text = _projects
            .Count(IsRiskProject)
            .ToString();

        var averageProgress = _projects.Count == 0
            ? 0
            : (int)Math.Round(_projects.Average(project => project.ProgressPercent));

        AverageProgressText.Text = $"{averageProgress}%";
    }

    private void RefreshInsights()
    {
        var runningCount = _projects.Count(project => project.Status == "进行中");
        var riskCount = _projects.Count(IsRiskProject);
        var completedCount = _projects.Count(project => project.Status == "已完成");
        var averageProgress = _projects.Count == 0
            ? 0
            : (int)Math.Round(_projects.Average(project => project.ProgressPercent));

        ProjectStatusSummaryText.Text =
            $"当前共有 {_projects.Count} 个项目，进行中 {runningCount} 个，已完成 {completedCount} 个，风险项目 {riskCount} 个，平均进度 {averageProgress}%。";

        if (_projects.Count == 0)
        {
            ProjectProgressInsightText.Text = "当前暂无项目记录，建议先建立一个主项目，再继续拆解任务和复盘。";
        }
        else if (averageProgress < 40)
        {
            ProjectProgressInsightText.Text = "当前项目平均进度偏低，建议检查是否需要重新拆解任务或明确阶段目标。";
        }
        else if (averageProgress < 80)
        {
            ProjectProgressInsightText.Text = "当前项目处于主要推进阶段，建议重点关注风险项目和输出物完成情况。";
        }
        else
        {
            ProjectProgressInsightText.Text = "当前项目整体处于收尾状态，建议核对输出物、复盘记录和交接资料是否完整。";
        }

        ProjectRiskInsightText.Text = riskCount == 0
            ? "当前暂无明显高风险项目，建议继续保持项目阶段和风险状态更新。"
            : $"当前有 {riskCount} 个风险项目，需要负责人优先确认阻塞点、资源需求和负责人状态。";

        var busiestOwner = _projects
            .Where(project => !string.IsNullOrWhiteSpace(project.OwnerName))
            .GroupBy(project => project.OwnerName)
            .Select(group => new
            {
                OwnerName = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .FirstOrDefault();

        ProjectOwnerInsightText.Text = busiestOwner is null
            ? "当前暂无负责人分布数据。"
            : $"当前负责项目最多的是 {busiestOwner.OwnerName}，关联项目 {busiestOwner.Count} 个。";
    }

    private void RefreshProjectCards()
    {
        ProjectsItemsControl.ItemsSource = _projects
            .OrderByDescending(GetProjectSortWeight)
            .ThenBy(project => project.ProgressPercent)
            .ThenBy(project => project.EndDate)
            .ToList();
    }

    private void ClearProjectForm()
    {
        ProjectNameTextBox.Text = string.Empty;
        ProjectOwnerTextBox.Text = string.Empty;
        ProjectStageTextBox.Text = string.Empty;
        ProjectProgressTextBox.Text = string.Empty;
        ProjectDescriptionTextBox.Text = string.Empty;

        ProjectStatusComboBox.SelectedIndex = 0;
        ProjectRiskComboBox.SelectedIndex = 0;
        ProjectStartDatePicker.Date = DateTimeOffset.Now;
        ProjectEndDatePicker.Date = DateTimeOffset.Now.AddDays(14);
    }

    private static int ParseProgressPercent(string text)
    {
        if (!int.TryParse(text, out var progressPercent))
        {
            return 0;
        }

        return Math.Clamp(progressPercent, 0, 100);
    }

    private static bool IsRiskProject(ProjectItem project)
    {
        return project.RiskLevel == "高风险" ||
               project.Status == "关注" ||
               project.Status == "滞后";
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
}
