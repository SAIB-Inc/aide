using Aide.Core.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace Aide.Ui.Components.Pages;

public partial class Capabilities
{
    [Inject]
    public required CapabilityRegistry Registry { get; set; }

    [Inject]
    public required ILogger<Capabilities> Logger { get; set; }

    private List<CapabilityDisplay> _capabilities = [];
    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        LoadCapabilities();
    }

    private void LoadCapabilities()
    {
        _isLoading = true;

        try
        {
            Logger.LogInformation("Loading capabilities from registry");

            _capabilities = Registry.GetAll()
                .Select(c => new CapabilityDisplay
                {
                    Name = c.Name,
                    Description = c.Description
                })
                .ToList();

            Logger.LogInformation("Loaded {Count} capabilities", _capabilities.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading capabilities");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private record CapabilityDisplay
    {
        public required string Name { get; init; }
        public required string Description { get; init; }
    }
}
