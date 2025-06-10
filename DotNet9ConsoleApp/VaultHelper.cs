using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Diagnostics;

namespace DotNet9ConsoleApp
{
    public class VaultHelper(string directory)
    {
        public List<string> GetVaultFiles()
        {
            List<string> vaultFiles = new List<string>();
            string[] files = Directory.GetFiles(directory, "appsettings.KLDR.json", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if ( Path.GetFileName(file)== "appsettings.KLDR.json")
                {
                    vaultFiles.Add(file);
                }
            }
            return vaultFiles;
        }

        public void CorrectKLDR(string configPath)
        {
            
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"Error: File not found: {configPath}");
                return;
            }

            try
            {
                // Read the JSON file
                string jsonContent = File.ReadAllText(configPath);
                var jsonNode = JsonNode.Parse(jsonContent);

                // Get the VaultOptions object
                var vaultOptions = jsonNode?["VaultOptions"];
                if (vaultOptions == null)
                {
                    Console.WriteLine("Error: VaultOptions not found in configuration");
                    return;
                }

                // Store original values for logging
                string originalRole = vaultOptions["Role"]?.ToString();
                string originalSecretId = vaultOptions["SecretId"]?.ToString();

                // Update the values
                vaultOptions["Role"] = "80238297-d650-2a52-f7f4-56ebf345df80";
                vaultOptions["SecretId"] = "18ec0f35-b286-69f3-0cc8-f9bf26debf4e";

                // Write back to file with proper formatting
                var options = new JsonSerializerOptions { WriteIndented = true };
                string updatedJson = jsonNode.ToJsonString(options);
                File.WriteAllText(configPath, updatedJson);

                Console.WriteLine("Successfully updated VaultOptions:");
                Console.WriteLine($"Original Role: {originalRole} -> New Role: {vaultOptions["Role"]}");
                Console.WriteLine($"Original SecretId: {originalSecretId} -> New SecretId: {vaultOptions["SecretId"]}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating vault configuration: {ex.Message}");
            }
        }
        
    }
} 