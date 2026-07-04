using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace TeamFlowDesk.Services;

public class OpenAiAssistantService : IAiAssistantService
{
    private static readonly HttpClient HttpClient = new();

    public async Task<AiAssistantResult> GenerateManagementSuggestionAsync(
        string relatedModule,
        string question,
        CancellationToken cancellationToken = default)
    {
        var settings = AiSettingsService.Load();

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return AiAssistantResult.Failure("当前未配置 API Base URL，请先在系统设置中填写。");
        }

        if (string.IsNullOrWhiteSpace(settings.Model))
        {
            return AiAssistantResult.Failure("当前未配置模型名称，请先在系统设置中填写。");
        }

        if (string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            return AiAssistantResult.Failure("当前未配置 API Key，请先在系统设置中填写。");
        }

        if (string.IsNullOrWhiteSpace(relatedModule))
        {
            return AiAssistantResult.Failure("关联模块不能为空。");
        }

        if (string.IsNullOrWhiteSpace(question))
        {
            return AiAssistantResult.Failure("问题描述不能为空。");
        }

        var instructions =
            """
            你是 TeamFlowDesk 的 AI 管理助手。
            这个系统面向团队负责人、队长、项目经理和方向负责人，用于内部私有化宏观管理。
            你的任务不是替负责人做最终决策，而是提供清晰、可执行、可被人工判断和修正的管理建议。

            输出要求：
            1. 先简要判断当前问题的核心。
            2. 给出 3 到 5 条可执行建议。
            3. 指出可能存在的风险或注意事项。
            4. 最后给出一句适合填写到“最终决策”前的参考总结。
            5. 不要替用户宣布最终结论。
            """;

        var userInput =
            $"""
            关联模块：{relatedModule}

            问题描述：
            {question}
            """;

        var requestBody = new
        {
            model = settings.Model,
            instructions,
            input = userInput,
            max_output_tokens = 900
        };

        var json = JsonSerializer.Serialize(requestBody);

        var endpoint = $"{settings.BaseUrl.TrimEnd('/')}/responses";

        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        request.Content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var response = await HttpClient.SendAsync(request, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                return AiAssistantResult.Failure(
                    $"AI 请求失败：{(int)response.StatusCode} {response.ReasonPhrase}\n{TrimForDisplay(responseText)}");
            }

            var outputText = ExtractOutputText(responseText);

            if (string.IsNullOrWhiteSpace(outputText))
            {
                return AiAssistantResult.Failure("AI 返回成功，但没有解析到有效文本。");
            }

            return AiAssistantResult.Success(outputText.Trim());
        }
        catch (TaskCanceledException)
        {
            return AiAssistantResult.Failure("AI 请求已取消或超时。");
        }
        catch (Exception ex)
        {
            return AiAssistantResult.Failure($"AI 请求异常：{ex.Message}");
        }
    }

    private static string ExtractOutputText(string responseText)
    {
        using var document = JsonDocument.Parse(responseText);
        var root = document.RootElement;

        if (root.TryGetProperty("output_text", out var outputTextElement) &&
            outputTextElement.ValueKind == JsonValueKind.String)
        {
            return outputTextElement.GetString() ?? string.Empty;
        }

        if (!root.TryGetProperty("output", out var outputElement) ||
            outputElement.ValueKind != JsonValueKind.Array)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        foreach (var outputItem in outputElement.EnumerateArray())
        {
            if (!outputItem.TryGetProperty("content", out var contentElement) ||
                contentElement.ValueKind != JsonValueKind.Array)
            {
                continue;
            }

            foreach (var contentItem in contentElement.EnumerateArray())
            {
                if (contentItem.TryGetProperty("text", out var textElement) &&
                    textElement.ValueKind == JsonValueKind.String)
                {
                    builder.AppendLine(textElement.GetString());
                }
            }
        }

        return builder.ToString();
    }

    private static string TrimForDisplay(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        const int maxLength = 500;

        return text.Length <= maxLength
            ? text
            : text[..maxLength] + "...";
    }
}