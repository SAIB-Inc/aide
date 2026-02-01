namespace Aide.Core.Abstractions;

/// <summary>
/// Response from an LLM provider
/// </summary>
public class LlmResponse
{
    /// <summary>
    /// Text response from the LLM (if no tool calls)
    /// </summary>
    public required string Text { get; init; }

    /// <summary>
    /// Tool calls requested by the LLM
    /// </summary>
    public required List<ToolCall> ToolCalls { get; init; }

    /// <summary>
    /// Total tokens used in this request
    /// </summary>
    public int TokenCount { get; init; }

    /// <summary>
    /// Cost of this request (if available)
    /// </summary>
    public decimal? Cost { get; init; }

    /// <summary>
    /// Model used for this response
    /// </summary>
    public required string Model { get; init; }

    /// <summary>
    /// Stop reason (e.g., "end_turn", "tool_use", "max_tokens")
    /// </summary>
    public string? StopReason { get; init; }
}
