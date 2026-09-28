class PathUtils
{
    public static void EnsurePathIsInCurrentDirectory(string path, out string normalizedFullPath)
    {
        normalizedFullPath = Path.GetFullPath(path);
        if (!normalizedFullPath.StartsWith(Environment.CurrentDirectory))
        {
            throw new AccessViolationException($"{path} is out of current working directory.");
        }
    }
}