using System.Text.Json;
using Anthropic.Models.Messages;

interface ITool
{
    string Name { get; }
    ToolUseResult Run(IReadOnlyDictionary<string, JsonElement> input);
    Tool GetTool();
}