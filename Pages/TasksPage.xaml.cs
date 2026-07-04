using System;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services.Ui;

namespace TeamFlowDesk.Pages;

public sealed partial class TasksPage : Page
{
    private readonly ObservableCollection<TaskItem> _tasks;

    public TasksPage()
    {
        InitializeComponent();

        TaskRepository.SeedIfEmpty();
        _tasks = new ObservableCollection<TaskItem>(TaskRepository.GetAll());

        TaskDeadlineDatePicker.Date = DateTimeOffset.Now.AddDays(3);

        RefreshAll();
    }

    private void AddTaskButton_Click(object sender, RoutedEventArgs e)
    {
        var title = TaskTitleTextBox.Text.Trim();
        var ownerName = TaskOwnerTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            TaskFormMessageText.Text = "任务名称不能为空。";
            return;
        }

        if (string.IsNullOrWhiteSpace(ownerName))
        {
            TaskFormMessageText.Text = "负责人不能为空。";
            return;
        }

        var newTask = new TaskItem
        {
            ProjectId = 1,
            Title = title,
            Description = TaskDescriptionTextBox.Text.Trim(),
            OwnerName = ownerName,
            Collaborators = "暂无",
            Status = PageInteractionService.GetComboBoxText(
                TaskStatusComboBox,
                "待处理"),
            Priority = PageInteractionService.GetComboBoxText(
                TaskPriorityComboBox,
                "普通"),
            RiskLevel = PageInteractionService.GetComboBoxText(
                TaskRiskComboBox,
                "正常"),
            Deadline = TaskDeadlineDatePicker.Date,
            RelatedEquipment = TaskEquipmentTextBox.Text.Trim(),
            OutputRequirement = TaskOutputTextBox.Text.Trim()
        };

        newTask.Id = TaskRepository.Add(newTask);
        _tasks.Insert(0, newTask);

        RefreshAll();
        ClearTaskForm();

        TaskFormMessageText.Text = $"已新增任务：{newTask.Title}";
    }

    private void ClearTaskFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearTaskForm();
        TaskFormMessageText.Text = "输入内容已清空。";
    }

    private void SetDoingTaskButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateTask(sender, "进行中", null, "任务已标记为进行中");
    }

    private void CompleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateTask(sender, "已完成", "正常", "任务已标记完成");
    }

    private void SetHighRiskTaskButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateTask(sender, null, "高风险", "任务已标记为高风险");
    }

    private void DeleteTaskButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var task = GetTaskFromButton(sender);

            if (task is null)
            {
                return;
            }

            TaskRepository.Delete(task.Id);
            _tasks.Remove(task);

            RefreshAll();

            TaskFormMessageText.Text = $"任务已删除：{task.Title}";
        });
    }

    private void UpdateTask(
        object sender,
        string? status,
        string? riskLevel,
        string message)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var task = GetTaskFromButton(sender);

            if (task is null)
            {
                return;
            }

            var updatedTask = CopyTaskWithNewState(
                task,
                status,
                riskLevel);

            TaskRepository.Update(updatedTask);

            PageInteractionService.ReplaceItem(
                _tasks,
                task,
                updatedTask);

            RefreshAll();

            TaskFormMessageText.Text = $"{message}：{updatedTask.Title}";
        });
    }

    private TaskItem? GetTaskFromButton(object sender)
    {
        return PageInteractionService.GetItemFromButton(
            sender,
            _tasks,
            task => task.Id);
    }

    private static TaskItem CopyTaskWithNewState(
        TaskItem source,
        string? status,
        string? riskLevel)
    {
        return new TaskItem
        {
            Id = source.Id,
            ProjectId = source.ProjectId,
            Title = source.Title,
            Description = source.Description,
            OwnerName = source.OwnerName,
            Collaborators = source.Collaborators,
            Status = string.IsNullOrWhiteSpace(status)
                ? source.Status
                : status,
            Priority = source.Priority,
            RiskLevel = string.IsNullOrWhiteSpace(riskLevel)
                ? source.RiskLevel
                : riskLevel,
            Deadline = source.Deadline,
            RelatedEquipment = source.RelatedEquipment,
            OutputRequirement = source.OutputRequirement
        };
    }

    private void RefreshAll()
    {
        RefreshStatistics();
        RefreshBoards();
        RefreshInsights();
    }

    private void RefreshStatistics()
    {
        TaskCountText.Text = _tasks.Count.ToString();

        CompletedTaskCountText.Text = _tasks
            .Count(task => task.Status == "已完成")
            .ToString();

        DoingTaskCountText.Text = _tasks
            .Count(task => task.Status == "进行中")
            .ToString();

        RiskTaskCountText.Text = _tasks
            .Count(IsRiskTask)
            .ToString();
    }

    private void RefreshBoards()
    {
        TodoTasksItemsControl.ItemsSource = _tasks
            .Where(task =>
                task.Status == "待处理" &&
                !IsRiskTask(task))
            .ToList();

        DoingTasksItemsControl.ItemsSource = _tasks
            .Where(task =>
                task.Status == "进行中" &&
                !IsRiskTask(task))
            .ToList();

        DoneTasksItemsControl.ItemsSource = _tasks
            .Where(task => task.Status == "已完成")
            .ToList();

        RiskTasksItemsControl.ItemsSource = _tasks
            .Where(IsRiskTask)
            .ToList();
    }

    private void RefreshInsights()
    {
        var riskCount = _tasks.Count(IsRiskTask);
        var doingCount = _tasks.Count(task => task.Status == "进行中");
        var todoCount = _tasks.Count(task => task.Status == "待处理");

        TaskStatusSummaryText.Text =
            $"当前共有 {_tasks.Count} 项任务，其中进行中 {doingCount} 项，待处理 {todoCount} 项，风险任务 {riskCount} 项。";

        if (riskCount > 0)
        {
            TaskPriorityInsightText.Text = "当前存在风险任务，建议优先处理高风险、延期或滞后的任务，再继续推进普通任务。";
        }
        else if (doingCount > 0)
        {
            TaskPriorityInsightText.Text = "当前任务推进较平稳，可以优先检查进行中任务的输出要求和截止时间。";
        }
        else
        {
            TaskPriorityInsightText.Text = "当前缺少进行中任务，建议负责人尽快明确下一阶段任务拆解。";
        }

        var busiestOwner = _tasks
            .Where(task => !string.IsNullOrWhiteSpace(task.OwnerName))
            .GroupBy(task => task.OwnerName)
            .Select(group => new
            {
                OwnerName = group.Key,
                Count = group.Count()
            })
            .OrderByDescending(item => item.Count)
            .FirstOrDefault();

        TaskOwnerInsightText.Text = busiestOwner is null
            ? "当前暂无负责人分布数据。"
            : $"当前任务最多的负责人是 {busiestOwner.OwnerName}，关联任务 {busiestOwner.Count} 项。";
    }

    private void ClearTaskForm()
    {
        TaskTitleTextBox.Text = string.Empty;
        TaskOwnerTextBox.Text = string.Empty;
        TaskDescriptionTextBox.Text = string.Empty;
        TaskEquipmentTextBox.Text = string.Empty;
        TaskOutputTextBox.Text = string.Empty;

        TaskStatusComboBox.SelectedIndex = 0;
        TaskPriorityComboBox.SelectedIndex = 1;
        TaskRiskComboBox.SelectedIndex = 0;
        TaskDeadlineDatePicker.Date = DateTimeOffset.Now.AddDays(3);
    }

    private static bool IsRiskTask(TaskItem task)
    {
        return task.RiskLevel == "高风险" ||
               task.Status == "延期" ||
               task.Status == "滞后";
    }
}