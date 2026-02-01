using System.Text.Json;
using Anthropic.SDK;
using Anthropic.SDK.Common;
using Anthropic.SDK.Constants;
using AnthropicSdk = Anthropic.SDK.Messaging;
using Aide.Core.Abstractions;

namespace Aide.Core.Providers;

/// <summary>
/// Claude (Anthropic) implementation of ILlmProvider.
/// Supports tool calling, conversation history, and streaming.
/// </summary>
public class ClaudeProvider : ILlmProvider
{
    private const string ConfigFileName = "appsettings.json";
    private const string ConfigDirectory = ".aide";

    private readonly AnthropicClient _client;
    private readonly string _model;

    public string Name => "Claude";

    /// <summary>
    /// Initialize Claude provider with automatic configuration loading.
    /// Checks ANTHROPIC_API_KEY env var, then ~/.aide/appsettings.json
    /// </summary>
    public ClaudeProvider() : this(LoadApiKey(), LoadModel())
    {
    }

    /// <summary>
    /// Initialize Claude provider with explicit API key and model
    /// </summary>
    /// <param name="apiKey">Anthropic API key</param>
    /// <param name="model">Model to use (default: claude-sonnet-4-5-20250929)</param>
    public ClaudeProvider(string apiKey, string? model = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);

        _client = new AnthropicClient(new APIAuthentication(apiKey));
        _model = model ?? AnthropicModels.Claude45Sonnet;
    }

    /// <summary>
    /// Load API key from environment variable or config file
    /// </summary>
    private static string LoadApiKey()
    {
        // Try environment variable first
        var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
        if (!string.IsNullOrEmpty(apiKey))
            return apiKey;

        // Try user config file
        var config = LoadConfigFile();
        if (config != null &&
            config.RootElement.TryGetProperty("Aide", out var aide) &&
            aide.TryGetProperty("Llm", out var llm) &&
            llm.TryGetProperty("Providers", out var providers) &&
            providers.TryGetProperty("Claude", out var claude) &&
            claude.TryGetProperty("ApiKey", out var key))
        {
            var keyValue = key.GetString();
            if (!string.IsNullOrEmpty(keyValue))
                return keyValue;
        }

        throw new InvalidOperationException(
            $"Claude API key not found. Set ANTHROPIC_API_KEY environment variable or add it to ~/{ConfigDirectory}/{ConfigFileName}");
    }

    /// <summary>
    /// Load model from config file (optional)
    /// </summary>
    private static string? LoadModel()
    {
        var config = LoadConfigFile();
        if (config != null &&
            config.RootElement.TryGetProperty("Aide", out var aide) &&
            aide.TryGetProperty("Llm", out var llm) &&
            llm.TryGetProperty("Providers", out var providers) &&
            providers.TryGetProperty("Claude", out var claude) &&
            claude.TryGetProperty("Model", out var model))
        {
            return model.GetString();
        }

        return null;
    }

    /// <summary>
    /// Load and parse the config file
    /// </summary>
    private static JsonDocument? LoadConfigFile()
    {
        var configPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            ConfigDirectory,
            ConfigFileName
        );

        if (!File.Exists(configPath))
            return null;

        try
        {
            var json = File.ReadAllText(configPath);
            return JsonDocument.Parse(json);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Send a request to Claude and get a response
    /// </summary>
    public async Task<LlmResponse> SendAsync(LlmRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        // Convert our messages to Claude format
        var claudeMessages = ConvertMessages(request.Messages);

        // Convert our tools to Claude format
        var claudeTools = request.Tools?.Select(ConvertToolDefinition).ToList();

        // Build the request
        var parameters = new AnthropicSdk.MessageParameters
        {
            Messages = claudeMessages,
            MaxTokens = request.MaxTokens,
            Model = _model,
            Temperature = (decimal)request.Temperature,
            Tools = claudeTools
        };

        // Add system prompt if provided
        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            parameters.System = [new AnthropicSdk.SystemMessage(request.SystemPrompt)];
        }

        // Send request to Claude
        var response = await _client.Messages.GetClaudeMessageAsync(parameters);

        // Convert response to our format
        return ConvertResponse(response);
    }

    /// <summary>
    /// Convert our Message format to Claude's Message format
    /// </summary>
    private static List<AnthropicSdk.Message> ConvertMessages(List<Message> messages)
    {
        var result = new List<AnthropicSdk.Message>();

        foreach (var message in messages)
        {
            // Skip system messages (handled separately via SystemPrompt)
            if (message.Role == "system")
                continue;

            // Map role
            var role = message.Role switch
            {
                "user" => AnthropicSdk.RoleType.User,
                "assistant" => AnthropicSdk.RoleType.Assistant,
                _ => AnthropicSdk.RoleType.User // tool results are sent as user messages in Claude
            };

            // Handle different message types
            if (message.Role == "tool")
            {
                // Tool result message
                result.Add(new AnthropicSdk.Message
                {
                    Role = AnthropicSdk.RoleType.User,
                    Content =
                    [
                        new AnthropicSdk.ToolResultContent
                        {
                            ToolUseId = message.ToolCallId,
                            Content = [new AnthropicSdk.TextContent { Text = message.Content ?? "" }]
                        }
                    ]
                });
            }
            else if (message.ToolCalls?.Count > 0)
            {
                // Assistant message with tool calls
                var contentBlocks = new List<AnthropicSdk.ContentBase>();

                // Add text content if present
                if (!string.IsNullOrWhiteSpace(message.Content))
                {
                    contentBlocks.Add(new AnthropicSdk.TextContent { Text = message.Content });
                }

                // Add tool use blocks
                foreach (var toolCall in message.ToolCalls)
                {
                    // Convert Dictionary to JsonNode
                    var inputJson = JsonSerializer.SerializeToNode(toolCall.Input);

                    contentBlocks.Add(new AnthropicSdk.ToolUseContent
                    {
                        Id = toolCall.Id,
                        Name = toolCall.Name,
                        Input = inputJson
                    });
                }

                result.Add(new AnthropicSdk.Message
                {
                    Role = AnthropicSdk.RoleType.Assistant,
                    Content = contentBlocks
                });
            }
            else
            {
                // Regular text message
                result.Add(new AnthropicSdk.Message
                {
                    Role = role,
                    Content = [new AnthropicSdk.TextContent { Text = message.Content ?? "" }]
                });
            }
        }

        return result;
    }

    /// <summary>
    /// Convert our ToolDefinition to Claude's Tool format
    /// </summary>
    private static Tool ConvertToolDefinition(ToolDefinition tool)
    {
        // Build properties object, only including enum if it has values
        var properties = tool.InputSchema.Properties?.ToDictionary(
            kvp => kvp.Key,
            kvp =>
            {
                var prop = new Dictionary<string, object>
                {
                    ["type"] = kvp.Value.Type,
                    ["description"] = kvp.Value.Description
                };

                if (kvp.Value.Enum?.Length > 0)
                {
                    prop["enum"] = kvp.Value.Enum;
                }

                return prop;
            }
        );

        // Convert our schema to a JsonNode for the Function constructor
        var schemaNode = JsonSerializer.SerializeToNode(new
        {
            type = tool.InputSchema.Type,
            properties,
            required = tool.InputSchema.Required ?? Array.Empty<string>()
        });

        // Create a Function with the schema
        var function = new Function(
            name: tool.Name,
            description: tool.Description,
            parameters: schemaNode
        );

        // Return as a Tool (implicit conversion from Function to Tool)
        return function;
    }

    /// <summary>
    /// Convert Claude's response to our LlmResponse format
    /// </summary>
    private static LlmResponse ConvertResponse(AnthropicSdk.MessageResponse response)
    {
        var text = string.Empty;
        var toolCalls = new List<ToolCall>();

        // Extract text and tool calls from content blocks
        foreach (var content in response.Content)
        {
            if (content is AnthropicSdk.TextContent textContent)
            {
                text += textContent.Text;
            }
            else if (content is AnthropicSdk.ToolUseContent toolUse)
            {
                // Convert JsonNode to Dictionary<string, object>
                var input = toolUse.Input?.Deserialize<Dictionary<string, object>>() ?? [];

                toolCalls.Add(new ToolCall
                {
                    Id = toolUse.Id,
                    Name = toolUse.Name,
                    Input = input
                });
            }
        }

        // Calculate token count
        var tokenCount = (response.Usage?.InputTokens ?? 0) + (response.Usage?.OutputTokens ?? 0);

        return new LlmResponse
        {
            Text = text,
            ToolCalls = toolCalls,
            TokenCount = tokenCount,
            Model = response.Model,
            StopReason = response.StopReason
        };
    }
}
