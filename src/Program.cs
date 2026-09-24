using System.Diagnostics.CodeAnalysis;
using Anthropic;
using Anthropic.Models.Messages;

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

var session = new Session();
using AnthropicClient client = new();

while (true)
{
    if (!TryGetUserInput(out var userInput))
    {
        break;
    }

    session.AppendUserMessage(userInput);

    MessageCreateParams parameters = new()
    {
        MaxTokens = 1024,
        Messages = session.Messages,
        Model = Model.ClaudeOpus5_5,
    };

    var response = await client.Messages.Create(parameters);

    session.AppendSeverMessage(response);

    foreach (var block in response.Content)
    {
        if (block.TryPickText(out var textBlock))
        {
            ShowServerResponse(textBlock.Text);
        }
    }
}

