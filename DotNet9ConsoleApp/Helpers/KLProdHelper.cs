namespace DotNet9ConsoleApp.Helpers;

public class KLProdHelper(string directory, Dictionary<string, string> map)
{
    private GitOperation gitOperation = new GitOperation(directory);

    public void Action()
    {
        const string klprodBranch = "226231-klprod";
        gitOperation.FetchAndPrune();
        gitOperation.DeleteBranch(klprodBranch, false);
        var mainBranch = GetMainBranch();
        if (string.IsNullOrEmpty(mainBranch))
        {
            return;
        }

        gitOperation.CreateBranch(klprodBranch, mainBranch);

        var klprodFile = CopyAppSetting();
        if (!string.IsNullOrEmpty(klprodFile))
        {
            ReplaceSetting(klprodFile);
        }

        var csprojFiles = Directory.GetFiles(Path.GetDirectoryName(klprodFile), "*.csproj");
        foreach (var csprojFile in csprojFiles)
        {
            ProjectHelper.AddKLContent(csprojFile);
            ProjectHelper.AddKLContentRemove(csprojFile);
            ProjectHelper.AddKLNone(csprojFile);
            ProjectHelper.AddKLNoneRemove(csprojFile);
        }

        gitOperation.Commit("feat: init setting klprod");
        gitOperation.Push();
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

        Console.WriteLine("Không tìm thấy branch origin/master và origin/main");

        return string.Empty;
    }

    private string CopyAppSetting()
    {
        var files = Directory.GetFiles(directory, "*.json", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            var filename = Path.GetFileName(file);
            var path = Path.GetDirectoryName(file);
            if (string.Equals(filename, "appsettings.PROD.json", StringComparison.CurrentCultureIgnoreCase))
            {
                var newFile = Path.Combine(path, "appsettings.KLPROD.json");
                File.Copy(file, newFile);

                return newFile;
            }
        }

        Console.WriteLine("KHong tim thay app setting");
        return string.Empty;
    }

    private void ReplaceSetting(string file)
    {
        var text = File.ReadAllText(file);
        foreach (var key in map.Keys)
        {
            text = text.Replace(key, map[key]);
        }

        File.WriteAllText(file, text);
    }
}