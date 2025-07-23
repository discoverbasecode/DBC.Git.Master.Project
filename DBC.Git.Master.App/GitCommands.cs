using System.Diagnostics;
using System.Text;
using TGui = Terminal.Gui;

namespace DBC.Git.Master.App;

public static class GitCommands
{
    public static void ExecuteGitCommand(string arguments)
    {
        Logger.Log($"Executing git command: {arguments}");
        using (Process process = new Process())
        {
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;

            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();
                Logger.Log($"Git command output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Logger.Log($"Git command error: {error}");

                var dialog = new TGui.Dialog("Command Output", 80, 20);
                dialog.Add(new TGui.TextView
                {
                    X = TGui.Pos.At(1),
                    Y = TGui.Pos.At(1),
                    Width = TGui.Dim.Fill() - 1,
                    Height = TGui.Dim.Fill() - 3,
                    Text = output + (string.IsNullOrEmpty(error) ? "" : $"\nError: {error}"),
                    ReadOnly = true
                });
                var okButton = new TGui.Button("OK");
                okButton.Clicked += () =>
                {
                    Logger.Log("Closing git command output dialog.");
                    TGui.Application.RequestStop(dialog);
                };
                dialog.AddButton(okButton);
                Logger.Log("Opening git command output dialog.");
                TGui.Application.Run(dialog);
            }
            catch (Exception ex)
            {
                TGui.MessageBox.ErrorQuery("Error", $"Failed to execute git {arguments}: {ex.Message}", "OK");
                Logger.Log($"Error executing git command: {ex.Message}");
            }
        }
    }

    public static void RunGitCommandWithInput(string command, string prompt)
    {
        Logger.Log($"Opening input dialog for git command: {command}");
        var dialog = new TGui.Dialog("Input", 50, 10);
        var label = new TGui.Label(prompt) { X = TGui.Pos.At(2), Y = TGui.Pos.At(2) };
        var input = new TGui.TextField("") { X = TGui.Pos.At(2), Y = TGui.Pos.At(4), Width = 40 };
        var okButton = new TGui.Button("OK")
        {
            X = TGui.Pos.At(2),
            Y = TGui.Pos.At(6)
        };
        okButton.Clicked += () =>
        {
            string? value = input.Text?.ToString();
            if (string.IsNullOrEmpty(value) && (command.Contains("pull") || command.Contains("push")))
                value = "main";
            Logger.Log($"Git command input: {value ?? "null"}");
            ExecuteGitCommand($"{command} {(string.IsNullOrEmpty(value) ? "" : value)}");
            TGui.Application.RequestStop(dialog);
        };
        dialog.Add(label, input, okButton);
        Logger.Log("Opening input dialog.");
        TGui.Application.Run(dialog);
    }
}