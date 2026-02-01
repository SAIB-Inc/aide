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

        // Configure HttpClient for API communication
        // TODO: Make API base URL configurable via settings
        builder.Services.AddHttpClient<AideApiClient>(client =>
        {
            // Default to localhost for development
            // In production, this should be configurable
            client.BaseAddress = new Uri("http://localhost:5009/");
            client.Timeout = TimeSpan.FromSeconds(120); // Long timeout for LLM responses
        });

        return builder.Build();
    }
}
