using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OpenObserveDemo.Controllers
{
    [ApiController]
    [Route("generate-traffic")]
    public class TestTrafficController : ControllerBase
    {
        private readonly ILogger<TestTrafficController> _logger;

        private static readonly ActivitySource ActivitySource = new("otlpDemo");
        private static readonly Meter Meter = new("otlpDemo");

        private static readonly Counter<int> RequestCounter =
            Meter.CreateCounter<int>("demo_requests_total", description: "Total simulated requests");
        private static readonly Histogram<double> LatencyHistogram =
            Meter.CreateHistogram<double>("demo_request_latency_ms", unit: "ms", description: "Simulated request latency");

        private static readonly string[] Endpoints = { "/orders", "/users", "/payments", "/inventory", "/search" };
        private static readonly string[] Methods = { "GET", "POST", "PUT", "DELETE" };

        public TestTrafficController(ILogger<TestTrafficController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Generate([FromQuery] int count = 20)
        {
            count = Math.Clamp(count, 1, 1000);
            var rng = Random.Shared;
            var errors = 0;

            for (var i = 0; i < count; i++)
            {
                var endpoint = Endpoints[rng.Next(Endpoints.Length)];
                var method = Methods[rng.Next(Methods.Length)];
                var isError = rng.NextDouble() < 0.15;
                var statusCode = isError ? (rng.NextDouble() < 0.5 ? 500 : 404) : 200;

                using var activity = ActivitySource.StartActivity("SimulatedRequest", ActivityKind.Server);
                activity?.SetTag("http.method", method);
                activity?.SetTag("http.route", endpoint);
                activity?.SetTag("http.status_code", statusCode);
                activity?.SetTag("demo.iteration", i);

                using (var child = ActivitySource.StartActivity("db.query", ActivityKind.Client))
                {
                    child?.SetTag("db.system", "postgresql");
                    child?.SetTag("db.statement", $"SELECT * FROM {endpoint.Trim('/')} LIMIT 50");
                    child?.SetStatus(ActivityStatusCode.Ok);
                    await Task.Delay(rng.Next(2, 15));
                }

                var latency = rng.Next(10, 450) + rng.NextDouble();
                var tags = new TagList
                {
                    { "endpoint", endpoint },
                    { "method", method },
                    { "status_code", statusCode }
                };
                RequestCounter.Add(1, tags);
                LatencyHistogram.Record(latency, tags);

                if (isError)
                {
                    errors++;
                    activity?.SetStatus(ActivityStatusCode.Error, $"HTTP {statusCode}");
                    _logger.LogError(
                        "Request failed: {Method} {Endpoint} returned {StatusCode} after {Latency:F1}ms",
                        method, endpoint, statusCode, latency);
                }
                else
                {
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    _logger.LogInformation(
                        "Request handled: {Method} {Endpoint} returned {StatusCode} in {Latency:F1}ms",
                        method, endpoint, statusCode, latency);
                }

                if (rng.NextDouble() < 0.3)
                {
                    _logger.LogWarning("Slow dependency detected on {Endpoint} (iteration {Iteration})", endpoint, i);
                }
            }

            _logger.LogInformation("Traffic burst complete: {Count} requests, {Errors} errors", count, errors);

            return Ok(new
            {
                generated = count,
                errors,
                streams = new { logs = "dotnetlogs", traces = "dotnettracing", metrics = "dotnetmetrics" },
                message = "Telemetry emitted. Check OpenObserve at http://localhost:5080 (org: default)."
            });
        }
    }
}
