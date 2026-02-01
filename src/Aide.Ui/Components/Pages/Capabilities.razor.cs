using Aide.Ui.Models;
using Aide.Ui.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Aide.Ui.Components.Pages;

public partial class Capabilities
{
    [Inject]
    public required AideApiClient ApiClient { get; set; }

    [Inject]
    public required ILogger<Capabilities> Logger { get; set; }

    [Inject]
    public required ISnackbar Snackbar { get; set; }

    private List<CapabilityInfo> _capabilities = [];
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        await LoadCapabilities();
    }

    private async Task LoadCapabilities()
    {
        _isLoading = true;

        try
        {
            Logger.LogInformation("Loading capabilities from API");

            var response = await ApiClient.GetCapabilitiesAsync();
            _capabilities = response.Capabilities;

            Logger.LogInformation("Loaded {Count} capabilities", _capabilities.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading capabilities");
            Snackbar.Add($"Failed to load capabilities: {ex.Message}", Severity.Error);
        }
        finally
        {
            _isLoading = false;
        }
    }
}
