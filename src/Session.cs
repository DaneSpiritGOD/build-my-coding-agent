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

    public void AppendSeverMessage(Message message)
    {
        messages.Add(MessageParam.FromRawUnchecked(message.RawData));
    }
}