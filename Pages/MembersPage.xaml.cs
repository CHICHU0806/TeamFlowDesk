using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class MembersPage : Page
{
    public MembersPage()
    {
        InitializeComponent();
        LoadMembers();
    }

    private void LoadMembers()
    {
        var members = MockDataService.GetMembers();

        MemberCountText.Text = members.Count.ToString();
        NormalWorkloadCountText.Text = members.Count(member => member.WorkloadStatus == "正常").ToString();
        IndependentCountText.Text = members.Count(member => member.AbilityLevel == "可独立完成").ToString();
        TotalTaskLoadText.Text = members.Sum(member => member.CurrentTaskCount).ToString();

        MembersListView.ItemsSource = members;
    }
}