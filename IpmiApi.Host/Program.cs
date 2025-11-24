using IpmiApi.Services.Interfaces;
using IpmiApi.Services.Services;
using IpmiApi.Services.Models;
using IpmiApi.Controllers;
using DotNetEnv;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<IpmiConfiguration>(options =>
{
    options.Host = Env.GetString("IPMI_HOST");
    options.Username = Env.GetString("IPMI_USERNAME", "ADMIN");
    options.Password = Env.GetString("IPMI_PASSWORD", "ADMIN");
    options.TimeoutSeconds = int.TryParse(Env.GetString("IPMI_TIMEOUT"), out var timeout) ? timeout : 30;
    options.UseHttps = !bool.TryParse(Env.GetString("IPMI_USE_HTTPS"), out var useHttps) || useHttps;
});

builder.Services.AddControllers()
    .AddApplicationPart(typeof(IpmiController).Assembly);

builder.Services.AddHttpClient<IIpmiService, SuperMicroIpmiService>();
builder.Services.AddScoped<IIpmiService, SuperMicroIpmiService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "IPMI API",
        Version = "v1",
        Description = "REST API for SuperMicro IPMI server management"
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    var controllersXmlFile = "IpmiApi.Controllers.xml";
    var controllersXmlPath = Path.Combine(AppContext.BaseDirectory, controllersXmlFile);
    if (File.Exists(controllersXmlPath))
    {
        c.IncludeXmlComments(controllersXmlPath);
    }
});

builder.Services.AddLogging(logging =>
{
    logging.AddConsole();
    logging.AddDebug();
    if (builder.Environment.IsDevelopment())
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "IPMI API v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

app.MapControllers();

app.MapHealthChecks("/health");

app.MapGet("/", () => new
{
    Service = "IPMI API",
    Version = "1.0.0",
    Description = "REST API for SuperMicro IPMI server management",
    Documentation = "/swagger",
    Health = "/health"
});

app.Run();
