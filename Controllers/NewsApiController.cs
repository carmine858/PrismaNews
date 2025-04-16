using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Configuration;


namespace PrismaNews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsApiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public NewsApiController(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchNews(string query)
        {
            var apiKey = _configuration["GNewsApiKey"];
            var url = $"https://gnews.io/api/v4/search?q={query}&lang=it&token={apiKey}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "Errore nella richiesta alla News API");

            var json = await response.Content.ReadAsStringAsync();
            return Ok(JsonDocument.Parse(json));
        }
    }
}
