namespace Aide.Core.Abstractions;

/// <summary>
/// JSON schema definition for capability tool parameters
/// </summary>
public record ToolSchema(
    string Type,  // "object"
    Dictionary<string, PropertySchema> Properties,
    string[] Required
);
