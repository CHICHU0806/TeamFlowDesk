using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class ReportsPage : Page
{
    public ReportsPage()
    {
        InitializeComponent();
        LoadReports();
    }

    private void LoadReports()
    {
        var reports = MockDataService.GetWeeklyReports();

        ReportCountText.Text = reports.Count.ToString();
        NormalProgressCountText.Text = reports.Count(report => report.ProgressStatus == "正常").ToString();
        AiReportCountText.Text = reports.Count(report =>
            !string.IsNullOrWhiteSpace(report.AiCollaborationSummary)).ToString();

        ReportsListView.ItemsSource = reports;
    }
}