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
            AbilityLevel = GetComboBoxText(MemberAbilityComboBox, "入门"),
            CurrentTaskCount = taskCount,
            WorkloadStatus = GetComboBoxText(MemberWorkloadComboBox, "正常"),
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
        PreserveScrollPosition(() =>
        {
            var member = GetMemberFromButton(sender);

            if (member is null)
            {
                return;
            }

            member.CurrentTaskCount++;

            if (member.CurrentTaskCount >= 5)
            {
                member.WorkloadStatus = "过载";
            }
            else if (member.CurrentTaskCount >= 3)
            {
                member.WorkloadStatus = "关注";
            }
            else if (member.CurrentTaskCount == 0)
            {
                member.WorkloadStatus = "空闲";
            }
            else
            {
                member.WorkloadStatus = "正常";
            }

            MemberRepository.Update(member);
            RefreshAll();

            MemberFormMessageText.Text = $"已增加任务负载：{member.Name}";
        });
    }
    
    private void DecreaseTaskCountButton_Click(object sender, RoutedEventArgs e)
    {
        PreserveScrollPosition(() =>
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

            if (member.CurrentTaskCount == 0)
            {
                member.WorkloadStatus = "空闲";
            }
            else if (member.CurrentTaskCount <= 2)
            {
                member.WorkloadStatus = "正常";
            }
            else if (member.CurrentTaskCount <= 4)
            {
                member.WorkloadStatus = "关注";
            }
            else
            {
                member.WorkloadStatus = "过载";
            }

            MemberRepository.Update(member);
            RefreshAll();

            MemberFormMessageText.Text = $"已减少任务负载：{member.Name}";
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
        PreserveScrollPosition(() =>
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
    
    private void UpdateMemberWorkloadStatus(object sender, string workloadStatus, string message)
    {
        PreserveScrollPosition(() =>
        {
            var member = GetMemberFromButton(sender);

            if (member is null)
            {
                return;
            }

            member.WorkloadStatus = workloadStatus;

            MemberRepository.Update(member);
            RefreshAll();

            MemberFormMessageText.Text = $"{message}：{member.Name}";
        });
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

    private void PreserveScrollPosition(Action action)
    {
        var verticalOffset = RootScrollViewer.VerticalOffset;

        action();

        DispatcherQueue.TryEnqueue(() =>
        {
            RootScrollViewer.ChangeView(
                null,
                verticalOffset,
                null,
                disableAnimation: true);
        });
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
            .Count(member =>
                member.AbilityLevel == "可独立完成" ||
                member.AbilityLevel == "可带人")
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
        var independentCount = _members.Count(member =>
            member.AbilityLevel == "可独立完成" ||
            member.AbilityLevel == "可带人");

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
            ? "当前成员能力整体较成熟，后续可以重点沉淀经验文档和交接机制。"
            : $"当前有 {beginnerCount} 名成员仍处于入门或熟悉阶段，建议结合任务作战板安排低风险练习任务，形成培养闭环。";
    }

    private void RefreshMemberCards()
    {
        MembersItemsControl.ItemsSource = _members
            .OrderByDescending(member =>
                member.WorkloadStatus == "过载" ? 4 :
                member.WorkloadStatus == "关注" ? 3 :
                member.WorkloadStatus == "正常" ? 2 :
                member.WorkloadStatus == "空闲" ? 1 : 0)
            .ThenByDescending(member => member.CurrentTaskCount)
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