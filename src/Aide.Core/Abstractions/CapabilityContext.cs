namespace Aide.Core.Abstractions;

/// <summary>
/// Context provided to a capability during execution
/// </summary>
public class CapabilityContext
{
    /// <summary>
    /// Primary input string for the capability
    /// </summary>
    public required string Input { get; init; }

    /// <summary>
    /// Additional structured parameters from the LLM
    /// </summary>
    public required Dictionary<string, object> Parameters { get; init; }

    /// <summary>
    /// Cancellation token for async operations
    /// </summary>
    public CancellationToken CancellationToken { get; init; }
}
