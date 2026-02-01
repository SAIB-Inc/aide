namespace Aide.Core.Abstractions;

/// <summary>
/// Result of a tool execution (returned to LLM)
/// </summary>
public class ToolResult
{
    /// <summary>
    /// Tool call ID this result corresponds to
    /// </summary>
    public required string ToolCallId { get; init; }

    /// <summary>
    /// Result content (success or error message)
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Whether the tool execution was successful
    /// </summary>
    public bool IsError { get; init; }
}
