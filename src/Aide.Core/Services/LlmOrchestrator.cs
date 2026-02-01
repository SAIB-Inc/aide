using System.Text.Json;
using Aide.Core.Abstractions;

namespace Aide.Core.Services;

/// <summary>
/// Orchestrates LLM interactions with tool calling and capability execution.
/// Manages conversation history and multi-step tool execution flow.
/// </summary>
public class LlmOrchestrator
{
    private readonly ILlmProvider _llmProvider;
    private readonly CapabilityRegistry _registry;
    private readonly Lock _lock = new();

    // MVP: In-memory conversation history (lost on restart)
    // Future: Persist to database or distributed cache
    private readonly Dictionary<string, List<Message>> _conversationHistory = [];

    /// <summary>
    /// Initialize orchestrator with LLM provider and capability registry
    /// </summary>
    /// <param name="llmProvider">LLM provider for completions</param>
    /// <param name="registry">Registry of available capabilities</param>
    public LlmOrchestrator(ILlmProvider llmProvider, CapabilityRegistry registry)
    {
        ArgumentNullException.ThrowIfNull(llmProvider);
        ArgumentNullException.ThrowIfNull(registry);

        _llmProvider = llmProvider;
        _registry = registry;
    }

    /// <summary>
    /// Process user input and return LLM response after executing any necessary tools
    /// </summary>
    /// <param name="sessionId">Session ID for conversation isolation</param>
    /// <param name="userInput">User's message</param>
    /// <param name="systemPrompt">Optional system prompt for this request</param>
    /// <param name="maxIterations">Maximum tool calling iterations (default: 10)</param>
    /// <returns>Final LLM response text</returns>
    /// <exception cref="InvalidOperationException">Thrown when max iterations exceeded</exception>
    public async Task<string> ProcessUserInput(
        string sessionId,
        string userInput,
        string? systemPrompt = null,
        int maxIterations = 10)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);
        ArgumentException.ThrowIfNullOrWhiteSpace(userInput);

        if (maxIterations <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxIterations), "Must be greater than 0");
        }

        // Get or create conversation history for this session
        var history = GetOrCreateHistory(sessionId);

        // Add user message to history
        lock (_lock)
        {
            history.Add(new Message { Role = "user", Content = userInput });
        }

        // Get available tools from capabilities
        var tools = _registry.ToToolDefinitions();

        // Tool calling loop with iteration limit
        var iterations = 0;
        while (iterations < maxIterations)
        {
            iterations++;

            // Send request to LLM with current history and available tools
            var llmRequest = new LlmRequest
            {
                SessionId = sessionId,
                Messages = [.. history], // Create copy to avoid concurrent modification
                Tools = tools.Count > 0 ? tools : null,
                SystemPrompt = systemPrompt
            };

            var response = await _llmProvider.SendAsync(llmRequest);

            // No tools to call, return final response
            if (response.ToolCalls.Count == 0)
            {
                lock (_lock)
                {
                    history.Add(new Message { Role = "assistant", Content = response.Text });
                }
                return response.Text;
            }

            // LLM wants to call tools - add assistant message with tool calls
            lock (_lock)
            {
                history.Add(new Message
                {
                    Role = "assistant",
                    Content = response.Text,
                    ToolCalls = response.ToolCalls
                });
            }

            // Execute each tool call and add results to history
            foreach (var toolCall in response.ToolCalls)
            {
                var result = await ExecuteCapability(toolCall);

                lock (_lock)
                {
                    history.Add(new Message
                    {
                        Role = "tool",
                        ToolCallId = toolCall.Id,
                        Content = result
                    });
                }
            }
        }

        // Max iterations exceeded
        throw new InvalidOperationException(
            $"Maximum tool calling iterations ({maxIterations}) exceeded. " +
            "This may indicate an infinite loop or overly complex task.");
    }

    /// <summary>
    /// Execute a capability based on a tool call from the LLM
    /// </summary>
    /// <param name="toolCall">Tool call from LLM</param>
    /// <returns>Result string (success output or error message)</returns>
    private async Task<string> ExecuteCapability(ToolCall toolCall)
    {
        try
        {
            // Get capability from registry
            if (!_registry.TryGet(toolCall.Name, out var capability))
            {
                return $"Error: Tool '{toolCall.Name}' not found in capability registry.";
            }

            // Build capability context from tool call input
            var context = new CapabilityContext
            {
                Input = (toolCall.Input.GetValueOrDefault("input")?.ToString()
                    ?? toolCall.Input.GetValueOrDefault("action")?.ToString()) ?? "",
                Parameters = toolCall.Input
            };

            // Execute capability (capability is guaranteed non-null by TryGet check above)
            var result = await capability!.ExecuteAsync(context);

            // Return result or error
            if (!result.Success)
            {
                return $"Error: {result.ErrorMessage}";
            }

            // Return output or serialized data
            return result.Output ?? JsonSerializer.Serialize(result.Data);
        }
        catch (Exception ex)
        {
            return $"Exception executing tool '{toolCall.Name}': {ex.Message}";
        }
    }

    /// <summary>
    /// Get conversation history for a session, creating if it doesn't exist
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Conversation history list</returns>
    private List<Message> GetOrCreateHistory(string sessionId)
    {
        lock (_lock)
        {
            if (!_conversationHistory.TryGetValue(sessionId, out var history))
            {
                history = [];
                _conversationHistory[sessionId] = history;
            }
            return history;
        }
    }

    /// <summary>
    /// Clear conversation history for a session
    /// </summary>
    /// <param name="sessionId">Session ID to clear</param>
    public void ClearHistory(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        lock (_lock)
        {
            _conversationHistory.Remove(sessionId);
        }
    }

    /// <summary>
    /// Clear all conversation histories
    /// </summary>
    public void ClearAllHistories()
    {
        lock (_lock)
        {
            _conversationHistory.Clear();
        }
    }

    /// <summary>
    /// Get the number of active sessions
    /// </summary>
    public int ActiveSessionCount
    {
        get
        {
            lock (_lock)
            {
                return _conversationHistory.Count;
            }
        }
    }

    /// <summary>
    /// Get message count for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Number of messages in session, or 0 if session doesn't exist</returns>
    public int GetMessageCount(string sessionId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        lock (_lock)
        {
            return _conversationHistory.TryGetValue(sessionId, out var history)
                ? history.Count
                : 0;
        }
    }
}
