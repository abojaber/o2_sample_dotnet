using Microsoft.AspNetCore.Mvc;

namespace ApiApplication1.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastIn2Controller : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastIn2Controller> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public string APIUrl { get; set; }
        public WeatherForecastIn2Controller(ILogger<WeatherForecastIn2Controller> logger, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
            APIUrl = config["Api2Url"];
        }

        [HttpGet(Name = "GetWeatherForecastIn2")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var httpClient = _httpClientFactory.CreateClient();
            await httpClient.GetAsync($"{APIUrl}/WeatherForecast");

            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}