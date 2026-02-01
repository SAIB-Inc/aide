namespace Aide.Core.Abstractions;

/// <summary>
/// Multi-provider abstraction for LLM interactions.
/// Supports Claude, OpenAI, Gemini, local models, and other providers.
/// </summary>
public interface ILlmProvider
{
    /// <summary>
    /// Provider name (e.g., "Claude", "OpenAI", "Gemini")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Send a request to the LLM and get a response
    /// </summary>
    Task<LlmResponse> SendAsync(LlmRequest request);
}
