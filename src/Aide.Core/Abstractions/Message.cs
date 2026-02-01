namespace Aide.Core.Abstractions;

/// <summary>
/// A message in the conversation history
/// </summary>
public class Message
{
    /// <summary>
    /// Role: "user", "assistant", "tool", "system"
    /// </summary>
    public required string Role { get; init; }

    /// <summary>
    /// Text content of the message
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Tool calls made by the assistant
    /// </summary>
    public List<ToolCall>? ToolCalls { get; init; }

    /// <summary>
    /// Tool call ID (for tool response messages)
    /// </summary>
    public string? ToolCallId { get; init; }
}
