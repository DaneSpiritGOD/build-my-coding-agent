using System.Text.Json;
using Anthropic.Models.Messages;

class ListFilesTool : ITool
{
    public string Name => "list_files";

    public Tool GetTool()
    {
        return new Tool()
        {
            Name = Name,
            Description = $"List files and directories at a given path. If no path is provided, lists files in the current directory. Paths are relative to the current working directory: {Environment.CurrentDirectory}",
            InputSchema = new InputSchema()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["path"] = JsonSerializer.SerializeToElement(new
                    {
                        type = "string",
                        description = "Optional relative path to list files from. Defaults to current directory if not provided.",
                    }),
                },
                Required = [],
            },
        };
    }

    public ToolUseResult Run(IReadOnlyDictionary<string, JsonElement> input)
    {
        var paths = Run(input.TryGetValue("path", out var jsonElem)? jsonElem.GetString() : null);
        return new() { Result = paths };
    }

    static string[] Run(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            path = Environment.CurrentDirectory;
        }

        PathUtils.EnsurePathIsInCurrentDirectory(path, out _);
        return Directory.GetFileSystemEntries(path!);
    }
}