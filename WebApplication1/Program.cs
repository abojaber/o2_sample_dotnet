using Serilog;
using SharedLib;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});
// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

//AddOpenTelemetryToELKStack
builder.AddOpenTelemetryToELKStack("WebApplication",
    builder.Configuration["OpenObserve:OtlpEndpoint"]!,
    login: builder.Configuration["OpenObserve:Login"],
    key: builder.Configuration["OpenObserve:Key"],
    organization: builder.Configuration["OpenObserve:Organization"] ?? "default");
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.Use(async (context, next) =>
{
    await next();

    if (context.Response.StatusCode >= 400)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(
            "HTTP {StatusCode} {Method} {Path}",
            context.Response.StatusCode,
            context.Request.Method,
            context.Request.Path);
    }
});

app.UseAuthorization();

app.MapRazorPages();

app.Run();
