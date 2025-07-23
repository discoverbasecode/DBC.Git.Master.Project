using System;

namespace DBC.Git.Master.App
{
    public static class Logger
    {
        private const string LogFilePath = "gitmaster_log.txt";

        public static void Log(string message)
        {
            try
            {
                File.AppendAllText(LogFilePath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}\n");
            }
            catch
            {
                // Ignore logging errors
            }
        }
    }
}