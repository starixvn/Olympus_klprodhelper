using System.Diagnostics;

namespace DotNet9ConsoleApp
{
    public class GitOperation(string directory)
    {
        public void Clean()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "clean -fd",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    Console.WriteLine(process.StandardOutput.ReadToEnd());
                    Console.WriteLine(process.StandardError.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during clean operation in directory '{directory}': {ex.Message}");
            }
        }

        public void FetchAndPrune()
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch --all --prune",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    Console.WriteLine("Fetching all remote branches and pruning deleted branches...");
                    Console.WriteLine(process.StandardOutput.ReadToEnd());
                    Console.WriteLine(process.StandardError.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during fetch and prune operation in directory '{directory}': {ex.Message}");
            }
        }

        public void CheckoutLatestBranch()
        {
            try
            {
                // First fetch all remote branches to get latest information
                var fetchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch --all",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(fetchStartInfo))
                {
                    process.WaitForExit();
                    var error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error fetching remote branches: {error}");
                        return;
                    }
                }

                // Get the latest commit hash and its branch from all remote branches
                var latestCommitStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "for-each-ref --sort=-committerdate refs/remotes/origin/ --format='%(refname:short)|%(committerdate:iso)|%(objectname)'",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string latestBranch = null;
                using (var process = Process.Start(latestCommitStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error getting remote branch information: {error}");
                        return;
                    }

                    // Get the first line which will be the most recent branch
                    var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(l => l.Replace("'", ""))
                        .Where(l => l.StartsWith("origin/"))
                        .Select(l =>
                        {
                            var parts = l.Split('|');
                            var b = parts[0].Replace("origin/", "").Trim();
                            var date = DateTime.Parse(parts[1]);
                            return (b, date);
                        })

                        .OrderByDescending(o => o.date)
                        .ToList();
                    latestBranch = lines.FirstOrDefault().b.Trim();
                }

                if (string.IsNullOrEmpty(latestBranch))
                {
                    Console.WriteLine("No remote branches found");
                    return;
                }

                // Create and checkout the branch locally if it doesn't exist
                var checkoutStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout -B {latestBranch} origin/{latestBranch}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(checkoutStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error checking out branch: {error}");
                        return;
                    }

                    Console.WriteLine($"Successfully checked out branch: {latestBranch}");
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during checkout latest branch operation in directory '{directory}': {ex.Message}");
            }
        }

        public void Commit(string message)
        {
            try
            {
                // Check if there are any changes
                var statusStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --porcelain",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string statusOutput;
                using (var process = Process.Start(statusStartInfo))
                {
                    process.WaitForExit();
                    statusOutput = process.StandardOutput.ReadToEnd();
                }

                // If no changes, return
                if (string.IsNullOrWhiteSpace(statusOutput))
                {
                    Console.WriteLine("No changes detected. Nothing to commit.");
                    return;
                }

                // Add all changes
                var addStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "add .",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(addStartInfo))
                {
                    process.WaitForExit();
                    var error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error adding files: {error}");
                    }
                }

                // Commit changes
                var commitStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"commit -m \"{message}\"",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(commitStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error committing changes: {error}");
                        return;
                    }

                    Console.WriteLine("Successfully committed changes:");
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during commit operation in directory '{directory}': {ex.Message}");
            }
        }

        public void Push()
        {
            try
            {
                // Get the current branch name
                var branchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string currentBranch;
                using (var process = Process.Start(branchStartInfo))
                {
                    process.WaitForExit();
                    currentBranch = process.StandardOutput.ReadToEnd().Trim();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error getting current branch: {error}");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(currentBranch))
                {
                    Console.WriteLine("Error: Could not determine current branch");
                    return;
                }

                // Unset upstream to avoid tracking the wrong remote branch
                var unsetUpstreamInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"branch --unset-upstream",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using (var process = Process.Start(unsetUpstreamInfo))
                {
                    process.WaitForExit();
                }

                // Always push with --set-upstream to ensure tracking the correct remote branch with the same name
                var pushStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"push --set-upstream origin {currentBranch}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(pushStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error pushing to remote: {error}");
                        return;
                    }

                    Console.WriteLine($"Successfully pushed branch '{currentBranch}' to remote:");
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during push operation in directory '{directory}': {ex.Message}");
            }
        }

        public void Rebase()
        {
            try
            {
                // First get current branch name
                var branchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string currentBranch;
                using (var process = Process.Start(branchStartInfo))
                {
                    process.WaitForExit();
                    currentBranch = process.StandardOutput.ReadToEnd().Trim();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error getting current branch: {error}");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(currentBranch))
                {
                    Console.WriteLine("Error: Could not determine current branch");
                    return;
                }

                // Fetch latest changes from origin
                var fetchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch origin",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(fetchStartInfo))
                {
                    process.WaitForExit();
                    var error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error fetching from origin: {error}");
                        return;
                    }
                }

                // Perform rebase
                var rebaseStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"rebase origin/{currentBranch}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(rebaseStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error during rebase: {error}");
                        Console.WriteLine("You may need to resolve conflicts manually.");
                        return;
                    }

                    Console.WriteLine($"Successfully rebased branch '{currentBranch}' onto origin/{currentBranch}:");
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during rebase operation in directory '{directory}': {ex.Message}");
            }
        }

        public void OpenGitExtensions(string gitFolder)
        {
            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = @"C:\Program Files\GitExtensions\GitExtensions.exe",
                    Arguments = $"browse \"{gitFolder}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening GitExtensions in directory '{directory}': {ex.Message}");
            }
        }

        public bool PrintDirty()
        {
            try
            {
                var statusStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "status --porcelain",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(statusStartInfo))
                {
                    process.WaitForExit();
                    var statusOutput = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error checking git status: {error}");
                        return true;
                    }

                    if (!string.IsNullOrWhiteSpace(statusOutput))
                    {
                        Console.WriteLine($"{directory} is dirty");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during PrintDirty operation in directory '{directory}': {ex.Message}");
                return true;
            }

            return false;
        }

        public void DeleteBranch(string branchName, bool isRemote = false)
        {
            try
            {
                // First check if we're trying to delete the current branch
                var currentBranchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "rev-parse --abbrev-ref HEAD",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                string currentBranch;
                using (var process = Process.Start(currentBranchStartInfo))
                {
                    process.WaitForExit();
                    currentBranch = process.StandardOutput.ReadToEnd().Trim();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error getting current branch: {error}");
                        return;
                    }
                }

                if (currentBranch == branchName)
                {
                    Console.WriteLine($"Cannot delete the current branch '{branchName}'. Please checkout a different branch first.");
                    return;
                }

                // Delete local branch
                var deleteLocalStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"branch -D {branchName}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(deleteLocalStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error deleting local branch: {error}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully deleted local branch '{branchName}'");
                    }
                }

                if (isRemote)
                {
                    // Delete remote branch
                    var deleteRemoteStartInfo = new ProcessStartInfo
                    {
                        FileName = "git",
                        Arguments = $"push origin --delete {branchName}",
                        WorkingDirectory = directory,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using (var process = Process.Start(deleteRemoteStartInfo))
                    {
                        process.WaitForExit();
                        var output = process.StandardOutput.ReadToEnd();
                        var error = process.StandardError.ReadToEnd();

                        if (!string.IsNullOrEmpty(error))
                        {
                            Console.WriteLine($"Error deleting remote branch: {error}");
                        }
                        else
                        {
                            Console.WriteLine($"Successfully deleted remote branch '{branchName}'");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during DeleteBranch operation in directory '{directory}': {ex.Message}");
            }
        }

        public void CreateBranch(string name, string originBranch)
        {
            try
            {
                // Check if branch already exists by listing branches
                var checkBranchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "branch --list",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(checkBranchStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error checking branches: {error}");
                        return;
                    }

                    // Check if branch exists locally (remove asterisk for current branch)
                    var branches = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(b => b.Trim().TrimStart('*', ' '))
                        .ToList();

                    if (branches.Contains(name))
                    {
                        Console.WriteLine($"Branch '{name}' already exists. Please use a different name.");
                        return;
                    }
                }

                // Fetch latest changes from origin
                var fetchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch origin",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(fetchStartInfo))
                {
                    process.WaitForExit();
                    var error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error fetching from origin: {error}");
                        return;
                    }
                }

                // Create and checkout new branch from origin branch
                var checkoutStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout -b {name} origin/{originBranch}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(checkoutStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error creating branch: {error}");
                        return;
                    }

                    Console.WriteLine($"Successfully created and checked out branch '{name}' from origin/{originBranch}:");
                    Console.WriteLine(output);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during CreateBranch operation in directory '{directory}': {ex.Message}");
            }
        }

        public bool CheckLocalBranchExisted(string branchName)
        {
            try
            {
                var branchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "branch --list",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(branchStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error checking local branches: {error}");
                        return false;
                    }

                    // Check if branch exists locally (remove asterisk for current branch)
                    var branches = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(b => b.Trim().TrimStart('*', ' '))
                        .ToList();

                    return branches.Contains(branchName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during CheckLocalBranchExisted operation in directory '{directory}': {ex.Message}");
                return false;
            }
        }

        public bool CheckRemoteBranchExisted(string branchName)
        {
            try
            {
                // First fetch to ensure we have latest remote branch information
                var fetchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "fetch --prune",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(fetchStartInfo))
                {
                    process.WaitForExit();
                    var error = process.StandardError.ReadToEnd();
                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error fetching remote branches: {error}");
                        return false;
                    }
                }

                // List remote branches
                var branchStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = "branch -r --list",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(branchStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error checking remote branches: {error}");
                        return false;
                    }

                    // Check if branch exists remotely (remove 'origin/' prefix)
                    var branches = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(b => b.Trim().Replace("origin/", ""))
                        .ToList();

                    return branches.Contains(branchName);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during CheckRemoteBranchExisted operation in directory '{directory}': {ex.Message}");
                return false;
            }
        }

        public void CheckoutBranch(string branchName)
        {
            try
            {
                var checkoutStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout {branchName}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(checkoutStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error checking out branch: {error}");
                    }
                    else
                    {
                        Console.WriteLine($"Checked out branch '{branchName}'");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during CheckoutBranch operation in directory '{directory}': {ex.Message}");
            }
        }

        public void PullLatest(string branchName)
        {
            try
            {
                var pullStartInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"pull origin {branchName}",
                    WorkingDirectory = directory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (var process = Process.Start(pullStartInfo))
                {
                    process.WaitForExit();
                    var output = process.StandardOutput.ReadToEnd();
                    var error = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(error))
                    {
                        Console.WriteLine($"Error pulling latest changes: {error}");
                    }
                    else
                    {
                        Console.WriteLine($"Successfully pulled latest changes from origin/{branchName}:");
                        Console.WriteLine(output);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during PullLatest operation in directory '{directory}': {ex.Message}");
            }
        }
    }
}