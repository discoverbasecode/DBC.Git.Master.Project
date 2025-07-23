using Octokit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TGui = Terminal.Gui;

namespace DBC.Git.Master.App
{
    public class GitHubService
    {
        public GitHubClient? GitHubClient { get; set; }
        public string? GitHubToken { get; set; }

        public async Task LoadGitHubToken()
        {
            await GitHubConfigManager.LoadGitHubToken(this, null);
        }

        public async Task Connect(TGui.Label statusLabel, TGui.ListView repoListView)
        {
            if (string.IsNullOrEmpty(GitHubToken))
            {
                statusLabel.Text = "Error: Please enter a valid GitHub token.";
                Logger.Log("Error: Empty GitHub token.");
                return;
            }

            GitHubClient = new GitHubClient(new ProductHeaderValue("GitMaster"))
            {
                Credentials = new Credentials(GitHubToken)
            };

            try
            {
                Logger.Log("Testing GitHub token by fetching current user...");
                var user = await GitHubClient.User.Current();
                Logger.Log($"User fetched: {user.Login}");
                statusLabel.Text = $"Connected as: {user.Login}";

                Logger.Log("Fetching repositories...");
                var repos = await GitHubClient.Repository.GetAllForCurrent();
                Logger.Log($"Fetched {repos.Count} repositories.");
                repoListView.SetSource(repos.Select(r => r.FullName).ToList());
                Logger.Log("Repositories loaded.");

                // Save token
                GitHubConfigManager.SaveGitHubToken(GitHubToken, statusLabel);
                Logger.Log("GitHub token saved.");
            }
            catch (Octokit.AuthorizationException authEx)
            {
                statusLabel.Text = "Error: Invalid GitHub token.";
                Logger.Log($"Authorization error in Connect: {authEx.Message}");
                GitHubToken = null;
                GitHubClient = null;
            }
            catch (Octokit.RateLimitExceededException rateEx)
            {
                statusLabel.Text = $"Error: Rate limit exceeded. Try again after {rateEx.Reset.ToLocalTime()}.";
                Logger.Log($"Rate limit exceeded in Connect: {rateEx.Message}");
                GitHubToken = null;
                GitHubClient = null;
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"GitHub Error: {ex.Message}";
                Logger.Log($"GitHub Error in Connect: {ex.Message}");
                GitHubToken = null;
                GitHubClient = null;
            }
        }

        public void ResetToken(TGui.Label statusLabel, TGui.TextField tokenField, TGui.ListView repoListView)
        {
            GitHubToken = null;
            GitHubClient = null;
            if (File.Exists("github_config.json"))
            {
                try
                {
                    File.Delete("github_config.json");
                    Logger.Log("GitHub token config file deleted.");
                }
                catch (Exception ex)
                {
                    Logger.Log($"Error deleting config file: {ex.Message}");
                    statusLabel.Text = $"Error deleting config file: {ex.Message}";
                }
            }

            statusLabel.Text = "GitHub token reset. Please enter a new token.";
            tokenField.Text = "";
            repoListView.SetSource(new List<string>());
        }

        public async Task LoadRepositories(TGui.Label statusLabel, TGui.ListView repoListView)
        {
            try
            {
                Logger.Log("Loading repositories with existing token...");
                Logger.Log("Attempting to fetch current user...");
                var user = await GitHubClient.User.Current();
                Logger.Log($"User fetched: {user.Login}");
                statusLabel.Text = $"Connected as: {user.Login}";
                Logger.Log("Attempting to fetch repositories...");
                var repos = await GitHubClient.Repository.GetAllForCurrent();
                Logger.Log($"Fetched {repos.Count} repositories.");
                repoListView.SetSource(repos.Select(r => r.FullName).ToList());
                Logger.Log("Repositories loaded successfully.");
            }
            catch (Octokit.AuthorizationException authEx)
            {
                Logger.Log($"Authorization error: {authEx.Message}");
                statusLabel.Text = "GitHub token invalid. Please enter a new token.";
                GitHubToken = null;
                GitHubClient = null;
            }
            catch (Octokit.RateLimitExceededException rateEx)
            {
                Logger.Log($"Rate limit exceeded: {rateEx.Message}");
                statusLabel.Text = $"Error: Rate limit exceeded. Try again after {rateEx.Reset.ToLocalTime()}.";
                GitHubToken = null;
                GitHubClient = null;
            }
            catch (Exception ex)
            {
                Logger.Log($"Error loading repositories: {ex.Message}");
                statusLabel.Text = $"Error loading repositories: {ex.Message}";
                GitHubToken = null;
                GitHubClient = null;
            }
        }
    }
}