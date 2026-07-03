using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class ProjectsPage : Page
{
    public ProjectsPage()
    {
        InitializeComponent();
        LoadProjects();
    }

    private void LoadProjects()
    {
        var projects = MockDataService.GetProjects();

        ProjectCountText.Text = projects.Count.ToString();
        ActiveProjectCountText.Text = projects.Count(project => project.Status == "进行中").ToString();
        RiskProjectCountText.Text = projects.Count(project =>
            project.RiskLevel == "高风险" ||
            project.RiskLevel == "中风险").ToString();

        ProjectsListView.ItemsSource = projects;
    }
}