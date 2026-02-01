using Aide.Core.Abstractions;
using Aide.Core.Providers;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Aide.Core.Tests;

/// <summary>
/// Integration tests for ClaudeProvider.
/// Requires ANTHROPIC_API_KEY environment variable to be set.
/// </summary>
public class ClaudeProviderTests
{
    private readonly string? _apiKey;

    public ClaudeProviderTests()
    {
        // Load API key from environment variable or user secrets
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<ClaudeProviderTests>()
            .Build();

        _apiKey = configuration["ANTHROPIC_API_KEY"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
    }

    [Fact]
    public void Constructor_WithValidApiKey_ShouldInitialize()
    {
        // Arrange & Act
        var provider = new ClaudeProvider("test-api-key");

        // Assert
        provider.Name.Should().Be("Claude");
    }

    [Fact]
    public void Constructor_WithNullApiKey_ShouldThrow()
    {
        // Act
        var act = () => new ClaudeProvider(null!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithEmptyApiKey_ShouldThrow()
    {
        // Act
        var act = () => new ClaudeProvider("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public async Task SendAsync_SimpleMessage_ShouldReturnResponse()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);

        var request = new LlmRequest
        {
            SessionId = "test-session",
            Messages =
            [
                new Message
                {
                    Role = "user",
                    Content = "Say 'Hello, World!' and nothing else."
                }
            ],
            MaxTokens = 100
        };

        // Act
        var response = await provider.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().NotBeNullOrEmpty();
        response.Text.Should().Contain("Hello");
        response.Model.Should().NotBeNullOrEmpty();
        response.TokenCount.Should().BeGreaterThan(0);
        response.ToolCalls.Should().BeEmpty();
    }

    [Fact(Skip = "Integration test - requires API key")]
    public async Task SendAsync_WithSystemPrompt_ShouldRespectSystemMessage()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);

        var request = new LlmRequest
        {
            SessionId = "test-session",
            SystemPrompt = "You are a helpful assistant that only responds in JSON format.",
            Messages =
            [
                new Message
                {
                    Role = "user",
                    Content = "What is 2+2?"
                }
            ],
            MaxTokens = 200
        };

        // Act
        var response = await provider.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().NotBeNullOrEmpty();
        response.TokenCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task SendAsync_WithTools_ShouldCallTool()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);

        var request = new LlmRequest
        {
            SessionId = "test-session",
            Messages =
            [
                new Message
                {
                    Role = "user",
                    Content = "What's the weather in San Francisco?"
                }
            ],
            Tools =
            [
                new ToolDefinition
                {
                    Name = "get_weather",
                    Description = "Get the current weather for a location",
                    InputSchema = new ToolSchema(
                        Type: "object",
                        Properties: new Dictionary<string, PropertySchema>
                        {
                            ["location"] = new PropertySchema(
                                Type: "string",
                                Description: "The city and state, e.g. San Francisco, CA"
                            ),
                            ["unit"] = new PropertySchema(
                                Type: "string",
                                Description: "Temperature unit (celsius or fahrenheit)",
                                Enum: ["celsius", "fahrenheit"]
                            )
                        },
                        Required: ["location"]
                    )
                }
            ],
            MaxTokens = 500
        };

        // Act
        var response = await provider.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.ToolCalls.Should().NotBeEmpty();
        response.ToolCalls[0].Name.Should().Be("get_weather");
        response.ToolCalls[0].Input.Should().ContainKey("location");
        response.ToolCalls[0].Input["location"].ToString().Should().Contain("San Francisco");
        response.TokenCount.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Integration test - requires API key")]
    public async Task SendAsync_MultiTurnConversation_ShouldMaintainContext()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);

        var messages = new List<Message>
        {
            new() { Role = "user", Content = "My name is Alice." },
            new() { Role = "assistant", Content = "Nice to meet you, Alice! How can I help you today?" },
            new() { Role = "user", Content = "What's my name?" }
        };

        var request = new LlmRequest
        {
            SessionId = "test-session",
            Messages = messages,
            MaxTokens = 100
        };

        // Act
        var response = await provider.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().Contain("Alice");
        response.TokenCount.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Integration test - requires API key")]
    public async Task SendAsync_WithToolResult_ShouldProcessResult()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);

        // Simulate a conversation with tool use and result
        var messages = new List<Message>
        {
            new() { Role = "user", Content = "What's the weather in San Francisco?" },
            new()
            {
                Role = "assistant",
                ToolCalls =
                [
                    new ToolCall
                    {
                        Id = "tool_1",
                        Name = "get_weather",
                        Input = new Dictionary<string, object>
                        {
                            ["location"] = "San Francisco, CA",
                            ["unit"] = "fahrenheit"
                        }
                    }
                ]
            },
            new()
            {
                Role = "tool",
                ToolCallId = "tool_1",
                Content = "{\"temperature\": 65, \"condition\": \"sunny\", \"humidity\": 60}"
            }
        };

        var request = new LlmRequest
        {
            SessionId = "test-session",
            Messages = messages,
            MaxTokens = 200
        };

        // Act
        var response = await provider.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Text.Should().NotBeNullOrEmpty();
        response.Text.Should().Contain("65");
        response.Text.Should().Contain("sunny");
        response.ToolCalls.Should().BeEmpty(); // Should not request more tool calls
        response.TokenCount.Should().BeGreaterThan(0);
    }

    [Fact(Skip = "Integration test - requires API key")]
    public async Task SendAsync_WithCustomModel_ShouldUseSpecifiedModel()
    {
        // Arrange
        SkipIfNoApiKey();
        var customModel = "claude-3-5-haiku-20241022";
        var provider = new ClaudeProvider(_apiKey!, customModel);

        var request = new LlmRequest
        {
            SessionId = "test-session",
            Messages =
            [
                new Message
                {
                    Role = "user",
                    Content = "Say hello"
                }
            ],
            MaxTokens = 50
        };

        // Act
        var response = await provider.SendAsync(request);

        // Assert
        response.Should().NotBeNull();
        response.Model.Should().Be(customModel);
        response.Text.Should().NotBeNullOrEmpty();
    }

    private void SkipIfNoApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new SkipException("ANTHROPIC_API_KEY environment variable not set. Set it to run integration tests.");
        }
    }

    private class SkipException(string message) : Exception(message);
}
