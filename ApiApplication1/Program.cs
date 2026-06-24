using Serilog;
using SharedLib;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//AddOpenTelemetryToELKStack
builder.AddOpenTelemetryToELKStack("Api1",
    builder.Configuration["OpenObserve:OtlpEndpoint"]!,
    login: builder.Configuration["OpenObserve:Login"],
    key: builder.Configuration["OpenObserve:Key"],
    organization: builder.Configuration["OpenObserve:Organization"] ?? "default");

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

//app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
