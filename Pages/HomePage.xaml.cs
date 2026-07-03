using System.Linq;
using Microsoft.UI.Xaml.Controls;
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
        var projects = MockDataService.GetProjects();
        var tasks = MockDataService.GetTasks();
        var members = MockDataService.GetMembers();
        var equipment = MockDataService.GetEquipment();
        var aiRecords = MockDataService.GetAiRecords();

        ProjectCountText.Text = projects.Count.ToString();
        TaskCountText.Text = tasks.Count.ToString();
        RiskTaskCountText.Text = tasks.Count(task =>
            task.RiskLevel == "高风险" ||
            task.Status == "延期" ||
            task.Status == "滞后").ToString();
        AiRecordCountText.Text = aiRecords.Count.ToString();

        ProjectsListView.ItemsSource = projects;
        TasksListView.ItemsSource = tasks;
        MembersListView.ItemsSource = members;
        EquipmentListView.ItemsSource = equipment;
        AiRecordsListView.ItemsSource = aiRecords;
    }
}