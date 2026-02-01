using Aide.Capabilities;
using Aide.Core.Providers;
using Aide.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add user-level configuration from ~/.aide/appsettings.json
var userConfigPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".aide",
    "appsettings.json"
);

if (File.Exists(userConfigPath))
{
    builder.Configuration.AddJsonFile(userConfigPath, optional: true, reloadOnChange: true);
}

// Environment variables with AIDE_ prefix override everything
builder.Configuration.AddEnvironmentVariables(prefix: "AIDE_");

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS for MAUI app and local development
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:*",
                "https://localhost:*",
                "app://localhost"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .SetIsOriginAllowedToAllowWildcardSubdomains();
    });
});

// Register Capability Registry as singleton
builder.Services.AddSingleton<CapabilityRegistry>(sp =>
{
    var registry = new CapabilityRegistry();

    // Register built-in capabilities
    registry.Register(new HelloWorldCapability());
    registry.Register(new SystemInfoCapability());

    return registry;
});

// Register LLM Provider
builder.Services.AddSingleton<ClaudeProvider>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var apiKey = config["Aide:Llm:Providers:Claude:ApiKey"]
        ?? config["ANTHROPIC_API_KEY"]
        ?? throw new InvalidOperationException(
            "Claude API key not found. Set it in ~/.aide/appsettings.json or AIDE_ANTHROPIC_API_KEY environment variable.");

    var model = config["Aide:Llm:Providers:Claude:Model"];

    return new ClaudeProvider(apiKey, model);
});

// Register LLM Orchestrator as scoped (per request)
builder.Services.AddScoped<LlmOrchestrator>(sp =>
{
    var provider = sp.GetRequiredService<ClaudeProvider>();
    var registry = sp.GetRequiredService<CapabilityRegistry>();

    return new LlmOrchestrator(provider, registry);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors();
app.UseAuthorization();
app.MapControllers();

// Log startup information
var logger = app.Services.GetRequiredService<ILogger<Program>>();
var registry = app.Services.GetRequiredService<CapabilityRegistry>();

logger.LogInformation("Aide API starting...");
logger.LogInformation("Registered capabilities: {Count}", registry.Count);

foreach (var capability in registry.GetAll())
{
    logger.LogInformation("  - {Name}: {Description}", capability.Name, capability.Description);
}

app.Run();
