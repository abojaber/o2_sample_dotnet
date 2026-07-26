using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OpenObserveDemo.Lib
{
    public class DelayService
    {
        private readonly ILogger<DelayService> _logger;
        private static readonly ActivitySource ActivitySource = new("otlp-demo-other.DelayService");
        private static readonly Meter Meter = new("otlp-demo-other.DelayService", "1.0.0");
        private static readonly Counter<int> DelayCounter = Meter.CreateCounter<int>(
            "delay_invocations_total",
            description: "Total number of delay invocations");
        private static readonly Histogram<double> DelayHistogram = Meter.CreateHistogram<double>(
            "delay_duration_ms",
            unit: "ms",
            description: "Histogram of delay durations");

        public DelayService(ILogger<DelayService> logger)
        {
            _logger = logger;
        }

        public async Task DelayFor(int milliseconds, CancellationToken ct = default)
        {
            using var activity = ActivitySource.StartActivity("DelayFor");
            activity?.SetTag("delay_ms", milliseconds);

            _logger.LogInformation("Delay started for {DelayMs}ms", milliseconds);

            var stopwatch = Stopwatch.StartNew();
            await Task.Delay(milliseconds, ct);
            stopwatch.Stop();

            var elapsed = stopwatch.Elapsed.TotalMilliseconds;

            DelayCounter.Add(1, new KeyValuePair<string, object?>("delay_ms", milliseconds));
            DelayHistogram.Record(elapsed, new KeyValuePair<string, object?>("delay_ms", milliseconds));

            activity?.SetStatus(ActivityStatusCode.Ok);
            _logger.LogInformation("Delay completed in {Elapsed:F1}ms (requested: {DelayMs}ms)", elapsed, milliseconds);
        }
    }
}
