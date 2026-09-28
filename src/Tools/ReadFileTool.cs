using System.Text.Json;
using Anthropic.Models.Messages;

class ReadFileTool : ITool
{
    public string Name => "read_file";

    public Tool GetTool()
    {
        return new Tool()
        {
            Name = Name,
            Description = "Read the contents of a given relative file path. Use this when you want to see what's inside a file. Do not use this with directory names.",
            InputSchema = new InputSchema()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["path"] = JsonSerializer.SerializeToElement(new
                    {
                        type = "string",
                        description = "The relative path of a file in the working directory.",
                    }),
                },
                Required = ["path"],
            },
        };
    }

    public ToolUseResult Run(IReadOnlyDictionary<string, JsonElement> input)
    {
        return new() { Result = Run(input["path"].GetString()!)};
    }

    static string Run(string path)
    {
        return File.ReadAllText(path);
    }
}