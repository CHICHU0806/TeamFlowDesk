using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;

namespace TeamFlowDesk.Pages;

public sealed partial class EquipmentPage : Page
{
    private readonly ObservableCollection<EquipmentItem> _equipment;

    public EquipmentPage()
    {
        InitializeComponent();

        EquipmentRepository.SeedIfEmpty();
        _equipment = new ObservableCollection<EquipmentItem>(EquipmentRepository.GetAll());

        EquipmentListView.ItemsSource = _equipment;

        RefreshStatistics();
    }

    private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        var name = EquipmentNameTextBox.Text.Trim();
        var code = EquipmentCodeTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            EquipmentFormMessageText.Text = "器材名称不能为空。";
            return;
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            EquipmentFormMessageText.Text = "器材编号不能为空。";
            return;
        }

        var newEquipment = new EquipmentItem
        {
            Name = name,
            Code = code,
            Category = EquipmentCategoryTextBox.Text.Trim(),
            Status = GetComboBoxText(EquipmentStatusComboBox, "可用"),
            Location = EquipmentLocationTextBox.Text.Trim(),
            CurrentHolder = EquipmentHolderTextBox.Text.Trim(),
            RelatedTask = EquipmentRelatedTaskTextBox.Text.Trim(),
            MaintenanceRecord = EquipmentMaintenanceTextBox.Text.Trim()
        };

        newEquipment.Id = EquipmentRepository.Add(newEquipment);
        _equipment.Insert(0, newEquipment);

        RefreshStatistics();
        ClearEquipmentForm();

        EquipmentFormMessageText.Text = $"已新增器材：{newEquipment.Name}";
    }

    private void ClearEquipmentFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearEquipmentForm();
        EquipmentFormMessageText.Text = "输入内容已清空。";
    }

    private void SetAvailableButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "可用", "器材已标记为可用");
    }

    private void SetNeedCheckButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "待检查", "器材已标记为待检查");
    }

    private void SetUsingButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "使用中", "器材已标记为使用中");
    }

    private void DeleteEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return;
        }

        if (!int.TryParse(button.Tag.ToString(), out var equipmentId))
        {
            return;
        }

        var item = _equipment.FirstOrDefault(equipment => equipment.Id == equipmentId);

        if (item is null)
        {
            return;
        }

        EquipmentRepository.Delete(item.Id);
        _equipment.Remove(item);

        RefreshStatistics();

        EquipmentFormMessageText.Text = $"器材已删除：{item.Name}";
    }

    private void UpdateEquipmentStatus(object sender, string status, string message)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return;
        }

        if (!int.TryParse(button.Tag.ToString(), out var equipmentId))
        {
            return;
        }

        var item = _equipment.FirstOrDefault(equipment => equipment.Id == equipmentId);

        if (item is null)
        {
            return;
        }

        item.Status = status;
        EquipmentRepository.Update(item);

        EquipmentListView.ItemsSource = null;
        EquipmentListView.ItemsSource = _equipment;

        RefreshStatistics();

        EquipmentFormMessageText.Text = $"{message}：{item.Name}";
    }

    private void RefreshStatistics()
    {
        EquipmentCountText.Text = _equipment.Count.ToString();

        AvailableEquipmentCountText.Text = _equipment
            .Count(item => item.Status == "可用")
            .ToString();

        UsingEquipmentCountText.Text = _equipment
            .Count(item => item.Status == "使用中" || item.Status == "借出")
            .ToString();

        AbnormalEquipmentCountText.Text = _equipment
            .Count(item =>
                item.Status == "待检查" ||
                item.Status == "损坏" ||
                item.Status == "维修中" ||
                item.Status == "报废")
            .ToString();
    }

    private void ClearEquipmentForm()
    {
        EquipmentNameTextBox.Text = string.Empty;
        EquipmentCodeTextBox.Text = string.Empty;
        EquipmentCategoryTextBox.Text = string.Empty;
        EquipmentLocationTextBox.Text = string.Empty;
        EquipmentHolderTextBox.Text = string.Empty;
        EquipmentRelatedTaskTextBox.Text = string.Empty;
        EquipmentMaintenanceTextBox.Text = string.Empty;

        EquipmentStatusComboBox.SelectedIndex = 0;
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