using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Updater
{
    class Program
    {
        static string logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater_log.txt");

        static void Log(string message)
        {
            string line = $"{DateTime.Now:yyyy/MM/dd HH:mm:ss}: {message}";
            File.AppendAllText(logFile, line + Environment.NewLine);
        }

        static void Main(string[] args)
        {
            Log("Updater started.");
            Log($"arg 1 {args[0]}");
            Log($"arg 2 {args[1]}");
            Log($"arg 3 {args[2]}");
            if (args.Length < 3)
            {
                Log("Usage: Updater.exe <zipPath> <targetDir> <exeToLaunch>");
                return;
            }

            string zipPath = args[0];
            string targetDir = args[1];
            string exeToLaunch = args[2];

            Log($"Zip path: {zipPath}");
            Log($"Target dir: {targetDir}");
            Log($"Exe to launch: {exeToLaunch}");

            try
            {
                Thread.Sleep(1000); 

                string tempExtractDir = Path.Combine(Path.GetTempPath(), "TCFInventoryBoss_Extract");

                if (Directory.Exists(tempExtractDir))
                    Directory.Delete(tempExtractDir, true);

                ZipFile.ExtractToDirectory(zipPath, tempExtractDir);
                Log("Zip extracted to temp directory.");
               
                foreach (string filePath in Directory.GetFiles(tempExtractDir, "*", SearchOption.AllDirectories))
                {
                    string relativePath = filePath.Substring(tempExtractDir.Length + 1);
                    string destPath = Path.Combine(targetDir, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                    File.Copy(filePath, destPath, true);
                }

                Log("Files copied to target directory.");

                Process.Start(new ProcessStartInfo
                {
                    FileName = exeToLaunch,
                    UseShellExecute = true
                });

                Log("Updated application started.");
            }
            catch (Exception ex)
            {
                Log("Update failed: " + ex.Message);
            }
        }
    }
}
