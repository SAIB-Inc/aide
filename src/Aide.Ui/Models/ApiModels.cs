namespace Aide.Ui.Models;

/// <summary>
/// Request model for chat endpoint
/// </summary>
public record ChatRequest
{
    /// <summary>
    /// Session ID for conversation continuity
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// User's message to the AI agent
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Optional system prompt to guide AI behavior
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Maximum tool calling iterations (default: 10)
    /// </summary>
    public int? MaxIterations { get; init; }
}

/// <summary>
/// Response model for chat endpoint
/// </summary>
public record ChatResponse
{
    /// <summary>
    /// Session ID used for this conversation
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// AI agent's response message
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Timestamp of the response
    /// </summary>
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Capability information model
/// </summary>
public record CapabilityInfo
{
    /// <summary>
    /// Capability name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable description
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// JSON schema for capability input parameters
    /// </summary>
    public required object InputSchema { get; init; }
}

/// <summary>
/// Response model for capabilities list endpoint
/// </summary>
public record CapabilitiesListResponse
{
    /// <summary>
    /// List of available capabilities
    /// </summary>
    public required List<CapabilityInfo> Capabilities { get; init; }

    /// <summary>
    /// Total number of capabilities
    /// </summary>
    public int Count { get; init; }
}

/// <summary>
/// Message model for chat UI
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Message role (user or assistant)
    /// </summary>
    public string Role { get; set; } = "";

    /// <summary>
    /// Message content
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// Message timestamp
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
