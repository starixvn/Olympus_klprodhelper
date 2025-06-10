using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DotNet9ConsoleApp.Helpers
{
    public class SettingHelper
    {
        /// <summary>
        /// Reads a tab-delimited file and returns a dictionary where the first column is the key and the second column is the value.
        /// </summary>
        /// <param name="filePath">Path to the tab-delimited file</param>
        /// <returns>Dictionary with first column as key and second column as value</returns>
        public static Dictionary<string, string> Read(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file {filePath} was not found.");
            }

            var result = new Dictionary<string, string>();

            try
            {
                var lines = File.ReadAllLines(filePath);
                foreach (var line in lines)
                {
                    // Trim the entire line first
                    var trimmedLine = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedLine)) continue;

                    // Split and trim each part
                    var parts = trimmedLine.Split('\t')
                        .Select(part => part.Trim())
                        .ToArray();

                    if (parts.Length >= 2)
                    {
                        var key = parts[0];
                        var value = parts[1];
                        
                        if (!string.IsNullOrWhiteSpace(key) && !result.ContainsKey(key))
                        {
                            result[key] = value;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading file {filePath}: {ex.Message}", ex);
            }

            return result;
        }
    }
} 