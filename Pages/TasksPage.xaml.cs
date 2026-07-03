using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class TasksPage : Page
{
    public TasksPage()
    {
        InitializeComponent();
        LoadTasks();
    }

    private void LoadTasks()
    {
        var tasks = MockDataService.GetTasks();

        TaskCountText.Text = tasks.Count.ToString();
        CompletedTaskCountText.Text = tasks.Count(task => task.Status == "已完成").ToString();
        DoingTaskCountText.Text = tasks.Count(task => task.Status == "进行中").ToString();
        RiskTaskCountText.Text = tasks.Count(task =>
            task.RiskLevel == "高风险" ||
            task.Status == "延期" ||
            task.Status == "滞后").ToString();

        TasksListView.ItemsSource = tasks;
    }
}