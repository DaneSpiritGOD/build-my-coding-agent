using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;

List<ITool> tools = new List<ITool>()
{
    new ReadFileTool(),
    new ListFilesTool(),
};

var clientTools = tools.Select(x => new ToolUnion(x.GetTool())).ToArray();

var session = new Session();
session.SessionEventOccurring += OnSessionEventOccurring;

using AnthropicClient client = new();

var tryGetUserInput = true;

while (true)
{
    if (tryGetUserInput)
    {
        if (!TryGetUserInput(out var userInput))
        {
            break;
        }

        session.AppendUserMessage(userInput);
    }

    MessageCreateParams parameters = new()
    {
        MaxTokens = 1024,
        Messages = session.Messages,
        Model = Model.ClaudeOpus5_5,
        Tools = clientTools,
    };

    Message response;
    try
    {
        response = await client.Messages.Create(parameters);
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"API 调用失败，结束会话: {ex.Message}");
        break;
    }

    session.AppendServerResponse(response);

    tryGetUserInput = response.StopReason != StopReason.ToolUse;
    Console.WriteLine($"Stop reason: {response.StopReason}");

    var toolResults = new List<(string ToolId, ToolUseResult Result)>();
    foreach (var block in response.Content)
    {
        if (block.TryPickText(out var textBlock))
        {
            ShowServerResponse(textBlock.Text);
        }

        if (block.TryPickToolUse(out var toolUseBlock))
        {
            toolResults.Add((toolUseBlock.ID, RunTool(toolUseBlock.ID, toolUseBlock.Name, toolUseBlock.Input)));
        }
    }

    // Tool use results need to be aggregated into one single block of the session,
    // Otherwise we will get AnthropicBadRequestException ("tool_use ids were found without tool_result blocks immediately after")
    if (toolResults.Count > 0)
    {
        session.AppendToolUseResults(toolResults);
    }
}

bool TryGetUserInput([NotNullWhen(true)]out string? input)
{
    Console.Write("You: ");
    input = Console.In.ReadLine();
    return !string.IsNullOrWhiteSpace(input);
}

void ShowServerResponse(string message)
{
    Console.Write("Assistant: ");
    Console.WriteLine(message);
}

ToolUseResult RunTool(string toolId, string toolName, IReadOnlyDictionary<string, JsonElement> toolInput)
{
    try
    {
        Console.WriteLine($"Tool call ({toolId}): {toolName} | {string.Join(", ", toolInput.Select(x => x.Key + ": " + x.Value.ToString()))}");
        return tools.First(x => x.Name == toolName).Run(toolInput);
    }
    catch (Exception ex)
    {
        return new()
        {
            Error = ex.ToString(),
        };
    }
}

void OnSessionEventOccurring(object? sender, SessionEvent sessionEvent)
{
    switch (sessionEvent.EventName)
    {
        case "ToolUseResult":
            Console.WriteLine($"Session Event (ToolUseResult): {sessionEvent.Content}");
            break;
        default:
            break;
    }
}
