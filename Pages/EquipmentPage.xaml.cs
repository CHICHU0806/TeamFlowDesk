using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services.Ui;

namespace TeamFlowDesk.Pages;

public sealed partial class EquipmentPage : Page
{
    private readonly ObservableCollection<EquipmentItem> _equipment;

    public EquipmentPage()
    {
        InitializeComponent();

        EquipmentRepository.SeedIfEmpty();
        _equipment = new ObservableCollection<EquipmentItem>(EquipmentRepository.GetAll());

        RefreshAll();
    }

    private void AddEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        var name = EquipmentNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            EquipmentFormMessageText.Text = "器材名称不能为空。";
            return;
        }

        var newEquipment = new EquipmentItem
        {
            Name = name,
            Code = EquipmentCodeTextBox.Text.Trim(),
            Category = EquipmentCategoryTextBox.Text.Trim(),
            Status = PageInteractionService.GetComboBoxText(
                EquipmentStatusComboBox,
                "可用"),
            Location = EquipmentLocationTextBox.Text.Trim(),
            CurrentHolder = EquipmentHolderTextBox.Text.Trim(),
            RelatedTask = EquipmentRelatedTaskTextBox.Text.Trim(),
            MaintenanceRecord = EquipmentMaintenanceRecordTextBox.Text.Trim()
        };

        newEquipment.Id = EquipmentRepository.Add(newEquipment);
        _equipment.Insert(0, newEquipment);

        RefreshAll();
        ClearEquipmentForm();

        EquipmentFormMessageText.Text = $"已新增器材：{newEquipment.Name}";
    }

    private void ClearEquipmentFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearEquipmentForm();
        EquipmentFormMessageText.Text = "输入内容已清空。";
    }

    private void SetAvailableEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "可用", "器材已标记为可用");
    }

    private void SetUsingEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "使用中", "器材已标记为使用中");
    }

    private void SetCheckingEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "待检查", "器材已标记为待检查");
    }

    private void SetMaintenanceEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "维修中", "器材已标记为维修中");
    }

    private void SetBrokenEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateEquipmentStatus(sender, "损坏", "器材已标记为损坏");
    }

    private void DeleteEquipmentButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var equipment = GetEquipmentFromButton(sender);

            if (equipment is null)
            {
                return;
            }

            EquipmentRepository.Delete(equipment.Id);
            _equipment.Remove(equipment);

            RefreshAll();

            EquipmentFormMessageText.Text = $"器材已删除：{equipment.Name}";
        });
    }

    private void UpdateEquipmentStatus(
        object sender,
        string status,
        string message)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var equipment = GetEquipmentFromButton(sender);

            if (equipment is null)
            {
                return;
            }

            var updatedEquipment = CopyEquipmentWithNewStatus(
                equipment,
                status);

            EquipmentRepository.Update(updatedEquipment);

            PageInteractionService.ReplaceItem(
                _equipment,
                equipment,
                updatedEquipment);

            RefreshAll();

            EquipmentFormMessageText.Text = $"{message}：{updatedEquipment.Name}";
        });
    }

    private EquipmentItem? GetEquipmentFromButton(object sender)
    {
        return PageInteractionService.GetItemFromButton(
            sender,
            _equipment,
            item => item.Id);
    }

    private static EquipmentItem CopyEquipmentWithNewStatus(
        EquipmentItem source,
        string status)
    {
        return new EquipmentItem
        {
            Id = source.Id,
            Name = source.Name,
            Code = source.Code,
            Category = source.Category,
            Status = status,
            Location = source.Location,
            CurrentHolder = source.CurrentHolder,
            RelatedTask = source.RelatedTask,
            MaintenanceRecord = source.MaintenanceRecord
        };
    }

    private void RefreshAll()
    {
        RefreshStatistics();
        RefreshInsights();
        RefreshEquipmentCards();
    }

    private void RefreshStatistics()
    {
        EquipmentCountText.Text = _equipment.Count.ToString();

        AvailableEquipmentCountText.Text = _equipment
            .Count(item => item.Status == "可用")
            .ToString();

        UsingEquipmentCountText.Text = _equipment
            .Count(item => item.Status == "使用中")
            .ToString();

        AbnormalEquipmentCountText.Text = _equipment
            .Count(IsAbnormalEquipment)
            .ToString();
    }

    private void RefreshInsights()
    {
        var availableCount = _equipment.Count(item => item.Status == "可用");
        var usingCount = _equipment.Count(item => item.Status == "使用中");
        var abnormalCount = _equipment.Count(IsAbnormalEquipment);

        EquipmentStatusSummaryText.Text =
            $"当前共有 {_equipment.Count} 件器材，其中可用 {availableCount} 件，使用中 {usingCount} 件，异常或待处理 {abnormalCount} 件。";

        if (abnormalCount > 0)
        {
            EquipmentRiskInsightText.Text = $"当前存在 {abnormalCount} 件待检查、维修中或损坏器材，建议优先确认是否影响正在推进的任务。";
        }
        else if (availableCount == 0 && _equipment.Count > 0)
        {
            EquipmentRiskInsightText.Text = "当前暂无可用器材，建议检查是否存在资源占用过高或状态未及时更新的问题。";
        }
        else
        {
            EquipmentRiskInsightText.Text = "当前器材风险整体可控，建议继续保持使用状态和维护记录更新。";
        }

        if (usingCount > availableCount && _equipment.Count > 0)
        {
            EquipmentUsageInsightText.Text = "当前使用中器材数量较多，安排新任务前请确认资源是否冲突。";
        }
        else if (usingCount == 0)
        {
            EquipmentUsageInsightText.Text = "当前暂无使用中的器材记录，可以检查任务和器材是否已经建立关联。";
        }
        else
        {
            EquipmentUsageInsightText.Text = $"当前有 {usingCount} 件器材处于使用中，建议关注其对应任务是否按期归还或释放资源。";
        }

        var missingMaintenanceCount = _equipment.Count(item =>
            string.IsNullOrWhiteSpace(item.MaintenanceRecord));

        EquipmentMaintenanceInsightText.Text = missingMaintenanceCount == 0
            ? "当前器材均有维护记录，可持续记录使用异常、维修过程和归还检查情况。"
            : $"当前有 {missingMaintenanceCount} 件器材缺少维护记录，建议补充检查结果、使用注意事项或历史异常。";
    }

    private void RefreshEquipmentCards()
    {
        EquipmentItemsControl.ItemsSource = _equipment
            .OrderByDescending(GetEquipmentSortWeight)
            .ThenBy(item => item.Category)
            .ThenBy(item => item.Name)
            .ToList();
    }

    private void ClearEquipmentForm()
    {
        EquipmentNameTextBox.Text = string.Empty;
        EquipmentCodeTextBox.Text = string.Empty;
        EquipmentCategoryTextBox.Text = string.Empty;
        EquipmentLocationTextBox.Text = string.Empty;
        EquipmentHolderTextBox.Text = string.Empty;
        EquipmentRelatedTaskTextBox.Text = string.Empty;
        EquipmentMaintenanceRecordTextBox.Text = string.Empty;

        EquipmentStatusComboBox.SelectedIndex = 0;
    }

    private static bool IsAbnormalEquipment(EquipmentItem item)
    {
        return item.Status == "待检查" ||
               item.Status == "维修中" ||
               item.Status == "损坏" ||
               item.Status == "报废";
    }

    private static int GetEquipmentSortWeight(EquipmentItem item)
    {
        return item.Status switch
        {
            "损坏" => 5,
            "维修中" => 4,
            "待检查" => 3,
            "使用中" => 2,
            "可用" => 1,
            _ => 0
        };
    }
}
