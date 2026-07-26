using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OpenObserveDemo.Lib
{
    public class TestService
    {
        private readonly ILogger<TestService> _logger;
        private readonly DelayService _delayService;
        private static readonly ActivitySource ActivitySource = new("test-otlp-demo.TestService");
        private static readonly Meter Meter = new("test-otlp-demo.Metrics", "1.0.0");
        private static readonly Counter<int> PageHitCounter = Meter.CreateCounter<int>("function_hits");

        public TestService(ILogger<TestService> logger, DelayService delayService)
        {
            _logger = logger;
            _delayService = delayService;
        }

        public async Task<int> Subtractor(int a, int b)
        {
            using var activity = ActivitySource.StartActivity("Subtractor");
            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger.LogInformation("Subtractor method called with parameters: {a} and {b}", a, b);
            PageHitCounter.Add(1, new KeyValuePair<string, object?>("metric_x", "subtractor"));

            await _delayService.DelayFor(b*10);

            return a - b;
        }
    }
}