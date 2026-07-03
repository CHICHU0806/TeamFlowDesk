using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class AiRecordsPage : Page
{
    public AiRecordsPage()
    {
        InitializeComponent();
        LoadAiRecords();
    }

    private void LoadAiRecords()
    {
        var records = MockDataService.GetAiRecords();

        RecordCountText.Text = records.Count.ToString();
        AcceptedCountText.Text = records.Count(record => record.AdoptionStatus == "采纳").ToString();
        PartAcceptedCountText.Text = records.Count(record => record.AdoptionStatus == "部分采纳").ToString();
        ModuleCountText.Text = records
            .Select(record => record.RelatedModule)
            .Distinct()
            .Count()
            .ToString();

        AiRecordsListView.ItemsSource = records;
    }
}