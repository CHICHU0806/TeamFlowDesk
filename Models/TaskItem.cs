namespace TeamFlowDesk.Models;

public class TaskItem
{
    public int Id { get; set; }

    public int ProjectId { get; set; }

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
}