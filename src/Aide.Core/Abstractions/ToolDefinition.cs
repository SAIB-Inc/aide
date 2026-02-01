namespace Aide.Core.Abstractions;

/// <summary>
/// Definition of a tool/capability available to the LLM
/// </summary>
public class ToolDefinition
{
    /// <summary>
    /// Tool name (matches capability name)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Description of what the tool does
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// JSON schema for tool input parameters
    /// </summary>
    public required ToolSchema InputSchema { get; init; }
}
