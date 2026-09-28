using System.Text.Json;
using Anthropic.Models.Messages;

struct SessionEvent
{
    public required string EventName { get; init; }
    public required string Content { get; init; }
}

sealed class Session
{
    List<MessageParam> messages = new ();
    public List<MessageParam> Messages => messages;

    public event EventHandler<SessionEvent>? SessionEventOccurring;

    public void AppendUserMessage(string msg)
    {
        SessionEventOccurring?.Invoke(this, new()
        {
            EventName = "UserMessage",
            Content = msg,
        });
        messages.Add(new()
        {
            Role = Role.User,
            Content = msg,
        });
    }

    public void AppendServerResponse(Message response)
    {
        SessionEventOccurring?.Invoke(this, new()
        {
            EventName = "ServerResponse",
            Content = response.ToString(),
        });
        messages.Add(new()
        {
            Role = Role.Assistant,
            Content = response.Content.Select(block => new ContentBlockParam(block.Json)).ToList(),
        });
    }

    public void AppendToolUseResults(IReadOnlyList<(string ToolId, ToolUseResult Result)> results)
    {
        var blocks = new List<ContentBlockParam>();
        foreach (var (toolId, result) in results)
        {
            SessionEventOccurring?.Invoke(this, new()
            {
                EventName = "ToolUseResult",
                Content = $"{toolId} {result}",
            });
            blocks.Add(new ContentBlockParam(new ToolResultBlockParam()
            {
                ToolUseID = toolId,
                Content = JsonSerializer.Serialize(result),
                IsError = result.Error is not null,
            }));
        }

        messages.Add(new()
        {
            Role = Role.User,
            Content = new MessageParamContent(blocks),
        });
    }
}