using PitCrewCommon;
using PitCrewCommon.Utilities;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace PitCrewUpdater
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            ConfigManager config = new ConfigManager();
            Translatable.Initialize(config.GetSetting(ConfigKey.Language) + ".json");
            
            SetupOnClose();

            if (args.Length == 2)
            {
                ExtractAndClose(args[0], args[1]);
                return;
            }

            //Because this uses Console.ReadKey we need to request they open it from the terminal.
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM")) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Logger.Error(401, Translatable.Get("updater.terminal"));
                return;
            }
            
            JsonElement response = Updater.GrabLatestVersion();

            string githubVer = Updater.GrabUpdateName(response);

            if (githubVer.Equals(Constants.VERSION))
            {
                PrintAndWait(Translatable.Get("updater.latest-version"));
                return;
            }

            //Find zip file for either Linux or Windows to download.
            string updateFolder = Updater.GrabZIPFile(response, githubVer).Result;

            if (string.IsNullOrWhiteSpace(updateFolder))
            {
                PrintAndWait(Translatable.Get("updater.could-not-find-valid-file"));
                return;
            }

            //Ask to close PitCrew so it's files can be updated.
            Process[] processes = Process.GetProcessesByName("PitCrew");
            foreach (Process process in processes)
            {
                string path = FileUtil.GetParentDir(process.MainModule.FileName);
                if (!path.Equals(FileUtil.GetProcessDir()))
                    continue;

                PrintAndWait(Translatable.Get("updater.modloader-running"), Translatable.Get("updater.prompt-modloader-close"));
                process.Kill();
            }
            
            ExtractAndClose(updateFolder, githubVer);
        }

        private static void ExtractAndClose(string updateFolder, string githubVer)
        {
            //Move every file but the updater itself since it's in use.
            string extractedFolder = Path.Combine(FileUtil.GetProcessDir(), updateFolder);

            foreach (string extractedFile in Directory.GetFiles(extractedFolder, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileNameWithoutExtension(extractedFile).Equals("PitCrewUpdater"))
                    continue;

                File.Move(extractedFile, Path.Combine(FileUtil.GetProcessDir(), Path.GetRelativePath(extractedFolder, extractedFile)), true);
            }
        }

        private static void PrintAndWait(string message, string continueMessage = "")
        {
            if (string.IsNullOrWhiteSpace(continueMessage))
                continueMessage = Translatable.Get("updater.prompt-updater-close");

            Logger.Print(message);
            Logger.Print(continueMessage);
            Console.ReadKey();
        }

        private static void SetupOnClose()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c timeout /t 1 && move /Y PitCrew-Windows\\PitCrewUpdater.exe . & rmdir /S /Q PitCrew-Windows & PitCrew.exe",
                        WorkingDirectory = FileUtil.GetProcessDir(),
                        CreateNoWindow = true
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "-c \"mv PitCrew-Linux/PitCrewUpdater ./PitCrewUpdater ; rm -r PitCrew-Linux; ./PitCrew\"",
                        WorkingDirectory = FileUtil.GetProcessDir(),
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            };
        }
    }
}