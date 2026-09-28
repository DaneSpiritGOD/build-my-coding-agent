class ToolUseResult
{
    public object? Result { get; init; }
    public string? Error { get; init; }

    public override string ToString()
    {
        return $"Result: {Result}, Error: {Error}";
    }
}