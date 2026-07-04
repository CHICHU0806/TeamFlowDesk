namespace TeamFlowDesk.Services;

public class AiAssistantResult
{
    public bool IsSuccess { get; init; }

    public string Content { get; init; } = string.Empty;

    public string ErrorMessage { get; init; } = string.Empty;

    public static AiAssistantResult Success(string content)
    {
        return new AiAssistantResult
        {
            IsSuccess = true,
            Content = content
        };
    }

    public static AiAssistantResult Failure(string errorMessage)
    {
        return new AiAssistantResult
        {
            IsSuccess = false,
            ErrorMessage = errorMessage
        };
    }
}