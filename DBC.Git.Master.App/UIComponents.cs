using Octokit;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TGui = Terminal.Gui;
using System.Collections.Generic;

namespace DBC.Git.Master.App;

public static class UiComponents
{
    public static async void ShowRepositoryContents(GitHubService? githubService, TGui.ListView repoListView)
    {
        Logger.Log("View Repository Contents button clicked.");
        if (repoListView.SelectedItem == -1 || githubService?.GitHubClient == null)
        {
            TGui.MessageBox.ErrorQuery("Error", "Please connect to GitHub and select a repository.", "OK");
            Logger.Log("Error: No repository selected or GitHub client not initialized.");
            return;
        }

        var selectedRepo = repoListView.Source.ToList()[repoListView.SelectedItem] as string;
        var repoName = selectedRepo?.Split('/')[1];
        var owner = selectedRepo?.Split('/')[0];
        if (repoName != null && owner != null)
        {
            Logger.Log($"Showing contents for repository: {owner}/{repoName}");
            ShowRepositoryContents(githubService, owner, repoName, "");
        }
        else
        {
            Logger.Log("Error: Invalid repository selection.");
            TGui.MessageBox.ErrorQuery("Error", "Invalid repository selection.", "OK");
        }
    }

    private static async void ShowRepositoryContents(GitHubService githubService, string owner, string repoName,
        string path)
    {
        Logger.Log($"Opening repository contents for {owner}/{repoName} at path: {path}...");
        if (githubService.GitHubClient == null)
        {
            TGui.MessageBox.ErrorQuery("Error", "GitHub client not initialized.", "OK");
            Logger.Log("Error: GitHub client not initialized.");
            return;
        }

        var dialog = new TGui.Dialog($"Contents of {owner}/{repoName}/{path}", 80, 20);
        var fileListView = new TGui.ListView
        {
            X = TGui.Pos.At(1),
            Y = TGui.Pos.At(1),
            Width = TGui.Dim.Fill() - 1,
            Height = TGui.Dim.Fill() - 3
        };
        dialog.Add(fileListView);

        try
        {
            Logger.Log($"Fetching contents for path: {path}...");
            IReadOnlyList<RepositoryContent> contents;
            if (string.IsNullOrEmpty(path))
            {
                contents = await githubService.GitHubClient.Repository.Content.GetAllContents(owner, repoName);
            }
            else
            {
                contents = await githubService.GitHubClient.Repository.Content
                    .GetAllContents(owner, repoName, path);
            }

            var items = contents.Select(c => $"{(c.Type == ContentType.Dir ? "[DIR] " : "")}{c.Path}").ToList();
            fileListView.SetSource(items);
            Logger.Log($"Loaded {contents.Count} items at path: {path}");

            var viewButton = new TGui.Button("Open/View")
            {
                X = TGui.Pos.At(1),
                Y = TGui.Pos.At(17)
            };
            viewButton.Clicked += async () =>
            {
                if (fileListView.SelectedItem == -1)
                {
                    Logger.Log("No item selected.");
                    TGui.MessageBox.ErrorQuery("Error", "Please select a file or folder.", "OK");
                    return;
                }

                var selectedItem = contents[fileListView.SelectedItem];
                var selectedPath = selectedItem.Path;
                Logger.Log($"Selected item: {selectedPath} (Type: {selectedItem.Type})");

                if (selectedItem.Type == ContentType.Dir)
                {
                    Logger.Log($"Navigating to folder: {selectedPath}");
                    TGui.Application.RequestStop(dialog);
                    ShowRepositoryContents(githubService, owner, repoName, selectedPath);
                }
                else if (selectedItem.Type == ContentType.File)
                {
                    Logger.Log($"Viewing file: {selectedPath}");
                    try
                    {
                        var fileContent =
                            await githubService.GitHubClient.Repository.Content.GetAllContents(owner, repoName,
                                selectedPath);
                        var content = fileContent.FirstOrDefault()?.Content ?? "No content available.";
                        var contentDialog = new TGui.Dialog($"Content of {selectedPath}", 80, 20);
                        contentDialog.Add(new TGui.TextView
                        {
                            X = TGui.Pos.At(1),
                            Y = TGui.Pos.At(1),
                            Width = TGui.Dim.Fill() - 1,
                            Height = TGui.Dim.Fill() - 3,
                            Text = content,
                            ReadOnly = true
                        });
                        var closeContentButton = new TGui.Button("Close");
                        closeContentButton.Clicked += () =>
                        {
                            Logger.Log("Closing file content dialog.");
                            TGui.Application.RequestStop(contentDialog);
                        };
                        contentDialog.AddButton(closeContentButton);
                        Logger.Log("Opening file content dialog.");
                        TGui.Application.Run(contentDialog);
                    }
                    catch (Exception ex)
                    {
                        TGui.MessageBox.ErrorQuery("Error", $"Failed to load file content: {ex.Message}", "OK");
                        Logger.Log($"Error loading file content: {ex.Message}");
                    }
                }
                else
                {
                    TGui.MessageBox.ErrorQuery("Error", $"Unsupported item type: {selectedItem.Type}", "OK");
                    Logger.Log($"Unsupported item type: {selectedItem.Type}");
                }
            };
            dialog.AddButton(viewButton);

            var backButton = new TGui.Button("Back")
            {
                X = TGui.Pos.Right(viewButton) + 2,
                Y = TGui.Pos.At(17)
            };
            backButton.Clicked += () =>
            {
                if (!string.IsNullOrEmpty(path))
                {
                    var parentPath = Path.GetDirectoryName(path)?.Replace('\\', '/') ?? "";
                    Logger.Log($"Navigating back to parent path: {parentPath}");
                    TGui.Application.RequestStop(dialog);
                    ShowRepositoryContents(githubService, owner, repoName, parentPath);
                }
                else
                {
                    Logger.Log("Closing repository contents dialog (at root).");
                    TGui.Application.RequestStop(dialog);
                }
            };
            dialog.AddButton(backButton);

            var closeButton = new TGui.Button("Close")
            {
                X = TGui.Pos.Right(backButton) + 2,
                Y = TGui.Pos.At(17)
            };
            closeButton.Clicked += () =>
            {
                Logger.Log("Closing repository contents dialog.");
                TGui.Application.RequestStop(dialog);
            };
            dialog.AddButton(closeButton);
        }
        catch (Exception ex)
        {
            TGui.MessageBox.ErrorQuery("Error", $"Failed to load repository contents: {ex.Message}", "OK");
            Logger.Log($"Error loading repository contents: {ex.Message}");
        }

        Logger.Log("Opening repository contents dialog.");
        TGui.Application.Run(dialog);
    }

    public static async void AddFileToRepository(GitHubService? githubService, TGui.ListView repoListView)
    {
        Logger.Log("Opening dialog to add file to repository...");
        if (repoListView.SelectedItem == -1 || githubService?.GitHubClient == null)
        {
            TGui.MessageBox.ErrorQuery("Error", "Please connect to GitHub and select a repository.", "OK");
            Logger.Log("Error: No repository selected or GitHub client not initialized.");
            return;
        }

        var selectedRepo = repoListView.Source.ToList()[repoListView.SelectedItem] as string;
        var repoName = selectedRepo?.Split('/')[1];
        var owner = selectedRepo?.Split('/')[0];
        if (repoName == null || owner == null)
        {
            TGui.MessageBox.ErrorQuery("Error", "Invalid repository selection.", "OK");
            Logger.Log("Error: Invalid repository selection.");
            return;
        }

        var dialog = new TGui.Dialog("Add File to Repository", 60, 20);
        var localPathLabel = new TGui.Label("Local File Path (e.g., D:\\MyFiles\\Example.cs):")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(2),
            Width = TGui.Dim.Fill() - 2
        };
        var localPathField = new TGui.TextField("")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(4),
            Width = 50
        };
        var repoPathLabel = new TGui.Label("Repository Path (e.g., src/Example.cs):")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(6),
            Width = TGui.Dim.Fill() - 2
        };
        var repoPathField = new TGui.TextField("")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(8),
            Width = 50
        };
        var branchLabel = new TGui.Label("Branch (default: main):")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(10),
            Width = TGui.Dim.Fill() - 2
        };
        var branchField = new TGui.TextField("main")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(12),
            Width = 50
        };
        var commitMessageLabel = new TGui.Label("Commit Message:")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(14),
            Width = TGui.Dim.Fill() - 2
        };
        var commitMessageField = new TGui.TextField("Add new file from local")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(16),
            Width = 50
        };
        var okButton = new TGui.Button("OK")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(18),
            IsDefault = true
        };
        okButton.Clicked += async () =>
        {
            Logger.Log("OK button clicked in Add File dialog.");
            var localPath = localPathField.Text.ToString();
            var repoPath = repoPathField.Text.ToString();
            var branch = branchField.Text.ToString();
            var commitMessage = commitMessageField.Text.ToString();
            if (string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(repoPath) ||
                string.IsNullOrEmpty(branch) || string.IsNullOrEmpty(commitMessage))
            {
                TGui.MessageBox.ErrorQuery("Error",
                    "Please provide local file path, repository path, branch, and commit message.", "OK");
                Logger.Log("Error: Missing local file path, repository path, branch, or commit message.");
                return;
            }

            if (!File.Exists(localPath))
            {
                TGui.MessageBox.ErrorQuery("Error", $"File {localPath} does not exist.", "OK");
                Logger.Log($"Error: File {localPath} does not exist.");
                return;
            }

            Logger.Log($"Reading file from {localPath}...");
            string content;
            try
            {
                content = await File.ReadAllTextAsync(localPath);
            }
            catch (Exception ex)
            {
                TGui.MessageBox.ErrorQuery("Error", $"Failed to read file {localPath}: {ex.Message}", "OK");
                Logger.Log($"Error reading file {localPath}: {ex.Message}");
                return;
            }

            Logger.Log($"Adding file {repoPath} to {owner}/{repoName} on branch {branch}...");
            try
            {
                var request = new CreateFileRequest(commitMessage, content, branch);
                var result =
                    await githubService.GitHubClient.Repository.Content.CreateFile(owner, repoName, repoPath,
                        request);
                TGui.MessageBox.Query("Success",
                    $"File {repoPath} added successfully to {owner}/{repoName} on branch {branch}.", "OK");
                Logger.Log($"File {repoPath} added successfully. Commit SHA: {result.Commit.Sha}");
                TGui.Application.RequestStop(dialog);
            }
            catch (Exception ex)
            {
                TGui.MessageBox.ErrorQuery("Error", $"Failed to add file: {ex.Message}", "OK");
                Logger.Log($"Error adding file {repoPath}: {ex.Message}");
            }
        };
        var cancelButton = new TGui.Button("Cancel")
        {
            X = TGui.Pos.Right(okButton) + 2,
            Y = TGui.Pos.At(18)
        };
        cancelButton.Clicked += () =>
        {
            Logger.Log("Cancel button clicked in Add File dialog.");
            TGui.Application.RequestStop(dialog);
        };

        dialog.KeyPress += (args) =>
        {
            if (args.KeyEvent.Key == TGui.Key.Enter)
            {
                Logger.Log("Enter key pressed in Add File dialog.");
                okButton.OnClicked();
                args.Handled = true;
            }
        };

        dialog.Add(localPathLabel, localPathField, repoPathLabel, repoPathField, branchLabel, branchField,
            commitMessageLabel, commitMessageField, okButton, cancelButton);
        Logger.Log("Opening add file dialog with OK and Cancel buttons.");
        TGui.Application.Run(dialog);
    }

    public static async void DeleteRepository(GitHubService? githubService, TGui.ListView repoListView)
    {
        Logger.Log("Opening dialog to delete repository...");
        if (repoListView.SelectedItem == -1 || githubService?.GitHubClient == null)
        {
            TGui.MessageBox.ErrorQuery("Error", "Please connect to GitHub and select a repository.", "OK");
            Logger.Log("Error: No repository selected or GitHub client not initialized.");
            return;
        }

        var selectedRepo = repoListView.Source.ToList()[repoListView.SelectedItem] as string;
        var repoName = selectedRepo?.Split('/')[1];
        var owner = selectedRepo?.Split('/')[0];
        if (repoName == null || owner == null)
        {
            TGui.MessageBox.ErrorQuery("Error", "Invalid repository selection.", "OK");
            Logger.Log("Error: Invalid repository selection.");
            return;
        }

        var confirmDialog = new TGui.Dialog("Confirm Delete", 60, 10);
        confirmDialog.Add(
            new TGui.Label($"Are you sure you want to delete {owner}/{repoName}? This action cannot be undone.")
            {
                X = TGui.Pos.At(2),
                Y = TGui.Pos.At(2),
                Width = TGui.Dim.Fill() - 2
            });
        var yesButton = new TGui.Button("Yes")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(6)
        };
        yesButton.Clicked += async () =>
        {
            Logger.Log($"Deleting repository {owner}/{repoName}...");
            try
            {
                await githubService.GitHubClient.Repository.Delete(owner, repoName);
                TGui.MessageBox.Query("Success", $"Repository {owner}/{repoName} deleted successfully.", "OK");
                Logger.Log($"Repository {owner}/{repoName} deleted successfully.");
                repoListView.SetSource(new List<string>());
                TGui.Application.RequestStop(confirmDialog);
            }
            catch (Exception ex)
            {
                TGui.MessageBox.ErrorQuery("Error", $"Failed to delete repository: {ex.Message}", "OK");
                Logger.Log($"Error deleting repository {owner}/{repoName}: {ex.Message}");
            }
        };
        var noButton = new TGui.Button("No")
        {
            X = TGui.Pos.Right(yesButton) + 2,
            Y = TGui.Pos.At(6)
        };
        noButton.Clicked += () =>
        {
            Logger.Log("Delete repository dialog cancelled.");
            TGui.Application.RequestStop(confirmDialog);
        };
        confirmDialog.Add(yesButton, noButton);
        Logger.Log("Opening delete repository confirmation dialog.");
        TGui.Application.Run(confirmDialog);
    }

    public static void ShowInfo(string term, string description)
    {
        Logger.Log($"Opening info dialog for term: {term}");
        var dialog = new TGui.Dialog(term, 60, 15);
        dialog.Add(new TGui.TextView
        {
            X = TGui.Pos.At(1),
            Y = TGui.Pos.At(1),
            Width = TGui.Dim.Fill() - 1,
            Height = TGui.Dim.Fill() - 3,
            Text = description,
            ReadOnly = true
        });
        var okButton = new TGui.Button("OK");
        okButton.Clicked += () =>
        {
            Logger.Log("Closing info dialog.");
            TGui.Application.RequestStop(dialog);
        };
        dialog.AddButton(okButton);
        TGui.Application.Run(dialog);
    }
}