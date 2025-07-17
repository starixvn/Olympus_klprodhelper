// See https://aka.ms/new-console-template for more information

using DotNet9ConsoleApp.Helpers;

Console.WriteLine("KL UAT Helper - Processing multiple directories");

var settingFile = @"settings.txt";
var foldersFile = @"folders.txt"; // File containing list of folders to process

var map = SettingHelper.Read(settingFile);
var folders = ReadFoldersList(foldersFile);

if (folders.Count == 0)
{
    Console.WriteLine("No folders found in folders.txt. Please add folder paths to process.");
    return;
}

Console.WriteLine($"Found {folders.Count} folders to process:");
foreach (var folder in folders)
{
    Console.WriteLine($"- {folder}");
}

foreach (var folder in folders)
{
    if (Directory.Exists(folder))
    {
        Console.WriteLine($"\nProcessing folder: {folder}");
        ProcessDirectory(folder);
    }
    else
    {
        Console.WriteLine($"\nWarning: Folder not found: {folder}");
    }
}

Console.WriteLine("\nAll folders processed!");

// Read list of folders from file
List<string> ReadFoldersList(string filePath)
{
    var folders = new List<string>();

    if (!File.Exists(filePath))
    {
        Console.WriteLine($"Folders file not found: {filePath}");
        return folders;
    }

    var lines = File.ReadAllLines(filePath);
    foreach (var line in lines)
    {
        var trimmedLine = line.Trim();
        if (!string.IsNullOrEmpty(trimmedLine) && !trimmedLine.StartsWith("#"))
        {
            folders.Add(trimmedLine);
        }
    }

    return folders;
}

// write a function to check if there is a .git directory in the current directory
bool IsGitDirectory(string directory)
{
    return Directory.Exists(Path.Combine(directory, ".git"));
}

void ProcessDirectory(string directory)
{
    if (IsGitDirectory(directory))
    {
        Console.WriteLine($"Found git repository: {directory}");
        var klHelper = new KLUatHelper(directory, map);
        klHelper.Action();
        return;
    }

    var subDirectories = Directory.GetDirectories(directory);
    foreach (var d in subDirectories)
    {
        ProcessDirectory(d);
    }
}