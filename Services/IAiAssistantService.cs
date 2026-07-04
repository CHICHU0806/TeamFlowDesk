namespace TeamFlowDesk.Services;

public interface IAiAssistantService
{
    Task<AiAssistantResult> GenerateManagementSuggestionAsync(
        string relatedModule,
        string question,
        CancellationToken cancellationToken = default);
}