using System.Text.RegularExpressions;

namespace DotNet9ConsoleApp.Helpers;

public class ProjectHelper
{
    public static void AddKLContent(string csprojFile)
    {
        if (!File.Exists(csprojFile))
        {
            Console.WriteLine($"File not found: {csprojFile}");
            return;
        }

        var content = File.ReadAllText(csprojFile);
        
        // Pattern to match Content tag with appsettings.PROD.json
        var pattern = @"<Content[^>]*appsettings\.PROD\.json[^>]*>[\s\S]*?</Content>";
        var match = Regex.Match(content, pattern);

        if (!match.Success)
        {
            Console.WriteLine("No matching Content tag found for appsettings.PROD.json");
            return;
        }

        // Get the matched content and create a new one with KLPROD
        var prodContent = match.Value;
        var klprodContent = prodContent.Replace("appsettings.PROD.json", "appsettings.KLPROD.json");

        // Insert the new content after the existing one
        var newContent = content.Insert(match.Index + match.Length, "\n" + klprodContent);
        
        // Write back to file
        File.WriteAllText(csprojFile, newContent);
        Console.WriteLine("Added KLPROD content to csproj file");
    }

    public static void AddKLContentRemove(string csprojFile)
    {
        if (!File.Exists(csprojFile))
        {
            Console.WriteLine($"File not found: {csprojFile}");
            return;
        }

        var content = File.ReadAllText(csprojFile);
        
        // Pattern to match Content tag with Remove="appsettings.PROD.json"
        var pattern = @"<Content\s+Remove=""appsettings\.PROD\.json""\s*/>";
        var match = Regex.Match(content, pattern);

        if (!match.Success)
        {
            Console.WriteLine("No matching Content Remove tag found for appsettings.PROD.json");
            return;
        }

        // Get the matched content and create a new one with KLPROD
        var prodContent = match.Value;
        var klprodContent = prodContent.Replace("appsettings.PROD.json", "appsettings.KLPROD.json");

        // Insert the new content after the existing one
        var newContent = content.Insert(match.Index + match.Length, "\n" + klprodContent);
        
        // Write back to file
        File.WriteAllText(csprojFile, newContent);
        Console.WriteLine("Added KLPROD Remove content to csproj file");
    }

    public static void AddKLNone(string csprojFile)
    {
        if (!File.Exists(csprojFile))
        {
            Console.WriteLine($"File not found: {csprojFile}");
            return;
        }

        var content = File.ReadAllText(csprojFile);
        
        // Pattern to match None tag with appsettings.PROD.json
        var pattern = @"<None[^>]*appsettings\.PROD\.json[^>]*>[\s\S]*?</None>";
        var match = Regex.Match(content, pattern);

        if (!match.Success)
        {
            Console.WriteLine("No matching None tag found for appsettings.PROD.json");
            return;
        }

        // Get the matched content and create a new one with KLPROD
        var prodContent = match.Value;
        var klprodContent = prodContent.Replace("appsettings.PROD.json", "appsettings.KLPROD.json");

        // Insert the new content after the existing one
        var newContent = content.Insert(match.Index + match.Length, "\n" + klprodContent);
        
        // Write back to file
        File.WriteAllText(csprojFile, newContent);
        Console.WriteLine("Added KLPROD None content to csproj file");
    }

    public static void AddKLNoneRemove(string csprojFile)
    {
        if (!File.Exists(csprojFile))
        {
            Console.WriteLine($"File not found: {csprojFile}");
            return;
        }

        var content = File.ReadAllText(csprojFile);
        
        // Pattern to match None tag with Remove="appsettings.PROD.json"
        var pattern = @"<None\s+Remove=""appsettings\.PROD\.json""\s*/>";
        var match = Regex.Match(content, pattern);

        if (!match.Success)
        {
            Console.WriteLine("No matching None Remove tag found for appsettings.PROD.json");
            return;
        }

        // Get the matched content and create a new one with KLPROD
        var prodContent = match.Value;
        var klprodContent = prodContent.Replace("appsettings.PROD.json", "appsettings.KLPROD.json");

        // Insert the new content after the existing one
        var newContent = content.Insert(match.Index + match.Length, "\n" + klprodContent);
        
        // Write back to file
        File.WriteAllText(csprojFile, newContent);
        Console.WriteLine("Added KLPROD None Remove content to csproj file");
    }
} 