namespace TeamFlowDesk.Models;

public class WeeklyReportItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTimeOffset StartDate { get; set; }

    public DateTimeOffset EndDate { get; set; }

    public string CompletedWork { get; set; } = string.Empty;

    public string Problems { get; set; } = string.Empty;

    public string NextPlan { get; set; } = string.Empty;

    public string AiCollaborationSummary { get; set; } = string.Empty;

    public string ManagerReview { get; set; } = string.Empty;

    public string ProgressStatus { get; set; } = "正常";
}