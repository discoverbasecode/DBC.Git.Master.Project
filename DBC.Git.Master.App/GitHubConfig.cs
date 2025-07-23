using System.Text.Json;
using TGui = Terminal.Gui;

namespace DBC.Git.Master.App;

public class GitHubConfig
{
    public string? Token { get; set; }
}

public static class GitHubConfigManager
{
    private const string ConfigFilePath = "github_config.json";

    public static async Task LoadGitHubToken(GitHubService service, TGui.Label? statusLabel)
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                Logger.Log("Reading GitHub config file...");
                var json = await File.ReadAllTextAsync(ConfigFilePath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var config = JsonSerializer.Deserialize<GitHubConfig>(json, options);
                if (config?.Token != null)
                {
                    service.GitHubToken = config.Token;
                    service.GitHubClient = new Octokit.GitHubClient(new Octokit.ProductHeaderValue("GitMaster"))
                    {
                        Credentials = new Octokit.Credentials(config.Token)
                    };
                    Logger.Log("GitHub token loaded successfully.");
                }
                else
                {
                    Logger.Log("GitHub token not found in config file.");
                    if (statusLabel != null)
                        statusLabel.Text = "No valid GitHub token found. Please enter a token.";
                }
            }
            else
            {
                Logger.Log("GitHub config file not found.");
                if (statusLabel != null)
                    statusLabel.Text = "No GitHub token found. Please enter a token.";
            }
        }
        catch (Exception ex)
        {
            Logger.Log($"Error loading GitHub token: {ex.Message}");
            if (statusLabel != null)
                statusLabel.Text = $"Error loading GitHub token: {ex.Message}";
        }
    }

    public static void SaveGitHubToken(string? token, TGui.Label? statusLabel)
    {
        try
        {
            var config = new GitHubConfig
            {
                Token = token
            };
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(config, options);
            File.WriteAllText(ConfigFilePath, json);
            Logger.Log("GitHub token saved to config file.");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error saving GitHub token: {ex.Message}");
            if (statusLabel != null)
                statusLabel.Text = $"Failed to save GitHub token: {ex.Message}";
        }
    }
}