using Aide.Core.Abstractions;
using Aide.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace Aide.Api.Controllers;

/// <summary>
/// Capabilities management and execution endpoint
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CapabilitiesController(
    CapabilityRegistry registry,
    ILogger<CapabilitiesController> logger) : ControllerBase
{
    /// <summary>
    /// List all registered capabilities
    /// </summary>
    /// <returns>List of capabilities with their metadata</returns>
    [HttpGet]
    [ProducesResponseType(typeof(CapabilitiesListResponse), StatusCodes.Status200OK)]
    public ActionResult<CapabilitiesListResponse> ListCapabilities()
    {
        var capabilities = registry.GetAll()
            .Select(cap => new CapabilityInfo
            {
                Name = cap.Name,
                Description = cap.Description,
                InputSchema = cap.GetInputSchema()
            })
            .ToList();

        return Ok(new CapabilitiesListResponse
        {
            Capabilities = capabilities,
            Count = capabilities.Count
        });
    }

    /// <summary>
    /// Get details for a specific capability
    /// </summary>
    /// <param name="name">Capability name</param>
    /// <returns>Capability details including input schema</returns>
    [HttpGet("{name}")]
    [ProducesResponseType(typeof(CapabilityInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public ActionResult<CapabilityInfo> GetCapability(string name)
    {
        if (!registry.TryGet(name, out var capability))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Capability Not Found",
                Detail = $"No capability named '{name}' is registered",
                Status = StatusCodes.Status404NotFound
            });
        }

        return Ok(new CapabilityInfo
        {
            Name = capability!.Name,
            Description = capability.Description,
            InputSchema = capability.GetInputSchema()
        });
    }

    /// <summary>
    /// Execute a capability directly (bypassing LLM orchestrator)
    /// </summary>
    /// <param name="name">Capability name</param>
    /// <param name="request">Execution request with input and parameters</param>
    /// <returns>Capability execution result</returns>
    [HttpPost("{name}/execute")]
    [ProducesResponseType(typeof(CapabilityExecutionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CapabilityExecutionResponse>> ExecuteCapability(
        string name,
        [FromBody] CapabilityExecutionRequest request)
    {
        if (!registry.TryGet(name, out var capability))
        {
            return NotFound(new ProblemDetails
            {
                Title = "Capability Not Found",
                Detail = $"No capability named '{name}' is registered",
                Status = StatusCodes.Status404NotFound
            });
        }

        try
        {
            logger.LogInformation("Executing capability {Name} with input: {Input}", name, request.Input);

            var context = new CapabilityContext
            {
                Input = request.Input ?? "",
                Parameters = request.Parameters ?? new Dictionary<string, object>()
            };

            var result = await capability!.ExecuteAsync(context);

            logger.LogInformation(
                "Capability {Name} execution completed. Success: {Success}",
                name,
                result.Success);

            return Ok(new CapabilityExecutionResponse
            {
                Success = result.Success,
                Output = result.Output,
                Data = result.Data,
                ErrorMessage = result.ErrorMessage,
                ErrorCode = result.ErrorCode
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing capability {Name}", name);

            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Execution Error",
                Detail = $"An error occurred executing capability '{name}': {ex.Message}",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
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
/// Capability information model
/// </summary>
public record CapabilityInfo
{
    /// <summary>
    /// Capability name (used to invoke)
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// Human-readable description of what the capability does
    /// </summary>
    public required string Description { get; init; }

    /// <summary>
    /// JSON schema for capability input parameters
    /// </summary>
    public required ToolSchema InputSchema { get; init; }
}

/// <summary>
/// Request model for capability execution
/// </summary>
public record CapabilityExecutionRequest
{
    /// <summary>
    /// Primary input string
    /// </summary>
    public string? Input { get; init; }

    /// <summary>
    /// Additional parameters as key-value pairs
    /// </summary>
    public Dictionary<string, object>? Parameters { get; init; }
}

/// <summary>
/// Response model for capability execution
/// </summary>
public record CapabilityExecutionResponse
{
    /// <summary>
    /// Whether execution was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Human-readable output
    /// </summary>
    public string? Output { get; init; }

    /// <summary>
    /// Structured data result
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// Error message if execution failed
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Error code if execution failed
    /// </summary>
    public string? ErrorCode { get; init; }
}
