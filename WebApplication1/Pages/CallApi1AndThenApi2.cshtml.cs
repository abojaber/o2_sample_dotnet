using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication1.Pages
{
    public class CallApi1AndThenApi2 : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public string APIUrl { get; set; }
        public CallApi1AndThenApi2(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            APIUrl = config["Api1Url"];
        }
        public async Task OnGet()
        {
            var httpClient = _httpClientFactory.CreateClient();
            for (int i = 0; i < 10; i++)
            {
                await httpClient.GetAsync($"{APIUrl}/WeatherForecastIn2");
            }
        }
    }
}
