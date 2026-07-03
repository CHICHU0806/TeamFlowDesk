namespace TeamFlowDesk.Models;

public class ProjectItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public string Status { get; set; } = "进行中";

    public string CurrentStage { get; set; } = string.Empty;

    public string RiskLevel { get; set; } = "正常";

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public int ProgressPercent { get; set; }
}