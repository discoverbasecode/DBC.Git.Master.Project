using System;
using System.Threading.Tasks;
using TGui = Terminal.Gui;
using System.Collections.Generic;

namespace DBC.Git.Master.App
{
    class Program
    {
        static GitHubService? githubService;
        static TGui.Label? githubStatusLabel;

        static async Task Main(string[] args)
        {
            Logger.Log("Starting GitMaster application...");
            try
            {
                // Load saved token if exists
                Logger.Log("Loading GitHub token...");
                githubService = new GitHubService();
                await githubService.LoadGitHubToken();

                TGui.Application.Init();
                Logger.Log("Terminal.Gui initialized.");
                var top = TGui.Application.Top;

                // Create main window
                var win = new TGui.Window("GitMaster CLI")
                {
                    X = TGui.Pos.At(0),
                    Y = TGui.Pos.At(0),
                    Width = TGui.Dim.Fill(),
                    Height = TGui.Dim.Fill()
                };
                top.Add(win);

                // GitHub connection status
                githubStatusLabel = new TGui.Label(githubService.GitHubToken != null
                    ? "Connected to GitHub"
                    : "Please enter a GitHub token")
                {
                    X = TGui.Pos.At(2),
                    Y = TGui.Pos.At(1),
                    Width = TGui.Dim.Fill()
                };
                win.Add(githubStatusLabel);

                // Input for GitHub token
                var tokenLabel = new TGui.Label("GitHub Personal Access Token: ")
                {
                    X = TGui.Pos.At(2),
                    Y = TGui.Pos.At(3)
                };
                var tokenField = new TGui.TextField(githubService.GitHubToken ?? "")
                {
                    X = TGui.Pos.Right(tokenLabel),
                    Y = TGui.Pos.At(3),
                    Width = 40
                };
                win.Add(tokenLabel, tokenField);

                // Repository list
                var repoListView = new TGui.ListView
                {
                    X = TGui.Pos.At(2),
                    Y = TGui.Pos.At(5),
                    Width = TGui.Dim.Fill() - 2,
                    Height = 5
                };
                win.Add(repoListView);

                // Connect button
                var connectButton = new TGui.Button("Connect to GitHub")
                {
                    X = TGui.Pos.At(2),
                    Y = TGui.Pos.At(11)
                };
                connectButton.Clicked += async () =>
                {
                    Logger.Log("Connect to GitHub button clicked.");
                    githubService.GitHubToken = tokenField.Text.ToString();
                    await githubService.Connect(githubStatusLabel, repoListView);
                };
                win.Add(connectButton);

                // Reset token button
                var resetTokenButton = new TGui.Button("Reset GitHub Token")
                {
                    X = TGui.Pos.Right(connectButton) + 2,
                    Y = TGui.Pos.At(11)
                };
                resetTokenButton.Clicked += () =>
                {
                    Logger.Log("Reset GitHub Token button clicked.");
                    githubService.ResetToken(githubStatusLabel, tokenField, repoListView);
                };
                win.Add(resetTokenButton);

                // View repository contents button
                var viewRepoButton = new TGui.Button("View Repository Contents")
                {
                    X = TGui.Pos.Right(resetTokenButton) + 2,
                    Y = TGui.Pos.At(11)
                };
                viewRepoButton.Clicked += () =>
                {
                    Logger.Log("View Repository Contents button clicked.");
                    UiComponents.ShowRepositoryContents(githubService, repoListView);
                };
                win.Add(viewRepoButton);

                // Git Commands Menu
                var menu = new TGui.MenuBar(new TGui.MenuBarItem[]
                {
                    new TGui.MenuBarItem("_Git Commands", new TGui.MenuItem[]
                    {
                        new TGui.MenuItem("_Add", "",
                            () => GitCommands.RunGitCommandWithInput("add", "Enter files to add (e.g., . for all):")),
                        new TGui.MenuItem("_Add File to GitHub", "",
                            () => UiComponents.AddFileToRepository(githubService, repoListView)),
                        new TGui.MenuItem("_Branch", "", () => GitCommands.ExecuteGitCommand("branch")),
                        new TGui.MenuItem("_Checkout", "",
                            () => GitCommands.RunGitCommandWithInput("checkout", "Enter branch name:")),
                        new TGui.MenuItem("_Clean", "", () => GitCommands.ExecuteGitCommand("clean -fd")),
                        new TGui.MenuItem("_Clone", "",
                            () => GitCommands.RunGitCommandWithInput("clone", "Enter repository URL:")),
                        new TGui.MenuItem("_Commit", "",
                            () => GitCommands.RunGitCommandWithInput("commit -m", "Enter commit message:")),
                        new TGui.MenuItem("_Commit --amend", "", () => GitCommands.ExecuteGitCommand("commit --amend")),
                        new TGui.MenuItem("_Config", "",
                            () => GitCommands.RunGitCommandWithInput("config",
                                "Enter config command (e.g., user.name 'Your Name'):")),
                        new TGui.MenuItem("_Delete Repository", "",
                            () => UiComponents.DeleteRepository(githubService, repoListView)),
                        new TGui.MenuItem("_Fetch", "", () => GitCommands.ExecuteGitCommand("fetch")),
                        new TGui.MenuItem("_Init", "", () => GitCommands.ExecuteGitCommand("init")),
                        new TGui.MenuItem("_Log", "", () => GitCommands.ExecuteGitCommand("log --oneline")),
                        new TGui.MenuItem("_Merge", "",
                            () => GitCommands.RunGitCommandWithInput("merge", "Enter branch to merge:")),
                        new TGui.MenuItem("_Pull", "",
                            () => GitCommands.RunGitCommandWithInput("pull origin",
                                "Enter branch name (default: main):")),
                        new TGui.MenuItem("_Push", "",
                            () => GitCommands.RunGitCommandWithInput("push origin",
                                "Enter branch name (default: main):")),
                        new TGui.MenuItem("_Rebase", "",
                            () => GitCommands.RunGitCommandWithInput("rebase", "Enter branch or commit:")),
                        new TGui.MenuItem("_Rebase -i", "",
                            () => GitCommands.RunGitCommandWithInput("rebase -i", "Enter commit or branch:")),
                        new TGui.MenuItem("_Reflog", "", () => GitCommands.ExecuteGitCommand("reflog")),
                        new TGui.MenuItem("_Remote", "", () => GitCommands.ExecuteGitCommand("remote -v")),
                        new TGui.MenuItem("_Reset", "",
                            () => GitCommands.RunGitCommandWithInput("reset",
                                "Enter reset mode (e.g., --soft, --hard) or commit:")),
                        new TGui.MenuItem("_Revert", "",
                            () => GitCommands.RunGitCommandWithInput("revert", "Enter commit to revert:")),
                        new TGui.MenuItem("_Status", "", () => GitCommands.ExecuteGitCommand("status"))
                    }),
                    new TGui.MenuBarItem("_Git Terminology", new TGui.MenuItem[]
                    {
                        new TGui.MenuItem("_Branch", "",
                            () => UiComponents.ShowInfo("Branch",
                                "A branch is a parallel version of a repository, allowing multiple lines of development.")),
                        new TGui.MenuItem("_Centralized Workflow", "",
                            () => UiComponents.ShowInfo("Centralized Workflow",
                                "A workflow where a single repository serves as the central hub for all changes.")),
                        new TGui.MenuItem("_Feature Branch Workflow", "",
                            () => UiComponents.ShowInfo("Feature Branch Workflow",
                                "Developers create feature branches for new features, merging them into main when complete.")),
                        new TGui.MenuItem("_Forking", "",
                            () => UiComponents.ShowInfo("Forking",
                                "Forking creates a personal copy of a repository to work on independently.")),
                        new TGui.MenuItem("_Gitflow Workflow", "",
                            () => UiComponents.ShowInfo("Gitflow Workflow",
                                "A branching model with main, develop, feature, release, and hotfix branches.")),
                        new TGui.MenuItem("_HEAD", "",
                            () => UiComponents.ShowInfo("HEAD",
                                "HEAD is a pointer to the current branch or commit you are working on.")),
                        new TGui.MenuItem("_Hook", "",
                            () => UiComponents.ShowInfo("Hook",
                                "Scripts that run automatically on certain Git events, like pre-commit or post-merge.")),
                        new TGui.MenuItem("_Main", "",
                            () => UiComponents.ShowInfo("Main",
                                "The default branch in a Git repository, often called 'main' or 'master'.")),
                        new TGui.MenuItem("_Pull Request", "",
                            () => UiComponents.ShowInfo("Pull Request",
                                "A request to merge changes from one branch to another, often reviewed by collaborators.")),
                        new TGui.MenuItem("_Repository", "",
                            () => UiComponents.ShowInfo("Repository",
                                "A storage location for a project's files and version history.")),
                        new TGui.MenuItem("_Tag", "",
                            () => UiComponents.ShowInfo("Tag",
                                "A reference to a specific commit, often used to mark release points.")),
                        new TGui.MenuItem("_Version Control", "",
                            () => UiComponents.ShowInfo("Version Control",
                                "A system to manage changes to code or documents over time.")),
                        new TGui.MenuItem("_Working Tree", "",
                            () => UiComponents.ShowInfo("Working Tree",
                                "The current state of files in your working directory, including tracked and untracked files."))
                    }),
                    new TGui.MenuBarItem("_Exit", "", () => TGui.Application.RequestStop())
                });
                win.Add(menu);

                // Load repositories if token is valid
                if (githubService.GitHubClient != null && !string.IsNullOrEmpty(githubService.GitHubToken))
                {
                    await githubService.LoadRepositories(githubStatusLabel, repoListView);
                }

                Logger.Log("Starting Terminal.Gui application loop...");
                TGui.Application.Run();
                Logger.Log("Terminal.Gui application stopped.");
                TGui.Application.Shutdown();
            }
            catch (Exception ex)
            {
                Logger.Log($"Fatal error in Main: {ex.Message}");
                TGui.MessageBox.ErrorQuery("Fatal Error", $"Application crashed: {ex.Message}", "OK");
            }
        }
    }
}