namespace Aide.Core.Abstractions;

/// <summary>
/// Represents a capability/skill that can be executed by the AI agent.
/// Capabilities are tools that the LLM can call to perform actions.
/// </summary>
public interface ICapability
{
    /// <summary>
    /// Unique name of the capability (used for LLM tool identification)
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Human-readable description of what this capability does
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Returns the JSON schema for input parameters that the LLM will use
    /// when calling this capability as a tool
    /// </summary>
    ToolSchema GetInputSchema();

    /// <summary>
    /// Executes the capability with the provided context
    /// </summary>
    Task<CapabilityResult> ExecuteAsync(CapabilityContext context);
}
