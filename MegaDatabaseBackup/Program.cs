using CommandLine;
using System;
using System.Collections.Generic;
using System.IO;
using ShellProgressBar;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MegaDatabaseBackup
{
    internal class Program
    {
        [Verb("backup", HelpText = "Backup MongoDB Database to mega.nz Folder")]
        public class BackupOptions
        {
            [Option("background",
              Default = false,
              HelpText = "Background Mode no progress (Needed for Node.js)")]
            public bool Background { get; set; }

            [Option("workingDir",
              Default = @".\",
              HelpText = "Working Directory (Needed for Node.js)")]
            public string WorkingDir { get; set; }

            [Option("db",
              Default = "RisDb",
              HelpText = "MongoDB Database Name")]
            public string DbName { get; set; }

            [Option("backupName",
              Default = "Benlaghouati",
              HelpText = "MongoDB Database Name")]
            public string BackupName { get; set; }

            [Option("host",
              Default = "127.0.0.1",
              HelpText = "MongoDB Host")]
            public string Host { get; set; }

            [Option("email",
              Required = true,
              HelpText = "mega.nz Account Email")]
            public string Email { get; set; }

            [Option("password",
              Required = true,
              HelpText = "mega.nz Account Password")]
            public string Password { get; set; }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<BackupOptions>(args)
                .WithParsedAsync(RunOptions);
            Console.ReadKey();
        }

        private static async Task RunOptions(BackupOptions opts)
        {
            var backupDir = new DirectoryInfo(opts.WorkingDir)
                .Parent
                .CreateSubdirectory("Backup")
                .FullName;
            var outputFile = Path.Combine(backupDir, $"{opts.BackupName}_{DateTime.Now.ToString("yyyy-MM-dd_HH-mm")}.gz");

            App.CreateBackup(opts.DbName, outputFile, opts.Host, opts.WorkingDir);

            ProgressBar progressBar = new ProgressBar(10000, $"Uploading Database Backup: {outputFile}", new ProgressBarOptions()
            {
                ProgressBarOnBottom = true,
                DisplayTimeInRealTime = true,
            });
            IProgress<double> progress = progressBar.AsProgress<double>();

            Progress<double> uploadProgress = new Progress<double>((x) =>
            {
                progress.Report(x / 100);
            });

            if (opts.Background)
            {
                App.UploadFileSync(opts.Email, opts.Password, opts.BackupName, outputFile);
            }
            else
            {
                await App.UploadFile(opts.Email, opts.Password, opts.BackupName, outputFile, uploadProgress);
            }

            Process.GetCurrentProcess().Kill();
        }

        private static void HandleParseError(IEnumerable<Error> errs)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}