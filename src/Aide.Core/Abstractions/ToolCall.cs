namespace Aide.Core.Abstractions;

/// <summary>
/// A tool call requested by the LLM
/// </summary>
public class ToolCall
{
    /// <summary>
    /// Unique ID for this tool call (for matching with results)
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Name of the tool to call
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Input parameters as key-value pairs
    /// </summary>
    public required Dictionary<string, object> Input { get; init; }
}
