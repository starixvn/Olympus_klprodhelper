// See https://aka.ms/new-console-template for more information

using DotNet9ConsoleApp.Helpers;

Console.WriteLine("Hello, World!");

var settingFile = @"settings.txt";
var root = @"D:\Projects\Olympus\auto-fantasy-service";

var map = SettingHelper.Read(settingFile);

ProcessDirectory(root);

// write a function to check if there is a .git directory in the current directory
bool IsGitDirectory(string directory)
{
    var subDirs = Directory.GetDirectories(directory);
    foreach (var subDir in subDirs)
    {
        if (Directory.Exists(Path.Combine(directory, ".git")))
        {
            return true;
        }
    }

    return false;
}

void ProcessDirectory(string directory)
{
    if (IsGitDirectory(directory))
    {
        var klHelper = new KLProdHelper(directory, map);
        klHelper.Action();

        return;
    }

    var subDirectories = Directory.GetDirectories(directory);
    foreach (var d in subDirectories)
    {
        ProcessDirectory(d);
    }
}