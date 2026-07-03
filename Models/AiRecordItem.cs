namespace TeamFlowDesk.Models;

public class AiRecordItem
{
    public int Id { get; set; }

    public string RelatedModule { get; set; } = string.Empty;

    public string Question { get; set; } = string.Empty;

    public string AiSuggestion { get; set; } = string.Empty;

    public string HumanJudgement { get; set; } = string.Empty;

    public string FinalDecision { get; set; } = string.Empty;

    public string AdoptionStatus { get; set; } = "部分采纳";

    public DateTimeOffset CreatedAt { get; set; }
}