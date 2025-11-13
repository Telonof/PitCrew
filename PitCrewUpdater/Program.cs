using PitCrewCommon;
using PitCrewCommon.Utilities;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
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

            //Because this uses Console.ReadKey we need to request they open it from the terminal.
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TERM")) && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Logger.Error(401, Translatable.Get("updater.terminal"));
                return;
            }

            SetupOnClose();

            string filename = "";
            Task<Stream> file = null;
            HttpClient client = new HttpClient();

            //https://docs.github.com/en/rest/using-the-rest-api/troubleshooting-the-rest-api?apiVersion=2022-11-28#user-agent-required
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PitCrewUpdater", Constants.VERSION));

            JsonElement response = FetchGithubRelease(client);

            string githubVer = response.GetProperty("tag_name").ToString();

            if (githubVer.Equals(Constants.VERSION))
            {
                PrintAndWait(Translatable.Get("updater.latest-version"));
                return;
            }

            //Find zip file for either Linux or Windows to download.
            foreach (JsonElement item in response.GetProperty("assets").EnumerateArray())
            {
                filename = Path.GetFileNameWithoutExtension(item.GetProperty("name").ToString());

                if (filename.Contains("Linux") && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    file = client.GetStreamAsync(item.GetProperty("browser_download_url").ToString());
                    break;
                }
                
                if (filename.Contains("Windows") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    file = client.GetStreamAsync(item.GetProperty("browser_download_url").ToString());
                    break;
                }
            }

            if (file == null)
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

            Logger.Print(string.Format(Translatable.Get("updater.extracting"), githubVer));

            //Move every file but the updater itself since it's in use.
            ZipFile.ExtractToDirectory(file.Result, FileUtil.GetProcessDir(), true);
            string extractedFolder = Path.Combine(FileUtil.GetProcessDir(), filename);

            foreach (string extractedFile in Directory.GetFiles(extractedFolder, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileNameWithoutExtension(extractedFile).Equals("PitCrewUpdater"))
                    continue;

                File.Move(extractedFile, Path.Combine(FileUtil.GetProcessDir(), Path.GetRelativePath(extractedFolder, extractedFile)), true);
            }

            PrintAndWait(string.Format(Translatable.Get("updater.finished"), githubVer));
        }

        private static JsonElement FetchGithubRelease(HttpClient client)
        {
            Task<string> result = client.GetStringAsync("https://api.github.com/repos/Telonof/PitCrew/releases/latest");
            return JsonSerializer.Deserialize<JsonElement>(result.Result);
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
                        Arguments = "/c timeout /t 1 && move /Y PitCrew-Windows\\PitCrewUpdater.exe . & rmdir /S /Q PitCrew-Windows",
                        WorkingDirectory = FileUtil.GetProcessDir(),
                        CreateNoWindow = true
                    });
                }
                else
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = "-c \"mv PitCrew-Linux/PitCrewUpdater ./PitCrewUpdater ; rm -r PitCrew-Linux\"",
                        WorkingDirectory = FileUtil.GetProcessDir(),
                        CreateNoWindow = true,
                        UseShellExecute = false
                    });
                }
            };
        }
    }
}