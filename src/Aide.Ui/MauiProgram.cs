using Aide.Capabilities;
using Aide.Core.Providers;
using Aide.Core.Services;
using Aide.Ui.Services;
using Microsoft.Extensions.Logging;
using MudBlazor.Services;

namespace Aide.Ui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Add MudBlazor services
        builder.Services.AddMudServices();

        // Add app state management
        builder.Services.AddSingleton<AppStateService>();

        // Register Capability Registry with built-in capabilities
        builder.Services.AddSingleton<CapabilityRegistry>(sp =>
        {
            var registry = new CapabilityRegistry();
            registry.Register(new HelloWorldCapability());
            registry.Register(new SystemInfoCapability());
            return registry;
        });

        // Register LLM Provider (handles its own configuration)
        builder.Services.AddSingleton<ClaudeProvider>();

        // Register LLM Orchestrator as singleton to maintain conversation history
        builder.Services.AddSingleton<LlmOrchestrator>(sp =>
            new LlmOrchestrator(
                sp.GetRequiredService<ClaudeProvider>(),
                sp.GetRequiredService<CapabilityRegistry>()));

        return builder.Build();
    }
}
