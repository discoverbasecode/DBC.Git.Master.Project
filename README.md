# GitMaster CLI

## Overview
GitMaster CLI is a C# command-line interface (CLI) application built with Terminal.Gui and Octokit to interact with GitHub and manage local Git repositories. It provides a user-friendly terminal-based interface for performing common Git operations and GitHub repository management tasks, making it ideal for developers who prefer a GUI-like experience in a terminal environment.

## Features
- **GitHub Integration**:
  - Authenticate with GitHub using a personal access token.
  - View and navigate repository contents.
  - Add files to GitHub repositories from local paths.
  - Delete GitHub repositories.
  - List all repositories for the authenticated user.

- **Git Commands**:
  - Execute common Git commands (e.g., `add`, `commit`, `push`, `pull`, `branch`, etc.) directly from the interface.
  - Support for commands requiring user input (e.g., commit messages, branch names).
  - Display command output and errors in a dialog.

- **Git Terminology**:
  - Built-in glossary of common Git terms (e.g., branch, pull request, HEAD) with descriptions for educational purposes.

- **Logging**:
  - Logs all actions and errors to a `gitmaster_log.txt` file for debugging and tracking.

## Purpose
GitMaster CLI is designed to simplify Git and GitHub workflows for developers, especially those who want a lightweight, terminal-based tool to:
- Manage GitHub repositories without leaving the terminal.
- Execute Git commands with a guided interface, reducing the need to memorize syntax.
- Learn Git terminology through an integrated reference.
- Debug issues with detailed logging.

## Use Cases
- **Developers**: Streamline Git operations and GitHub repository management in a single interface.
- **Beginners**: Learn Git commands and terminology through an interactive, user-friendly tool.
- **Automation**: Perform repetitive Git tasks with a consistent interface, reducing errors.
- **Debugging**: Use logging to troubleshoot issues with Git commands or GitHub API interactions.

## Prerequisites
- [.NET SDK](https://dotnet.microsoft.com/download) (version compatible with the project).
- [Terminal.Gui](https://github.com/migueldeicaza/gui.cs) for the CLI interface.
- [Octokit](https://github.com/octokit/octokit.net) for GitHub API interactions.
- A GitHub personal access token with appropriate permissions (e.g., `repo` scope).

## Setup
1. Clone the repository:
   ```bash
   git clone <repository-url>
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Build and run the project:
   ```bash
   dotnet run
   ```
4. Enter your GitHub personal access token when prompted to connect to GitHub.

## Usage
- **GitHub Connection**: Enter your GitHub token and click "Connect to GitHub" to authenticate and load your repositories.
- **Repository Management**: Select a repository to view its contents, add files, or delete it.
- **Git Commands**: Use the "Git Commands" menu to execute commands like `commit`, `push`, or `status`.
- **Git Terminology**: Access the "Git Terminology" menu to learn about Git concepts.
- **Reset Token**: Clear the saved GitHub token if needed.

## File Structure
- `Program.cs`: Main entry point and UI setup.
- `GitHubConfig.cs`: Manages GitHub token loading and saving.
- `Logger.cs`: Handles application logging.
- `GitHubService.cs`: Manages GitHub API interactions.
- `UIComponents.cs`: Contains UI-related logic for dialogs and repository operations.
- `GitCommands.cs`: Executes Git commands and handles input dialogs.

## Notes
- Logs are saved to `gitmaster_log.txt` in the project directory.
- GitHub token is stored in `github_config.json` for persistence.
- Ensure Git is installed and accessible from the command line for local Git commands to work.
