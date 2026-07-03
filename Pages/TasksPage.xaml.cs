using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class TasksPage : Page
{
    private readonly ObservableCollection<TaskItem> _tasks;

    public TasksPage()
    {
        InitializeComponent();

        _tasks = new ObservableCollection<TaskItem>(MockDataService.GetTasks());
        TasksListView.ItemsSource = _tasks;

        TaskDeadlineDatePicker.Date = DateTimeOffset.Now.AddDays(3);

        RefreshStatistics();
    }

    private void AddTaskButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
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
            Id = _tasks.Count == 0 ? 1 : _tasks.Max(task => task.Id) + 1,
            ProjectId = 1,
            Title = title,
            Description = TaskDescriptionTextBox.Text.Trim(),
            OwnerName = ownerName,
            Collaborators = "暂无",
            Status = GetComboBoxText(TaskStatusComboBox, "待处理"),
            Priority = GetComboBoxText(TaskPriorityComboBox, "普通"),
            RiskLevel = GetComboBoxText(TaskRiskComboBox, "正常"),
            Deadline = TaskDeadlineDatePicker.Date,
            RelatedEquipment = TaskEquipmentTextBox.Text.Trim(),
            OutputRequirement = TaskOutputTextBox.Text.Trim()
        };

        _tasks.Add(newTask);

        RefreshStatistics();
        ClearTaskForm();

        TaskFormMessageText.Text = $"已新增任务：{newTask.Title}";
    }

    private void ClearTaskFormButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ClearTaskForm();
        TaskFormMessageText.Text = "输入内容已清空。";
    }

    private void CompleteTaskButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return;
        }

        if (!int.TryParse(button.Tag.ToString(), out var taskId))
        {
            return;
        }

        var task = _tasks.FirstOrDefault(item => item.Id == taskId);

        if (task is null)
        {
            return;
        }

        task.Status = "已完成";
        task.RiskLevel = "正常";

        TasksListView.ItemsSource = null;
        TasksListView.ItemsSource = _tasks;

        RefreshStatistics();

        TaskFormMessageText.Text = $"任务已标记完成：{task.Title}";
    }

    private void DeleteTaskButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return;
        }

        if (!int.TryParse(button.Tag.ToString(), out var taskId))
        {
            return;
        }

        var task = _tasks.FirstOrDefault(item => item.Id == taskId);

        if (task is null)
        {
            return;
        }

        _tasks.Remove(task);

        RefreshStatistics();

        TaskFormMessageText.Text = $"任务已删除：{task.Title}";
    }

    private void RefreshStatistics()
    {
        TaskCountText.Text = _tasks.Count.ToString();
        CompletedTaskCountText.Text = _tasks.Count(task => task.Status == "已完成").ToString();
        DoingTaskCountText.Text = _tasks.Count(task => task.Status == "进行中").ToString();
        RiskTaskCountText.Text = _tasks.Count(task =>
            task.RiskLevel == "高风险" ||
            task.Status == "延期" ||
            task.Status == "滞后").ToString();
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