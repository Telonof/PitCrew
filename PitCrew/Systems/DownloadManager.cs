using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PitCrew.Systems
{
    internal class DownloadManager
    {
        private string API = "https://api.modworkshop.net/files/";

        private HttpClient client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = true });

        public async Task<Stream> DownloadMod(string id)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, $"{API}{id}/download");
            var response = await client.SendAsync(request);
            return await response.Content.ReadAsStreamAsync();
        }
    }
}
