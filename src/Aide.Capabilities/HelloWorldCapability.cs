using Aide.Core.Abstractions;

namespace Aide.Capabilities;

/// <summary>
/// Simple greeting capability for testing and demonstration.
/// Supports basic greetings and personalized messages.
/// </summary>
public class HelloWorldCapability : ICapability
{
    public string Name => "hello_world";

    public string Description => "Send a friendly greeting. Can optionally include a name for personalized greetings.";

    public Task<CapabilityResult> ExecuteAsync(CapabilityContext context)
    {
        try
        {
            // Get optional name parameter
            var name = context.Parameters.TryGetValue("name", out var nameValue)
                ? nameValue?.ToString()
                : null;

            // Generate greeting message
            var greeting = string.IsNullOrWhiteSpace(name)
                ? "Hello! 👋 How can I help you today?"
                : $"Hello, {name}! 👋 Nice to meet you!";

            return Task.FromResult(new CapabilityResult
            {
                Success = true,
                Output = greeting
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new CapabilityResult
            {
                Success = false,
                ErrorMessage = $"Failed to generate greeting: {ex.Message}",
                ErrorCode = "HELLO_ERROR"
            });
        }
    }

    public ToolSchema GetInputSchema()
    {
        return new ToolSchema(
            Type: "object",
            Properties: new Dictionary<string, PropertySchema>
            {
                ["name"] = new PropertySchema(
                    Type: "string",
                    Description: "Optional name to include in the greeting"
                )
            },
            Required: []
        );
    }
}
