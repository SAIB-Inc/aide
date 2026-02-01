using Aide.Core.Abstractions;
using Aide.Core.Providers;
using Aide.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Aide.Core.Tests;

/// <summary>
/// Tests for LlmOrchestrator orchestration logic
/// </summary>
public class LlmOrchestratorTests
{
    private readonly string? _apiKey;

    public LlmOrchestratorTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<ClaudeProviderTests>()
            .Build();

        _apiKey = configuration["ANTHROPIC_API_KEY"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
    }

    [Fact]
    public void Constructor_WithValidDependencies_ShouldInitialize()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();

        // Act
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Assert
        orchestrator.Should().NotBeNull();
        orchestrator.ActiveSessionCount.Should().Be(0);
    }

    [Fact]
    public void Constructor_WithNullProvider_ShouldThrow()
    {
        // Arrange
        var registry = new CapabilityRegistry();

        // Act
        var act = () => new LlmOrchestrator(null!, registry);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_WithNullRegistry_ShouldThrow()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");

        // Act
        var act = () => new LlmOrchestrator(provider, null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ProcessUserInput_WithNullSessionId_ShouldThrow()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var act = async () => await orchestrator.ProcessUserInput(null!, "test");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ProcessUserInput_WithEmptyUserInput_ShouldThrow()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var act = async () => await orchestrator.ProcessUserInput("session", "");

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ProcessUserInput_WithInvalidMaxIterations_ShouldThrow()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var act = async () => await orchestrator.ProcessUserInput("session", "test", null, 0);

        // Assert
        await act.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ClearHistory_WithNullSessionId_ShouldThrow()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var act = () => orchestrator.ClearHistory(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetMessageCount_WithNullSessionId_ShouldThrow()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var act = () => orchestrator.GetMessageCount(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void GetMessageCount_ForNonExistentSession_ShouldReturnZero()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var count = orchestrator.GetMessageCount("non-existent");

        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void ClearAllHistories_ShouldClearAllSessions()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        orchestrator.ClearAllHistories();

        // Assert
        orchestrator.ActiveSessionCount.Should().Be(0);
    }

    [Fact]
    public async Task ProcessUserInput_SimpleMessage_ShouldReturnResponse()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var response = await orchestrator.ProcessUserInput(
            sessionId: "test-session",
            userInput: "Say 'Hello, World!' and nothing else.");

        // Assert
        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("Hello");
        orchestrator.ActiveSessionCount.Should().Be(1);
        orchestrator.GetMessageCount("test-session").Should().Be(2); // user + assistant
    }

    [Fact]
    public async Task ProcessUserInput_MultiTurn_ShouldMaintainContext()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);
        var sessionId = "multi-turn-session";

        // Act
        var response1 = await orchestrator.ProcessUserInput(sessionId, "My favorite color is blue.");
        var response2 = await orchestrator.ProcessUserInput(sessionId, "What's my favorite color?");

        // Assert
        response1.Should().NotBeNullOrEmpty();
        response2.Should().Contain("blue");
        orchestrator.GetMessageCount(sessionId).Should().Be(4); // 2 user + 2 assistant
    }

    [Fact]
    public async Task ProcessUserInput_WithCapability_ShouldExecuteCapability()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);
        var registry = new CapabilityRegistry();

        // Register a simple calculator capability
        registry.Register(new CalculatorCapability());

        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var response = await orchestrator.ProcessUserInput(
            sessionId: "calc-session",
            userInput: "What is 15 + 27? Use the calculator tool.");

        // Assert
        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("42");
    }

    [Fact]
    public void ClearHistory_ShouldRemoveSession()
    {
        // Arrange
        var provider = new ClaudeProvider("test-key");
        var registry = new CapabilityRegistry();
        var orchestrator = new LlmOrchestrator(provider, registry);
        var sessionId = "test-session";

        // Create a session (simulate by checking message count)
        orchestrator.GetMessageCount(sessionId); // This doesn't create history, but ClearHistory should handle non-existent

        // Act
        orchestrator.ClearHistory(sessionId);

        // Assert
        orchestrator.ActiveSessionCount.Should().Be(0);
        orchestrator.GetMessageCount(sessionId).Should().Be(0);
    }

    private void SkipIfNoApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new SkipException("ANTHROPIC_API_KEY environment variable not set.");
        }
    }

    private class SkipException(string message) : Exception(message);

    /// <summary>
    /// Simple calculator capability for testing
    /// </summary>
    private class CalculatorCapability : ICapability
    {
        public string Name => "calculator";

        public string Description => "Perform basic arithmetic operations (add, subtract, multiply, divide)";

        public Task<CapabilityResult> ExecuteAsync(CapabilityContext context)
        {
            try
            {
                var operation = context.Parameters.GetValueOrDefault("operation")?.ToString() ?? "add";
                var a = Convert.ToDouble(context.Parameters.GetValueOrDefault("a"));
                var b = Convert.ToDouble(context.Parameters.GetValueOrDefault("b"));

                var result = operation.ToLower() switch
                {
                    "add" => a + b,
                    "subtract" => a - b,
                    "multiply" => a * b,
                    "divide" => b != 0 ? a / b : throw new DivideByZeroException(),
                    _ => throw new ArgumentException($"Unknown operation: {operation}")
                };

                return Task.FromResult(new CapabilityResult
                {
                    Success = true,
                    Output = result.ToString()
                });
            }
            catch (Exception ex)
            {
                return Task.FromResult(new CapabilityResult
                {
                    Success = false,
                    ErrorMessage = $"Calculator error: {ex.Message}"
                });
            }
        }

        public ToolSchema GetInputSchema()
        {
            return new ToolSchema(
                Type: "object",
                Properties: new Dictionary<string, PropertySchema>
                {
                    ["operation"] = new PropertySchema(
                        Type: "string",
                        Description: "The operation to perform",
                        Enum: ["add", "subtract", "multiply", "divide"]
                    ),
                    ["a"] = new PropertySchema(
                        Type: "number",
                        Description: "First number"
                    ),
                    ["b"] = new PropertySchema(
                        Type: "number",
                        Description: "Second number"
                    )
                },
                Required: ["operation", "a", "b"]
            );
        }
    }
}
