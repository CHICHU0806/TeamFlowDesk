using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;

namespace TeamFlowDesk.Pages;

public sealed partial class MembersPage : Page
{
    private readonly ObservableCollection<MemberItem> _members;

    public MembersPage()
    {
        InitializeComponent();

        MemberRepository.SeedIfEmpty();
        _members = new ObservableCollection<MemberItem>(MemberRepository.GetAll());

        MembersListView.ItemsSource = _members;

        RefreshStatistics();
    }

    private void AddMemberButton_Click(object sender, RoutedEventArgs e)
    {
        var name = MemberNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            MemberFormMessageText.Text = "成员姓名不能为空。";
            return;
        }

        var taskCountText = MemberTaskCountTextBox.Text.Trim();

        if (!int.TryParse(taskCountText, out var taskCount))
        {
            taskCount = 0;
        }

        if (taskCount < 0)
        {
            taskCount = 0;
        }

        var newMember = new MemberItem
        {
            Name = name,
            Grade = MemberGradeTextBox.Text.Trim(),
            Direction = MemberDirectionTextBox.Text.Trim(),
            Role = MemberRoleTextBox.Text.Trim(),
            SkillTags = MemberSkillTagsTextBox.Text.Trim(),
            AbilityLevel = GetComboBoxText(MemberAbilityComboBox, "入门"),
            CurrentTaskCount = taskCount,
            WorkloadStatus = GetComboBoxText(MemberWorkloadComboBox, "正常"),
            GrowthPlan = MemberGrowthPlanTextBox.Text.Trim()
        };

        newMember.Id = MemberRepository.Add(newMember);
        _members.Insert(0, newMember);

        RefreshStatistics();
        ClearMemberForm();

        MemberFormMessageText.Text = $"已新增成员：{newMember.Name}";
    }

    private void ClearMemberFormButton_Click(object sender, RoutedEventArgs e)
    {
        ClearMemberForm();
        MemberFormMessageText.Text = "输入内容已清空。";
    }

    private void IncreaseTaskCountButton_Click(object sender, RoutedEventArgs e)
    {
        var member = GetMemberFromButton(sender);

        if (member is null)
        {
            return;
        }

        member.CurrentTaskCount++;

        if (member.CurrentTaskCount >= 4)
        {
            member.WorkloadStatus = "关注";
        }

        MemberRepository.Update(member);
        RefreshMemberList();
        RefreshStatistics();

        MemberFormMessageText.Text = $"已增加任务负载：{member.Name}";
    }

    private void DecreaseTaskCountButton_Click(object sender, RoutedEventArgs e)
    {
        var member = GetMemberFromButton(sender);

        if (member is null)
        {
            return;
        }

        if (member.CurrentTaskCount > 0)
        {
            member.CurrentTaskCount--;
        }

        if (member.CurrentTaskCount <= 2)
        {
            member.WorkloadStatus = "正常";
        }

        MemberRepository.Update(member);
        RefreshMemberList();
        RefreshStatistics();

        MemberFormMessageText.Text = $"已减少任务负载：{member.Name}";
    }

    private void SetNormalWorkloadButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMemberWorkloadStatus(sender, "正常", "成员已标记为正常负载");
    }

    private void SetAttentionWorkloadButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMemberWorkloadStatus(sender, "关注", "成员已标记为需要关注");
    }

    private void DeleteMemberButton_Click(object sender, RoutedEventArgs e)
    {
        var member = GetMemberFromButton(sender);

        if (member is null)
        {
            return;
        }

        MemberRepository.Delete(member.Id);
        _members.Remove(member);

        RefreshStatistics();

        MemberFormMessageText.Text = $"成员已删除：{member.Name}";
    }

    private void UpdateMemberWorkloadStatus(object sender, string workloadStatus, string message)
    {
        var member = GetMemberFromButton(sender);

        if (member is null)
        {
            return;
        }

        member.WorkloadStatus = workloadStatus;

        MemberRepository.Update(member);
        RefreshMemberList();
        RefreshStatistics();

        MemberFormMessageText.Text = $"{message}：{member.Name}";
    }

    private MemberItem? GetMemberFromButton(object sender)
    {
        if (sender is not Button button || button.Tag is null)
        {
            return null;
        }

        if (!int.TryParse(button.Tag.ToString(), out var memberId))
        {
            return null;
        }

        return _members.FirstOrDefault(member => member.Id == memberId);
    }

    private void RefreshStatistics()
    {
        MemberCountText.Text = _members.Count.ToString();

        NormalWorkloadCountText.Text = _members
            .Count(member => member.WorkloadStatus == "正常")
            .ToString();

        IndependentCountText.Text = _members
            .Count(member =>
                member.AbilityLevel == "可独立完成" ||
                member.AbilityLevel == "可带人")
            .ToString();

        TotalTaskLoadText.Text = _members
            .Sum(member => member.CurrentTaskCount)
            .ToString();
    }

    private void RefreshMemberList()
    {
        MembersListView.ItemsSource = null;
        MembersListView.ItemsSource = _members;
    }

    private void ClearMemberForm()
    {
        MemberNameTextBox.Text = string.Empty;
        MemberGradeTextBox.Text = string.Empty;
        MemberDirectionTextBox.Text = string.Empty;
        MemberRoleTextBox.Text = string.Empty;
        MemberSkillTagsTextBox.Text = string.Empty;
        MemberTaskCountTextBox.Text = string.Empty;
        MemberGrowthPlanTextBox.Text = string.Empty;

        MemberAbilityComboBox.SelectedIndex = 0;
        MemberWorkloadComboBox.SelectedIndex = 0;
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