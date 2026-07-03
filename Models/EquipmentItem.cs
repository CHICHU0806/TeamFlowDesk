namespace TeamFlowDesk.Models;

public class EquipmentItem
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Category { get; set; } = string.Empty;

    public string Status { get; set; } = "可用";

    public string Location { get; set; } = string.Empty;

    public string CurrentHolder { get; set; } = string.Empty;

    public string RelatedTask { get; set; } = string.Empty;

    public string MaintenanceRecord { get; set; } = string.Empty;
}