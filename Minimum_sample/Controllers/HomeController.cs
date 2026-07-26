using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using OpenTelemetry.Metrics;
using System.Diagnostics.Metrics;
using OpenObserveDemo.Lib;

namespace OpenObserveDemo.Controllers
{
    [ApiController]
    [Route("/")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TestService _testService;
        private readonly DelayService _delayService;
        private static readonly ActivitySource ActivitySource = new("otlp-demo-other");
        private static readonly Meter Meter = new("otlp-demo-other");
        private static readonly Counter<int> PageHitCounter = Meter.CreateCounter<int>("timepage_hits");
        private static readonly Histogram<double> ResponseTimeHistogram = Meter.CreateHistogram<double>("response_time_ms");
        private static readonly UpDownCounter<int> ActiveRequestCounter = Meter.CreateUpDownCounter<int>("active_requests");
        private static readonly Random Rng = new();

        public HomeController(ILogger<HomeController> logger, TestService testService, DelayService delayService)
        {
            _logger = logger;
            _testService = testService;
            _delayService = delayService;
        }

        [HttpGet]
        public async Task<ContentResult> GetPage()
        {
            using var activity = ActivitySource.StartActivity("GetTimePage");
            if (activity != null) activity.SetStatus(ActivityStatusCode.Ok);

            PageHitCounter.Add(1, new KeyValuePair<string, object?>("metric_x", "GetPageHome"));


            var act = Activity.Current;
            act?.SetTag("user.id", "request.userId");
            act?.SetTag("order.count", DateTime.Now.ToString());
            act?.AddEvent(new ActivityEvent("TimePageRequested"));

            _logger.LogInformation("Time page was requested at {time}", DateTime.UtcNow);
            _logger.LogError("This is a sample error log for demonstration purposes.");
            _logger.LogDebug("This is a sample debug log for demonstration purposes.");

            var now = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var randomX = Rng.Next(1, 100);
            var randomY = Rng.Next(1, 100);
            await _delayService.DelayFor(new Random().Next(1000, 2000));
            int result = await _testService.Subtractor(randomX, randomY);
            var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Current Time</title>
                </head>
                <body>
                    <h1>Current Time: is</h1>
                    <p>The time now is: <strong>{now}</strong></p>
                    <p>Test Service Subtractor Result ({randomX} - {randomY}): <strong>{result}</strong></p>
                </body>
                </html>";

            return new ContentResult
            {
                Content = html,
                ContentType = "text/html"
            };
        }

        [HttpGet("hi")]
        public async Task<ContentResult> GetHiPage()
        {
            using var activity = ActivitySource.StartActivity("GetHiPage");
            var act = Activity.Current;
            act?.SetTag("user.id", "userId");
            act?.SetTag("order.count", "orders.Count");
            act?.AddEvent(new ActivityEvent("cache.miss"));

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("endpoint", "/hi");

            PageHitCounter.Add(1, new KeyValuePair<string, object?>("metric_x", "GetHiPage"));
            activity?.SetTag("customTag", "customTagValue");            
            var stopwatch = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation("Serving time page");

                await _delayService.DelayFor(new Random().Next(1000, 2000));

                var html = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Hi Page</title>
                </head>
                <body>
                    <h1>Hi, this page x</h1>
                </body>
                </html>";
                return new ContentResult
                {
                    Content = html,
                    ContentType = "text/html",
                    StatusCode = 500
                };
            }
            finally
            {
                stopwatch.Stop();
                ResponseTimeHistogram.Record(stopwatch.ElapsedMilliseconds);
                ActiveRequestCounter.Add(-1);
            }
        }
    }
}