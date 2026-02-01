using System.Net.Http.Json;
using Aide.Ui.Models;

namespace Aide.Ui.Services;

/// <summary>
/// HTTP client for communicating with Aide API
/// </summary>
public class AideApiClient(HttpClient httpClient)
{
    /// <summary>
    /// Send a chat message to the AI agent
    /// </summary>
    public async Task<ChatResponse> SendChatMessageAsync(
        string message,
        string? sessionId = null,
        string? systemPrompt = null,
        int? maxIterations = null,
        CancellationToken cancellationToken = default)
    {
        var request = new ChatRequest
        {
            SessionId = sessionId,
            Message = message,
            SystemPrompt = systemPrompt,
            MaxIterations = maxIterations
        };

        var response = await httpClient.PostAsJsonAsync(
            "api/chat",
            request,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<ChatResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Failed to deserialize chat response");
    }

    /// <summary>
    /// Get list of all available capabilities
    /// </summary>
    public async Task<CapabilitiesListResponse> GetCapabilitiesAsync(
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<CapabilitiesListResponse>(
            "api/capabilities",
            cancellationToken);

        return response ?? throw new InvalidOperationException("Failed to get capabilities");
    }

    /// <summary>
    /// Get details for a specific capability
    /// </summary>
    public async Task<CapabilityInfo> GetCapabilityAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetFromJsonAsync<CapabilityInfo>(
            $"api/capabilities/{name}",
            cancellationToken);

        return response ?? throw new InvalidOperationException($"Failed to get capability: {name}");
    }

    /// <summary>
    /// Clear conversation history for a session
    /// </summary>
    public async Task ClearSessionAsync(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.DeleteAsync(
            $"api/chat/sessions/{sessionId}",
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }
}
