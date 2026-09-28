using System.Text.Json;
using Anthropic.Models.Messages;

class EditFileTool : ITool
{
    public string Name => "edit_file";

    public Tool GetTool()
    {
        return new Tool()
        {
            Name = Name,
            Description = @"Make edits to a text file.
Replaces 'oldStr' with 'newStr' in the given file. 'oldStr' and 'newStr' MUST be different from each other.
If the file specified with path doesn't exist, it will be created.",
            InputSchema = new InputSchema()
            {
                Properties = new Dictionary<string, JsonElement>
                {
                    ["path"] = JsonSerializer.SerializeToElement(new
                    {
                        type = "string",
                        description = "The path to the file",
                    }),
                    ["oldStr"] = JsonSerializer.SerializeToElement(new
                    {
                        type = "string",
                        description = "Text to search for - must match exactly and must only have one match exactly",
                    }),
                    ["newStr"] = JsonSerializer.SerializeToElement(new
                    {
                        type = "string",
                        description = "Text to replace old_str with",
                    }),
                },
                Required = ["path", "newStr"],
            },
        };
    }

    public ToolUseResult Run(IReadOnlyDictionary<string, JsonElement> input)
    {
        var path = input.TryGetValue("path", out var jsonElem1)? jsonElem1.GetString() : null;
        var oldStr = input.TryGetValue("oldStr", out var jsonElem2)? jsonElem2.GetString() : null;
        var newStr = input.TryGetValue("newStr", out var jsonElem3)? jsonElem3.GetString() : null;
        return Run(path, oldStr, newStr);
    }

    static ToolUseResult Run(string? path, string? oldStr, string? newStr)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ApplicationException($"Invalid parameter '{nameof(path)}'");
        }

        if (oldStr == newStr)
        {
            throw new ApplicationException($"Invalid parameter '{nameof(oldStr)}', '{nameof(newStr)}'");
        }

        PathUtils.EnsurePathIsInCurrentDirectory(path, out var filePath);

        if (!File.Exists(filePath) && string.IsNullOrEmpty(oldStr))
        {
            CreateNewFile(filePath, newStr);
            return new()
            {
                Result = $"Successfully created file {filePath}",
            };
        }

        if (string.IsNullOrEmpty(oldStr))
        {
            throw new ApplicationException($"Invalid parameter '{nameof(oldStr)}'");
        }

        var oldContent = File.ReadAllText(filePath);
        var newContent = oldContent.Replace(oldStr, newStr);

        if (oldContent == newContent) {
            throw new ApplicationException("oldStr not found in file'");
        }

        File.WriteAllText(filePath, newContent);
        return new()
        {
            Result = $"Successfully edited file {filePath}",
        };
    }

    static void CreateNewFile(string filePath, string? content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        File.WriteAllText(filePath, content ?? string.Empty);
    }
}