namespace Aide.Core.Abstractions;

/// <summary>
/// Request sent to an LLM provider
/// </summary>
public class LlmRequest
{
    /// <summary>
    /// Session ID for conversation tracking
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Conversation history (user, assistant, tool messages)
    /// </summary>
    public required List<Message> Messages { get; init; }

    /// <summary>
    /// Available tools that the LLM can call
    /// </summary>
    public List<ToolDefinition>? Tools { get; init; }

    /// <summary>
    /// System prompt (optional, provider-specific)
    /// </summary>
    public string? SystemPrompt { get; init; }

    /// <summary>
    /// Temperature for response generation (0.0 - 2.0)
    /// </summary>
    public double Temperature { get; init; } = 1.0;

    /// <summary>
    /// Maximum tokens to generate
    /// </summary>
    public int MaxTokens { get; init; } = 4096;
}
