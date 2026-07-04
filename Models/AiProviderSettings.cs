namespace TeamFlowDesk.Models;

public class AiProviderSettings
{
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public string Model { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}