using System.Text;

namespace DotNet9ConsoleApp;

public class IgnoreHelper
{
    private readonly string _directory;
    private readonly string _gitignorePath;

    public IgnoreHelper(string directory)
    {
        _directory = directory;
        _gitignorePath = Path.Combine(directory, ".gitignore");
    }

    public void AddIgnore()
    {
        // Create .gitignore file if it doesn't exist
        if (!File.Exists(_gitignorePath))
        {
            File.Create(_gitignorePath).Dispose();
        }

        var lines = File.ReadAllLines(_gitignorePath).ToList();
        var newLines = new List<string>();

        // Check and add bin directory
        if (!lines.Contains("**/bin/**"))
        {
            newLines.Add("**/bin/**");
        }

        // Check and add obj directory
        if (!lines.Contains("**/obj/**"))
        {
            newLines.Add("**/obj/**");
        }

        // Check and add .vs directory
        if (!lines.Contains("**/.vs/**"))
        {
            newLines.Add("**/.vs/**");
        }

        // Append new lines if any
        if (newLines.Any())
        {
            File.AppendAllLines(_gitignorePath, newLines);
        }
    }
} 