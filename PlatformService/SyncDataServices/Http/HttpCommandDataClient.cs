using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PlatformService.Dtos;

namespace PlatformService.SyncDataServices.Http
{
    public class HttpCommandDataClient : ICommandDataClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public HttpCommandDataClient(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _configuration = config;
        }

        public async Task SendPlatformToCommant(PlatformReadDto platform)
        {
            var httpContent = new StringContent(JsonSerializer.Serialize(platform), Encoding.UTF8, "applicatio/json");

            var response = await _httpClient.PostAsync($"http://commands-clusterip-srv:80/api/c/platforms/", httpContent);

            if(response.IsSuccessStatusCode)
            {
                System.Console.WriteLine("Sync POST to CommandService was OK");
            }
            else
            {
                System.Console.WriteLine("Sync POST to CommandService failed");
            }
        }
    }
}