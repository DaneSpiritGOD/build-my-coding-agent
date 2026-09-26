using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Anthropic;
using Anthropic.Models.Messages;

List<ITool> tools = new List<ITool>()
{
    new ReadFileTool(),
};

var clientTools = tools.Select(x => new ToolUnion(x.GetTool())).ToArray();

var session = new Session();
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

    var response = await client.Messages.Create(parameters);
    session.AppendServerResponse(response);

    tryGetUserInput = response.StopReason != StopReason.ToolUse;
    Console.WriteLine($"Stop reason: {response.StopReason}");
    foreach (var block in response.Content)
    {
        if (block.TryPickText(out var textBlock))
        {
            ShowServerResponse(textBlock.Text);
        }

        if (block.TryPickToolUse(out var toolUseBlock))
        {
            var result = RunTool(toolUseBlock.Name, toolUseBlock.Input);
            session.AppendToolUseResult(toolUseBlock.ID, result);
        }
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

string RunTool(string toolName, IReadOnlyDictionary<string, JsonElement> toolInput)
{
    Console.WriteLine($"Tool call - {toolName}");
    var result = tools.First(x => x.Name == toolName).Run(toolInput);
    // Console.Write($"Tool call ({toolName}) result: {result}");
    return result;
}
