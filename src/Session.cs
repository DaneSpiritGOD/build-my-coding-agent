using Anthropic.Models.Messages;

sealed class Session
{
    List<MessageParam> messages = new ();
    public List<MessageParam> Messages => messages;

    public void AppendUserMessage(string msg)
    {
        messages.Add(new()
        {
            Role = Role.User,
            Content = msg,
        });
    }

    public void AppendServerResponse(Message response)
    {
        messages.Add(new()
        {
            Role = Role.Assistant,
            Content = response.Content.Select(block => new ContentBlockParam(block.Json)).ToList(),
        });
    }

    public void AppendToolUseResult(string toolId, string result)
    {
        messages.Add(new()
        {
            Role = Role.User,
            Content = new MessageParamContent(
            [
                new ContentBlockParam(new ToolResultBlockParam() { ToolUseID = toolId, Content = result }),
            ]),
        });
    }
}