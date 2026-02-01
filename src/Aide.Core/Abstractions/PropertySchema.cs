namespace Aide.Core.Abstractions;

/// <summary>
/// Schema definition for a single property in the tool input
/// </summary>
public record PropertySchema(
    string Type,  // "string", "number", "boolean", "array", "object"
    string Description,
    string[]? Enum = null,
    object? Default = null
);
