using Aide.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aide.Api.Controllers;

/// <summary>
/// Chat endpoint for conversational AI interactions
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ChatController(LlmOrchestrator orchestrator, ILogger<ChatController> logger) : ControllerBase
{
    /// <summary>
    /// Send a message to the AI agent and get a response
    /// </summary>
    /// <param name="request">Chat request with user message and session ID</param>
    /// <returns>AI agent response</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ChatResponse>> SendMessage([FromBody] ChatRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Request",
                    Detail = "Message cannot be empty",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            logger.LogInformation(
                "Processing chat message for session {SessionId}: {Message}",
                request.SessionId,
                request.Message);

            var response = await orchestrator.ProcessUserInput(
                sessionId: request.SessionId ?? Guid.NewGuid().ToString(),
                userInput: request.Message,
                systemPrompt: request.SystemPrompt,
                maxIterations: request.MaxIterations ?? 10
            );

            logger.LogInformation(
                "Chat response for session {SessionId}: {ResponseLength} characters",
                request.SessionId,
                response.Length);

            return Ok(new ChatResponse
            {
                SessionId = request.SessionId ?? "",
                Message = response,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogWarning(ex, "Invalid operation in chat request");

            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Operation",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing chat message");

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred processing your request",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get message count for a session
    /// </summary>
    /// <param name="sessionId">Session ID</param>
    /// <returns>Number of messages in the session</returns>
    [HttpGet("sessions/{sessionId}/count")]
    [ProducesResponseType(typeof(SessionCountResponse), StatusCodes.Status200OK)]
    public ActionResult<SessionCountResponse> GetMessageCount(string sessionId)
    {
        var count = orchestrator.GetMessageCount(sessionId);

        return Ok(new SessionCountResponse
        {
            SessionId = sessionId,
            MessageCount = count
        });
    }

    /// <summary>
    /// Clear conversation history for a session
    /// </summary>
    /// <param name="sessionId">Session ID to clear</param>
    [HttpDelete("sessions/{sessionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult ClearSession(string sessionId)
    {
        orchestrator.ClearHistory(sessionId);
        logger.LogInformation("Cleared session {SessionId}", sessionId);

        return NoContent();
    }
}

/// <summary>
/// Request model for chat endpoint
/// </summary>
public record ChatRequest
{
    /// <summary>
    /// Session ID for conversation continuity (optional, will be generated if not provided)
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
/// Response model for session count endpoint
/// </summary>
public record SessionCountResponse
{
    /// <summary>
    /// Session ID
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Number of messages in the session
    /// </summary>
    public int MessageCount { get; init; }
}
