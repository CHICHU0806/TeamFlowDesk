using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TeamFlowDesk.Data;
using TeamFlowDesk.Models;
using TeamFlowDesk.Services.Ui;

namespace TeamFlowDesk.Pages;

public sealed partial class MembersPage : Page
{
    private readonly ObservableCollection<MemberItem> _members;

    public MembersPage()
    {
        InitializeComponent();

        MemberRepository.SeedIfEmpty();
        _members = new ObservableCollection<MemberItem>(MemberRepository.GetAll());

        RefreshAll();
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
            AbilityLevel = PageInteractionService.GetComboBoxText(
                MemberAbilityComboBox,
                "入门"),
            CurrentTaskCount = taskCount,
            WorkloadStatus = PageInteractionService.GetComboBoxText(
                MemberWorkloadComboBox,
                "正常"),
            GrowthPlan = MemberGrowthPlanTextBox.Text.Trim()
        };

        newMember.Id = MemberRepository.Add(newMember);
        _members.Insert(0, newMember);

        RefreshAll();
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
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var member = GetMemberFromButton(sender);

            if (member is null)
            {
                return;
            }

            var newTaskCount = member.CurrentTaskCount + 1;
            var newWorkloadStatus = CalculateWorkloadStatus(newTaskCount);

            var updatedMember = CopyMemberWithNewState(
                member,
                newTaskCount,
                newWorkloadStatus);

            MemberRepository.Update(updatedMember);

            PageInteractionService.ReplaceItem(
                _members,
                member,
                updatedMember);

            RefreshAll();

            MemberFormMessageText.Text = $"已增加任务负载：{updatedMember.Name}";
        });
    }

    private void DecreaseTaskCountButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var member = GetMemberFromButton(sender);

            if (member is null)
            {
                return;
            }

            var newTaskCount = member.CurrentTaskCount > 0
                ? member.CurrentTaskCount - 1
                : 0;

            var newWorkloadStatus = CalculateWorkloadStatus(newTaskCount);

            var updatedMember = CopyMemberWithNewState(
                member,
                newTaskCount,
                newWorkloadStatus);

            MemberRepository.Update(updatedMember);

            PageInteractionService.ReplaceItem(
                _members,
                member,
                updatedMember);

            RefreshAll();

            MemberFormMessageText.Text = $"已减少任务负载：{updatedMember.Name}";
        });
    }

    private void SetNormalWorkloadButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMemberWorkloadStatus(sender, "正常", "成员已标记为正常负载");
    }

    private void SetAttentionWorkloadButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMemberWorkloadStatus(sender, "关注", "成员已标记为需要关注");
    }

    private void SetOverloadWorkloadButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMemberWorkloadStatus(sender, "过载", "成员已标记为过载");
    }

    private void SetIdleWorkloadButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateMemberWorkloadStatus(sender, "空闲", "成员已标记为空闲");
    }

    private void DeleteMemberButton_Click(object sender, RoutedEventArgs e)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var member = GetMemberFromButton(sender);

            if (member is null)
            {
                return;
            }

            MemberRepository.Delete(member.Id);
            _members.Remove(member);

            RefreshAll();

            MemberFormMessageText.Text = $"成员已删除：{member.Name}";
        });
    }

    private void UpdateMemberWorkloadStatus(
        object sender,
        string workloadStatus,
        string message)
    {
        PageInteractionService.RunKeepingScrollPosition(this, () =>
        {
            var member = GetMemberFromButton(sender);

            if (member is null)
            {
                return;
            }

            var updatedMember = CopyMemberWithNewState(
                member,
                member.CurrentTaskCount,
                workloadStatus);

            MemberRepository.Update(updatedMember);

            PageInteractionService.ReplaceItem(
                _members,
                member,
                updatedMember);

            RefreshAll();

            MemberFormMessageText.Text = $"{message}：{updatedMember.Name}";
        });
    }

    private MemberItem? GetMemberFromButton(object sender)
    {
        return PageInteractionService.GetItemFromButton(
            sender,
            _members,
            member => member.Id);
    }

    private static MemberItem CopyMemberWithNewState(
        MemberItem source,
        int currentTaskCount,
        string workloadStatus)
    {
        return new MemberItem
        {
            Id = source.Id,
            Name = source.Name,
            Grade = source.Grade,
            Direction = source.Direction,
            Role = source.Role,
            SkillTags = source.SkillTags,
            AbilityLevel = source.AbilityLevel,
            CurrentTaskCount = currentTaskCount,
            WorkloadStatus = workloadStatus,
            GrowthPlan = source.GrowthPlan
        };
    }

    private static string CalculateWorkloadStatus(int currentTaskCount)
    {
        if (currentTaskCount <= 0)
        {
            return "空闲";
        }

        if (currentTaskCount <= 2)
        {
            return "正常";
        }

        if (currentTaskCount <= 4)
        {
            return "关注";
        }

        return "过载";
    }

    private void RefreshAll()
    {
        RefreshStatistics();
        RefreshInsights();
        RefreshMemberCards();
    }

    private void RefreshStatistics()
    {
        MemberCountText.Text = _members.Count.ToString();

        NormalWorkloadCountText.Text = _members
            .Count(member => member.WorkloadStatus == "正常")
            .ToString();

        IndependentCountText.Text = _members
            .Count(IsIndependentMember)
            .ToString();

        TotalTaskLoadText.Text = _members
            .Sum(member => member.CurrentTaskCount)
            .ToString();
    }

    private void RefreshInsights()
    {
        var attentionCount = _members.Count(member => member.WorkloadStatus == "关注");
        var overloadCount = _members.Count(member => member.WorkloadStatus == "过载");
        var idleCount = _members.Count(member => member.WorkloadStatus == "空闲");
        var independentCount = _members.Count(IsIndependentMember);

        MemberStatusSummaryText.Text =
            $"当前共有 {_members.Count} 名成员，任务负载总数为 {_members.Sum(member => member.CurrentTaskCount)}，其中关注 {attentionCount} 人，过载 {overloadCount} 人，空闲 {idleCount} 人。";

        if (overloadCount > 0)
        {
            WorkloadInsightText.Text = $"当前存在 {overloadCount} 名过载成员，建议负责人优先调整任务分配，避免关键成员长期承担过多任务。";
        }
        else if (attentionCount > 0)
        {
            WorkloadInsightText.Text = $"当前有 {attentionCount} 名成员需要关注，可以检查其任务数量、截止时间和任务难度是否合理。";
        }
        else if (idleCount > 0)
        {
            WorkloadInsightText.Text = $"当前有 {idleCount} 名成员处于空闲状态，可以考虑安排学习任务、辅助任务或阶段性练习任务。";
        }
        else
        {
            WorkloadInsightText.Text = "当前成员负载整体较平稳，可以继续保持任务状态更新。";
        }

        AbilityInsightText.Text = independentCount == 0
            ? "当前暂无可独立完成或可带人的成员记录，建议进一步细化成员能力等级。"
            : $"当前有 {independentCount} 名成员具备独立完成或带人能力，可优先承担关键任务或带新人任务。";

        var beginnerCount = _members.Count(member =>
            member.AbilityLevel == "入门" ||
            member.AbilityLevel == "熟悉");

        TrainingInsightText.Text = beginnerCount == 0
            ? "当前成员能力整体较成熟，当前培养重点为经验文档沉淀和交接机制落实。"
            : $"当前有 {beginnerCount} 名成员仍处于入门或熟悉阶段，建议结合任务作战板安排低风险练习任务，形成培养闭环。";
    }

    private void RefreshMemberCards()
    {
        MembersItemsControl.ItemsSource = _members
            .OrderByDescending(GetWorkloadSortWeight)
            .ThenByDescending(member => member.CurrentTaskCount)
            .ThenBy(member => member.Name)
            .ToList();
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

    private static bool IsIndependentMember(MemberItem member)
    {
        return member.AbilityLevel == "可独立完成" ||
               member.AbilityLevel == "可带人";
    }

    private static int GetWorkloadSortWeight(MemberItem member)
    {
        return member.WorkloadStatus switch
        {
            "过载" => 4,
            "关注" => 3,
            "正常" => 2,
            "空闲" => 1,
            _ => 0
        };
    }
}
