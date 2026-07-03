using System.Linq;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Services;

namespace TeamFlowDesk.Pages;

public sealed partial class EquipmentPage : Page
{
    public EquipmentPage()
    {
        InitializeComponent();
        LoadEquipment();
    }

    private void LoadEquipment()
    {
        var equipment = MockDataService.GetEquipment();

        EquipmentCountText.Text = equipment.Count.ToString();
        AvailableEquipmentCountText.Text = equipment.Count(item => item.Status == "可用").ToString();
        UsingEquipmentCountText.Text = equipment.Count(item => item.Status == "使用中").ToString();
        AbnormalEquipmentCountText.Text = equipment.Count(item =>
            item.Status == "待检查" ||
            item.Status == "损坏" ||
            item.Status == "维修中").ToString();

        EquipmentListView.ItemsSource = equipment;
    }
}