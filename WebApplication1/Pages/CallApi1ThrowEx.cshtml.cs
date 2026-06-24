using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace WebApplication1.Pages
{
    public class CallApi1ThrowEx : PageModel
    {
        private readonly ILogger<CallApi1ThrowEx> _logger;
        private readonly IHttpClientFactory _httpClientFactory;
        public string APIUrl { get; set; }
        public CallApi1ThrowEx(ILogger<CallApi1ThrowEx> logger, IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            APIUrl = config["Api1Url"];
            _logger = logger;
        }
        public async Task OnGet()
        {
            _logger.LogInformation("Get CallApi1ThrowEx ");
            var httpClient = _httpClientFactory.CreateClient();
            await httpClient.GetAsync($"{APIUrl}/ThrowEx");
        }
    }
}
