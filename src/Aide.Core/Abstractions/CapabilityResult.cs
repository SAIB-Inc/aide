namespace Aide.Core.Abstractions;

/// <summary>
/// Result returned from capability execution
/// </summary>
public class CapabilityResult
{
    /// <summary>
    /// Whether the capability executed successfully
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Human-readable output text (shown to LLM and user)
    /// </summary>
    public string? Output { get; init; }

    /// <summary>
    /// Structured data result (optional, for programmatic use)
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Machine-readable error code for programmatic error handling
    /// </summary>
    public string? ErrorCode { get; init; }
}
