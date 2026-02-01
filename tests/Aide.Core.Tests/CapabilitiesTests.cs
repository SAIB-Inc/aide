using Aide.Capabilities;
using Aide.Core.Abstractions;
using Aide.Core.Providers;
using Aide.Core.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;

namespace Aide.Core.Tests;

/// <summary>
/// Tests for built-in capabilities
/// </summary>
public class CapabilitiesTests
{
    private readonly string? _apiKey;

    public CapabilitiesTests()
    {
        var configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .AddUserSecrets<ClaudeProviderTests>()
            .Build();

        _apiKey = configuration["ANTHROPIC_API_KEY"] ?? Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
    }

    [Fact]
    public async Task HelloWorldCapability_WithoutName_ShouldReturnGenericGreeting()
    {
        // Arrange
        var capability = new HelloWorldCapability();
        var context = new CapabilityContext
        {
            Input = "",
            Parameters = new Dictionary<string, object>()
        };

        // Act
        var result = await capability.ExecuteAsync(context);

        // Assert
        result.Success.Should().BeTrue();
        result.Output.Should().NotBeNullOrEmpty();
        result.Output.Should().Contain("Hello");
        result.Output.Should().Contain("👋");
    }

    [Fact]
    public async Task HelloWorldCapability_WithName_ShouldReturnPersonalizedGreeting()
    {
        // Arrange
        var capability = new HelloWorldCapability();
        var context = new CapabilityContext
        {
            Input = "Alice",
            Parameters = new Dictionary<string, object>
            {
                ["name"] = "Alice"
            }
        };

        // Act
        var result = await capability.ExecuteAsync(context);

        // Assert
        result.Success.Should().BeTrue();
        result.Output.Should().NotBeNullOrEmpty();
        result.Output.Should().Contain("Hello, Alice");
        result.Output.Should().Contain("👋");
    }

    [Fact]
    public void HelloWorldCapability_Schema_ShouldHaveNameParameter()
    {
        // Arrange
        var capability = new HelloWorldCapability();

        // Act
        var schema = capability.GetInputSchema();

        // Assert
        schema.Type.Should().Be("object");
        schema.Properties.Should().ContainKey("name");
        schema.Properties["name"].Type.Should().Be("string");
        schema.Required.Should().BeEmpty();
    }

    [Fact]
    public async Task SystemInfoCapability_ShouldReturnSystemInformation()
    {
        // Arrange
        var capability = new SystemInfoCapability();
        var context = new CapabilityContext
        {
            Input = "",
            Parameters = new Dictionary<string, object>()
        };

        // Act
        var result = await capability.ExecuteAsync(context);

        // Assert
        result.Success.Should().BeTrue();
        result.Output.Should().NotBeNullOrEmpty();
        result.Output.Should().Contain("System Information");
        result.Output.Should().Contain("OS:");
        result.Output.Should().Contain("Machine Name:");
        result.Output.Should().Contain(".NET Runtime:");
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public void SystemInfoCapability_Schema_ShouldHaveNoRequiredParameters()
    {
        // Arrange
        var capability = new SystemInfoCapability();

        // Act
        var schema = capability.GetInputSchema();

        // Assert
        schema.Type.Should().Be("object");
        schema.Properties.Should().BeEmpty();
        schema.Required.Should().BeEmpty();
    }

    [Fact]
    public void SystemInfoCapability_Name_ShouldBeSystemInfo()
    {
        // Arrange
        var capability = new SystemInfoCapability();

        // Act & Assert
        capability.Name.Should().Be("system_info");
        capability.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void HelloWorldCapability_Name_ShouldBeHelloWorld()
    {
        // Arrange
        var capability = new HelloWorldCapability();

        // Act & Assert
        capability.Name.Should().Be("hello_world");
        capability.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Integration_HelloWorld_WithOrchestrator_ShouldWork()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);
        var registry = new CapabilityRegistry();
        registry.Register(new HelloWorldCapability());

        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var response = await orchestrator.ProcessUserInput(
            sessionId: "hello-test",
            userInput: "Use the hello_world tool to greet Alice");

        // Assert
        response.Should().NotBeNullOrEmpty();
        response.Should().Contain("Alice");
    }

    [Fact]
    public async Task Integration_SystemInfo_WithOrchestrator_ShouldWork()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);
        var registry = new CapabilityRegistry();
        registry.Register(new SystemInfoCapability());

        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var response = await orchestrator.ProcessUserInput(
            sessionId: "sysinfo-test",
            userInput: "Use the system_info tool to get my system information");

        // Assert
        response.Should().NotBeNullOrEmpty();
        // Should contain some system info from the capability
        response.ToLower().Should().ContainAny("os", "system", "machine", "runtime");
    }

    [Fact]
    public async Task Integration_MultipleCapabilities_WithOrchestrator_ShouldWork()
    {
        // Arrange
        SkipIfNoApiKey();
        var provider = new ClaudeProvider(_apiKey!);
        var registry = new CapabilityRegistry();
        registry.Register(new HelloWorldCapability());
        registry.Register(new SystemInfoCapability());

        var orchestrator = new LlmOrchestrator(provider, registry);

        // Act
        var response = await orchestrator.ProcessUserInput(
            sessionId: "multi-test",
            userInput: "First greet me as Bob, then tell me about my system");

        // Assert
        response.Should().NotBeNullOrEmpty();
        // Should use both capabilities
        response.Should().Contain("Bob");
    }

    private void SkipIfNoApiKey()
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new SkipException("ANTHROPIC_API_KEY environment variable not set.");
        }
    }

    private class SkipException(string message) : Exception(message);
}
