namespace TeamFlowDesk.Models;

public class MemberItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Grade { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string SkillTags { get; set; } = string.Empty;

    public string AbilityLevel { get; set; } = "入门";

    public int CurrentTaskCount { get; set; }

    public string WorkloadStatus { get; set; } = "正常";

    public string GrowthPlan { get; set; } = string.Empty;
}