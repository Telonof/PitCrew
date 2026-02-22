using PitCrewCommon.Utilities;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace PitCrewCommon
{
    public class Updater
    {
        private static HttpClient client = new HttpClient();

        public static JsonElement GrabLatestVersion()
        {
            ResetUserAgent();
            return FetchGithubRelease();
        }

        public static string GrabUpdateName(JsonElement response)
        {
            string githubVer = response.GetProperty("tag_name").ToString();

            return githubVer;
        }

        //Find zip file for either Linux or Windows to download.
        public async static Task<string> GrabZIPFile(JsonElement response, string version)
        {
            Logger.Print(string.Format(Translatable.Get("updater.extracting"), version));

            Stream file = null;
            string filename = "";

            foreach (JsonElement item in response.GetProperty("assets").EnumerateArray())
            {
                filename = Path.GetFileNameWithoutExtension(item.GetProperty("name").ToString());

                if (filename.Contains("Linux") && RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    file = client.GetStreamAsync(item.GetProperty("browser_download_url").ToString()).Result;
                    break;
                }

                if (filename.Contains("Windows") && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    file = client.GetStreamAsync(item.GetProperty("browser_download_url").ToString()).Result;
                    break;
                }
            }

            if (file == null)
                return "";

            ZipFile.ExtractToDirectory(file, FileUtil.GetProcessDir(), true);
            return filename;
        }

        private static void ResetUserAgent()
        {
            client.DefaultRequestHeaders.UserAgent.Clear();
            //https://docs.github.com/en/rest/using-the-rest-api/troubleshooting-the-rest-api?apiVersion=2022-11-28#user-agent-required
            client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("PitCrewUpdater", Constants.VERSION));
        }

        private static JsonElement FetchGithubRelease()
        {
            Task<string> result = client.GetStringAsync("https://api.github.com/repos/Telonof/PitCrew/releases/latest");
            return JsonSerializer.Deserialize<JsonElement>(result.Result);
        }
    }
}
