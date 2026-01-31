using PitCrewCommon;
using System;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;

namespace PitCrew.Systems
{
    internal class DownloadManager
    {
        private string API = "https://api.modworkshop.net/files/";

        private HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });

        public async Task<ZipArchive> DownloadMod(int id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{API}{id}/download");

            try
            {
                var response = await client.SendAsync(request, HttpCompletionOption.ResponseContentRead);
                return new ZipArchive(response.Content.ReadAsStreamAsync().Result, ZipArchiveMode.Read);
            }
            catch (HttpRequestException e)
            {
                Logger.Error(302, Translatable.Get("download.failed"));
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logger.Error(303, Translatable.Get("download.not-a-zip"));
            }

            return null;
        }
    }
}
