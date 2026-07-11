namespace TeamFlowDesk.Models;

public class TaskItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

    public string ProjectName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string Collaborators { get; set; } = string.Empty;

    public string Status { get; set; } = "待处理";

    public string Priority { get; set; } = "普通";

    public string RiskLevel { get; set; } = "正常";

    public DateTimeOffset Deadline { get; set; }

    public string RelatedEquipment { get; set; } = string.Empty;

    public string OutputRequirement { get; set; } = string.Empty;

    public string DeadlineDisplay => Deadline.ToString("yyyy-MM-dd");

    public string ScheduleHint
    {
        get
        {
            if (Status == "已完成")
            {
                return "已完成";
            }

            var remainingDays = (Deadline.Date - DateTimeOffset.Now.Date).Days;

            return remainingDays switch
            {
                < 0 => $"已逾期 {-remainingDays} 天",
                0 => "今天到期",
                <= 3 => $"{remainingDays} 天内到期",
                _ => "按计划推进"
            };
        }
    }
}
