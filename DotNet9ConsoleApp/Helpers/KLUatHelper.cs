namespace DotNet9ConsoleApp.Helpers;

public class KLUatHelper(string directory, Dictionary<string, string> map)
{
    private readonly GitOperation gitOperation = new GitOperation(directory);

    public void Action()
    {
        var mainBranch = GetMainBranch();
        if (string.IsNullOrEmpty(mainBranch))
        {
            return;
        }

        // Pull latest code from main/master branch
        gitOperation.CheckoutBranch(mainBranch);
        gitOperation.PullLatest(mainBranch);

        var uatFile = FindAppSettingsUAT();
        if (!string.IsNullOrEmpty(uatFile))
        {
            var hasChanges = ReplaceSetting(uatFile);
            if (hasChanges)
            {
                gitOperation.Commit("feat: update KL UAT settings");
                gitOperation.Push();
            }
            else
            {
                Console.WriteLine("No settings were replaced, skipping commit and push.");
            }
        }
    }

    private string GetMainBranch()
    {
        if (gitOperation.CheckRemoteBranchExisted("main"))
        {
            return "main";
        }

        if (gitOperation.CheckRemoteBranchExisted("master"))
        {
            return "master";
        }

        Console.WriteLine("Cannot find branch origin/master or origin/main");

        return string.Empty;
    }

    private string FindAppSettingsUAT()
    {
        var files = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            if (string.Equals(filename, "appsettings.UAT.json", StringComparison.CurrentCultureIgnoreCase))
            {
                return file;
            }
        }

        Console.WriteLine("Cannot find appsettings.UAT.json");
        return string.Empty;
    }

    private bool ReplaceSetting(string file)
    {
        var text = File.ReadAllText(file);
        var originalText = text;
        var hasChanges = false;

        foreach (var key in map.Keys)
        {
            if (text.Contains(key))
            {
                text = text.Replace(key, map[key]);
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            File.WriteAllText(file, text);
            Console.WriteLine($"Updated settings in {file}");
        }
        else
        {
            Console.WriteLine($"No keys found to replace in {file}");
        }

        return hasChanges;
    }
}