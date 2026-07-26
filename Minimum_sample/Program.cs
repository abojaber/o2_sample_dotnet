using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Exporter;
using OpenObserveDemo.Lib;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TestService>();
builder.Services.AddSingleton<DelayService>();

var otlpBaseUrl = "http://localhost:5080/api/default";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("otlp-demo-other"))
    .WithTracing(t =>
    {
        t.AddSource("otlp-demo-other");
        t.AddSource("otlpDemo");
        t.AddSource("test-otlp-demo.TestService");
        t.AddSource("otlp-demo-other.DelayService");
        t.AddAspNetCoreInstrumentation();
        t.AddHttpClientInstrumentation();

        t.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri($"{otlpBaseUrl}/v1/traces");
            o.Protocol = OtlpExportProtocol.HttpProtobuf;
            o.Headers = "Authorization=Basic ZmFoZEBlbG0uc2E6bzJvaV9TWmFPSGZPUnZFVUNsTHZwTTVvbG5wRWdMQVhrWUx6MA==,stream-name=default";
        });
    })
    .WithMetrics(m =>
    {
        m.AddMeter("otlp-demo-other");
        m.AddMeter("otlpDemo");
        m.AddMeter("test-otlp-demo.Metrics");
        m.AddMeter("otlp-demo-other.DelayService");
        m.AddAspNetCoreInstrumentation();
        m.AddRuntimeInstrumentation();

        m.AddOtlpExporter(o =>
        {
            o.Endpoint = new Uri($"{otlpBaseUrl}/v1/metrics");
            o.Protocol = OtlpExportProtocol.HttpProtobuf;
            o.Headers = "Authorization=Basic ZmFoZEBlbG0uc2E6bzJvaV9TWmFPSGZPUnZFVUNsTHZwTTVvbG5wRWdMQVhrWUx6MA==,stream-name=default";
        });
    });

builder.Logging.ClearProviders();
builder.Logging.AddOpenTelemetry(o =>
{
    o.IncludeFormattedMessage = true;
    o.ParseStateValues = true;

    o.AddOtlpExporter(exporter =>
    {
        exporter.Endpoint = new Uri("http://localhost:5080/api/3Fkx3wKAnUqjIEr5RA5Ice7hzG3/default/_json");
        exporter.Protocol = OtlpExportProtocol.HttpProtobuf;
        exporter.Headers = "Authorization=Basic ZmFoZEBlbG0uc2E6bzJvaV9TWmFPSGZPUnZFVUNsTHZwTTVvbG5wRWdMQVhrWUx6MA==,stream-name=default";
    });
});
builder.Logging.AddConsole();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
