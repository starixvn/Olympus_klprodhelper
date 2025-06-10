using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace DotNet9ConsoleApp.FanexData
{
    internal class SpHelper
    {
        public static void Action()
        {
            var keys = GetKeysFromFile("FanexData/SpKey.txt");
            CheckDuplicate(keys, @"D:\SourceCode\NAP.Reports\reportnet\src\TGR\SoccerPerformanceDetail\Criteria");
        }

        public static List<string> GetKeysFromFile(string filePath = "SpKey.txt")
        {
            var keys = new List<string>();
            
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filePath} not found.");
            }

            var lines = File.ReadAllLines(filePath);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                // Split the line by whitespace and take the second part (the key)
                var parts = line.Trim().Split(new[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    keys.Add(parts[1].Trim());
                }
            }

            return keys;
        }

        public static void ReplaceXTest(List<string> keys, string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File {filePath} not found.");
            }

            if (keys == null || !keys.Any())
            {
                throw new ArgumentException("Keys list cannot be null or empty.");
            }

            string content = File.ReadAllText(filePath);
            string modifiedContent = content;

            foreach (var key in keys)
            {
                // Create a case-insensitive pattern for SpName="{key}"
                string pattern = $@"SpName=""{Regex.Escape(key)}""";
                string replacement = $"SpName=\"{key}_xtest\"";
                
                // Perform case-insensitive replacement
                modifiedContent = Regex.Replace(modifiedContent, pattern, replacement, RegexOptions.IgnoreCase);
            }

            // Only write to file if changes were made
            if (modifiedContent != content)
            {
                File.WriteAllText(filePath, modifiedContent);
            }
        }

        public static void CheckDuplicate(List<string> keys, string directory)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Directory {directory} not found.");
            }

            if (keys == null || !keys.Any())
            {
                throw new ArgumentException("Keys list cannot be null or empty.");
            }

            // Get all files in directory and subdirectories
            var files = Directory.GetFiles(directory, "*.cs", SearchOption.AllDirectories)
                               .Where(f => !f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) && 
                                         !f.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                               .ToList();

            foreach (var key in keys)
            {
                // Find all files containing this key
                var filesWithKey = files.Where(file =>
                {
                    try
                    {
                        string content = File.ReadAllText(file);
                        // Look for key followed by a quote, case-insensitive
                        return Regex.IsMatch(content, $@"{Regex.Escape(key)}""", RegexOptions.IgnoreCase);
                    }
                    catch (Exception)
                    {
                        // Skip files that can't be read
                        return false;
                    }
                }).ToList();

                // If more than one file contains this key, report it
                if (filesWithKey.Count > 1)
                {
                    Console.WriteLine($"\nKey '{key}' found in multiple files:");
                    foreach (var file in filesWithKey)
                    {
                        Console.WriteLine($"- {file}");
                    }
                }
            }
        }
    }
}
