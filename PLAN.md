# Aide Development Plan - MVP

## Overview
Build a minimal viable AI agent runtime with:
- Core capability/skill system
- LLM orchestration layer (Claude with multi-provider support)
- Basic .NET MAUI UI
- One sample capability (Hello World)
- Foundation for MCP integration (future)

## Tech Stack
- **.NET 10** (SDK 10.0.102)
- **C# 13** - Modern syntax (primary constructors, collection expressions, etc.)
- **ASP.NET Core** - Web API
- **.NET MAUI Blazor Hybrid** - Cross-platform UI (iOS, Android, Windows, macOS)
- **Blazor** - Web components in native app
- **MudBlazor** - Material Design component library
- **Tailwind CSS** - Utility-first CSS for layouts and animations
- **Anthropic SDK** - Claude integration
- **PostgreSQL** - Future data persistence

---

## Phase 1: Project Foundation

### 1.1 Solution Structure
- [ ] Create solution `Aide.sln`
- [ ] Create `Aide.Core` class library (abstractions + core services)
- [ ] Create `Aide.Api` ASP.NET Core Web API
- [ ] Create `Aide.Capabilities` class library (built-in capabilities)
- [ ] Create `Aide.Ui` .NET MAUI app
- [ ] Configure project references
- [ ] Set up `.gitignore` for .NET projects
- [ ] Initialize git repository

**Files to create:**
```
Aide/
├── Aide.sln
├── .gitignore
├── README.md (update)
└── src/
    ├── Aide.Core/
    ├── Aide.Api/
    ├── Aide.Capabilities/
    └── Aide.Ui/
```

**Time estimate:** 30 minutes

---

## Phase 2: Core Abstractions

### 2.1 Capability Interface
**File:** `src/Aide.Core/Abstractions/ICapability.cs`

- [ ] Define `ICapability` interface
- [ ] Define `CapabilityContext` class
- [ ] Define `CapabilityResult` class
- [ ] Define `ToolSchema` classes (for LLM tool definitions)

**Interface Definition:**
```csharp
public interface ICapability
{
    string Name { get; }
    string Description { get; }

    // Returns JSON schema for LLM tool definition
    ToolSchema GetInputSchema();

    Task<CapabilityResult> ExecuteAsync(CapabilityContext context);
}

public class CapabilityContext
{
    public required string Input { get; init; }
    public required Dictionary<string, object> Parameters { get; init; }
    public CancellationToken CancellationToken { get; init; }
}

public class CapabilityResult
{
    public required bool Success { get; init; }
    public string? Output { get; init; }
    public object? Data { get; init; }
    public string? ErrorMessage { get; init; }
    public string? ErrorCode { get; init; }  // For programmatic error handling
}

public record ToolSchema(
    string Type,  // "object"
    Dictionary<string, PropertySchema> Properties,
    string[] Required
);

public record PropertySchema(
    string Type,  // "string", "number", "boolean", "array"
    string Description,
    string[]? Enum = null,
    object? Default = null
);
```

**Requirements:**
- Simple, clean interface
- Support for async execution
- JSON-serializable context and results
- Tool schema for LLM integration
- Error handling with ErrorMessage and ErrorCode

**Time estimate:** 45 minutes

### 2.2 LLM Provider Interface
**File:** `src/Aide.Core/Abstractions/ILlmProvider.cs`

- [ ] Define `ILlmProvider` interface (multi-provider abstraction)
- [ ] Define `LlmRequest` class
- [ ] Define `LlmResponse` class
- [ ] Define `ToolDefinition` class
- [ ] Define `ToolCall` class
- [ ] Define `ToolResult` class

**Requirements:**
- Provider-agnostic (works with Claude, GPT, Gemini, etc.)
- Support tool/function calling
- Support multi-turn conversations
- Streaming support (optional for MVP)

**Time estimate:** 45 minutes

---

## Phase 3: Core Services

### 3.1 Capability Registry
**File:** `src/Aide.Core/Services/CapabilityRegistry.cs`

- [ ] Implement in-memory capability registry
- [ ] `Register(ICapability)` method
- [ ] `Get(string name)` method
- [ ] `GetAll()` method
- [ ] `ToToolDefinitions()` method (convert capabilities to LLM tools)

**Time estimate:** 30 minutes

### 3.2 LLM Orchestrator
**File:** `src/Aide.Core/Services/LlmOrchestrator.cs`

- [ ] Implement orchestration logic
- [ ] `ProcessUserInput(string sessionId, string input)` method
- [ ] Handle tool calling loop
- [ ] Execute capabilities based on LLM decisions
- [ ] Return final response to user
- [ ] Manage conversation history per session

**Implementation Approach:**
```csharp
public class LlmOrchestrator
{
    private readonly ILlmProvider _llmProvider;
    private readonly CapabilityRegistry _registry;
    private readonly AuditLogService _audit;

    // MVP: In-memory conversation history (lost on restart)
    // Future: Persist to database
    private readonly Dictionary<string, List<Message>> _conversationHistory = new();

    public async Task<string> ProcessUserInput(string sessionId, string userInput)
    {
        // Get or create conversation history for this session
        if (!_conversationHistory.ContainsKey(sessionId))
        {
            _conversationHistory[sessionId] = new List<Message>();
        }

        var history = _conversationHistory[sessionId];
        history.Add(new Message { Role = "user", Content = userInput });

        // Get available tools from capabilities
        var tools = _registry.GetAll()
            .Select(cap => ConvertToToolDefinition(cap))
            .ToList();

        // Tool calling loop
        while (true)
        {
            var response = await _llmProvider.SendAsync(new LlmRequest
            {
                Messages = history,
                Tools = tools
            });

            // No tools to call, return final response
            if (!response.ToolCalls.Any())
            {
                history.Add(new Message { Role = "assistant", Content = response.Text });
                return response.Text;
            }

            // Execute tools
            history.Add(new Message { Role = "assistant", ToolCalls = response.ToolCalls });

            foreach (var toolCall in response.ToolCalls)
            {
                var result = await ExecuteCapability(toolCall);
                history.Add(new Message { Role = "tool", ToolCallId = toolCall.Id, Content = result });
            }
        }
    }

    private async Task<string> ExecuteCapability(ToolCall toolCall)
    {
        try
        {
            var capability = _registry.Get(toolCall.Name);
            var context = new CapabilityContext
            {
                Input = toolCall.Input.GetValueOrDefault("action")?.ToString() ?? "",
                Parameters = toolCall.Input
            };

            var result = await capability.ExecuteAsync(context);

            if (!result.Success)
            {
                return $"Error: {result.ErrorMessage}";
            }

            return result.Output ?? JsonSerializer.Serialize(result.Data);
        }
        catch (Exception ex)
        {
            return $"Exception: {ex.Message}";
        }
    }
}
```

**Requirements:**
- Use `ILlmProvider` abstraction
- Support multi-step tool execution
- Error handling for capability failures
- Conversation history management (in-memory for MVP)
- Session-based conversation isolation

**Time estimate:** 2.5 hours

### 3.3 Claude Provider Implementation
**File:** `src/Aide.Core/Providers/ClaudeProvider.cs`

- [ ] Implement `ILlmProvider` for Anthropic Claude
- [ ] Install `Anthropic` NuGet package
- [ ] Configure API key from settings
- [ ] Implement tool calling
- [ ] Map Claude's tool format to our abstractions

**Time estimate:** 1.5 hours

### 3.4 Audit Log Service
**File:** `src/Aide.Core/Services/AuditLogService.cs`

- [ ] Implement comprehensive audit logging for **all LLM actions and capability executions**
- [ ] `LogUserMessage(string userId, string message)` - Log user input
- [ ] `LogLlmResponse(string response, int tokenCount, decimal? cost)` - Log LLM output
- [ ] `LogToolCall(string toolName, object input)` - Log when LLM decides to call a tool
- [ ] `LogToolResult(string toolName, object result, TimeSpan duration, bool success)` - Log tool execution results
- [ ] `LogConversationTurn(ConversationTurn turn)` - Log complete interaction (user → LLM → tools → response)
- [ ] Storage options: in-memory (MVP), file-based JSON, or database (future)
- [ ] Privacy considerations: redact sensitive data (API keys, passwords, PII) from logs
- [ ] Include metadata: session ID, timestamp, model used, token usage, costs

**File:** `src/Aide.Core/Models/AuditLog.cs`

Define audit log models:

```csharp
public record AuditLogEntry
{
    public Guid Id { get; init; }
    public DateTime Timestamp { get; init; }
    public string SessionId { get; init; }
    public string UserId { get; init; }
    public AuditEventType EventType { get; init; }
    public object Data { get; init; }
}

public enum AuditEventType
{
    UserMessage,
    LlmResponse,
    ToolCall,
    ToolResult,
    Error
}

public record UserMessageEvent(string Message);
public record LlmResponseEvent(string Response, int TokenCount, decimal? Cost, string Model);
public record ToolCallEvent(string ToolName, object Input);
public record ToolResultEvent(string ToolName, object Result, TimeSpan Duration, bool Success, string? Error);
```

**Integration Approach - Decorator Pattern:**

The audit logging integrates via decorator pattern around `ILlmProvider`:

```csharp
public class AuditedLlmProvider : ILlmProvider
{
    private readonly ILlmProvider _inner;
    private readonly AuditLogService _audit;

    public AuditedLlmProvider(ILlmProvider inner, AuditLogService audit)
    {
        _inner = inner;
        _audit = audit;
    }

    public async Task<LlmResponse> SendAsync(LlmRequest request)
    {
        // Log user message
        var userMessage = request.Messages.LastOrDefault(m => m.Role == "user");
        if (userMessage != null)
        {
            await _audit.LogUserMessage(request.SessionId, userMessage.Content);
        }

        // Call inner provider
        var response = await _inner.SendAsync(request);

        // Log LLM response
        await _audit.LogLlmResponse(
            response.Text,
            response.TokenCount,
            response.Cost,
            response.Model
        );

        // Log tool calls
        foreach (var toolCall in response.ToolCalls)
        {
            await _audit.LogToolCall(toolCall.Name, toolCall.Input);
        }

        return response;
    }
}

// Capability execution logging happens in LlmOrchestrator.ExecuteCapability()
private async Task<string> ExecuteCapability(ToolCall toolCall)
{
    var stopwatch = Stopwatch.StartNew();

    try
    {
        var capability = _registry.Get(toolCall.Name);
        var result = await capability.ExecuteAsync(context);

        stopwatch.Stop();
        await _audit.LogToolResult(
            toolCall.Name,
            result.Output,
            stopwatch.Elapsed,
            result.Success,
            result.ErrorMessage
        );

        return result.Output;
    }
    catch (Exception ex)
    {
        stopwatch.Stop();
        await _audit.LogToolResult(
            toolCall.Name,
            null,
            stopwatch.Elapsed,
            false,
            ex.Message
        );
        throw;
    }
}
```

**Registration in Program.cs:**
```csharp
builder.Services.AddSingleton<AuditLogService>();
builder.Services.AddScoped<ClaudeProvider>();  // Concrete implementation
builder.Services.AddScoped<ILlmProvider>(sp =>
    new AuditedLlmProvider(
        sp.GetRequiredService<ClaudeProvider>(),
        sp.GetRequiredService<AuditLogService>()
    )
);
```

**Integration Points:**
- Decorator pattern wraps `ILlmProvider` to capture LLM interactions automatically
- `LlmOrchestrator` logs capability execution results
- All logging happens transparently without polluting business logic

**Example Audit Trail:**
```json
[
  {
    "id": "uuid",
    "timestamp": "2026-02-02T10:30:00Z",
    "sessionId": "session-123",
    "userId": "user-456",
    "eventType": "UserMessage",
    "data": { "message": "What's the BTC price?" }
  },
  {
    "eventType": "ToolCall",
    "data": { "toolName": "binance", "input": { "action": "get_price", "symbol": "BTCUSDT" } }
  },
  {
    "eventType": "ToolResult",
    "data": { "toolName": "binance", "result": "$67,432", "duration": "0.5s", "success": true }
  },
  {
    "eventType": "LlmResponse",
    "data": { "response": "Bitcoin is currently $67,432", "tokenCount": 150, "cost": 0.001, "model": "claude-sonnet-4.5" }
  }
]
```

**Requirements:**
- Immutable logs (append-only, never modify)
- Searchable by session, user, timestamp, event type, tool name
- Exportable (JSON, CSV) for analysis or compliance
- UI to view audit trail with filtering
- Configurable retention policy (e.g., keep 90 days)
- **Critical for:**
  - Security (who did what, when)
  - Debugging (reproduce issues, understand LLM reasoning)
  - Compliance (especially for trading/financial operations)
  - Cost tracking (monitor token usage and API costs)
  - User transparency (show what Aide did and why)

**Time estimate:** 2 hours

---

## Phase 4: Sample Capability

### 4.1 Hello World Capability
**File:** `src/Aide.Capabilities/HelloWorldCapability.cs`

- [ ] Implement `ICapability`
- [ ] Simple greeting functionality
- [ ] Support name parameter
- [ ] Return friendly message

**Example:**
```
User: "Say hello"
Capability: "Hello! 👋"

User: "Greet Alice"
Capability: "Hello, Alice! 👋"
```

**Time estimate:** 20 minutes

### 4.2 System Info Capability
**File:** `src/Aide.Capabilities/SystemInfoCapability.cs`

- [ ] Implement `ICapability`
- [ ] Get system information (OS, memory, disk, etc.)
- [ ] Return formatted info

**Example:**
```
User: "What's my system info?"
Capability: "OS: Windows 11, RAM: 16GB, Disk: 500GB free"
```

**Time estimate:** 30 minutes

---

## Phase 5: API Layer

### 5.1 ASP.NET Core Setup
**File:** `src/Aide.Api/Program.cs`

- [ ] Configure dependency injection
- [ ] Register `CapabilityRegistry`
- [ ] Register `LlmOrchestrator`
- [ ] Register `ClaudeProvider`
- [ ] Register built-in capabilities
- [ ] Configure CORS for MAUI app
- [ ] Configure app settings (API keys, etc.)

**Time estimate:** 45 minutes

### 5.2 Chat Controller
**File:** `src/Aide.Api/Controllers/ChatController.cs`

- [ ] `POST /api/chat` endpoint
- [ ] Accept user message
- [ ] Call `LlmOrchestrator`
- [ ] Return response
- [ ] Error handling

**Time estimate:** 30 minutes

### 5.3 Capabilities Controller
**File:** `src/Aide.Api/Controllers/CapabilitiesController.cs`

- [ ] `GET /api/capabilities` endpoint (list all)
- [ ] `POST /api/capabilities/{name}/execute` endpoint (manual execution)
- [ ] Return capability metadata

**Time estimate:** 30 minutes

### 5.4 Audit Log Controller
**File:** `src/Aide.Api/Controllers/AuditLogController.cs`

- [ ] `GET /api/audit` endpoint (list audit logs with filtering)
- [ ] Query parameters: sessionId, userId, startDate, endDate, eventType, toolName
- [ ] `GET /api/audit/{id}` endpoint (get specific log entry)
- [ ] `GET /api/audit/export` endpoint (export as JSON/CSV)
- [ ] `GET /api/audit/sessions/{sessionId}` endpoint (get full conversation trail)
- [ ] Pagination support for large logs

**Time estimate:** 45 minutes

### 5.5 Configuration
**Multiple Configuration Sources:**

Configuration is loaded from multiple sources with priority:
1. Environment variables (highest priority)
2. `~/.aide/appsettings.json` (user-level settings)
3. `appsettings.Development.json` (environment-specific)
4. `appsettings.json` (base settings)

This allows:
- **Local config** for project/team settings (checked into git)
- **User config** for personal API keys, plugins, preferences (not in git)
- **Environment variables** for deployment/CI secrets

**Setup in Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add user-level configuration
var userConfigPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
    ".aide",
    "appsettings.json"
);

if (File.Exists(userConfigPath))
{
    builder.Configuration.AddJsonFile(userConfigPath, optional: true, reloadOnChange: true);
}

// Environment variables override everything
builder.Configuration.AddEnvironmentVariables(prefix: "AIDE_");
```

**File:** `src/Aide.Api/appsettings.json` (Base Configuration)

```json
{
  "Aide": {
    "Llm": {
      "DefaultProvider": "Claude",
      "Providers": {
        "Claude": {
          "ApiKey": "",  // Override in user config or env vars
          "Model": "claude-sonnet-4-5-20250929",
          "MaxTokens": 4096,
          "Temperature": 1.0
        }
      }
    },
    "AuditLog": {
      "Enabled": true,
      "StorageType": "InMemory",
      "RetentionDays": 90,
      "RedactSensitiveData": true,
      "SensitiveKeys": ["apiKey", "password", "secret", "token"]
    },
    "Plugins": {
      "Directories": ["./plugins"],
      "Enabled": ["hello-world", "system-info"]
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": ["http://localhost:*", "app://localhost"]
  }
}
```

**File:** `~/.aide/appsettings.json` (User Configuration - Example)

```json
{
  "Aide": {
    "Llm": {
      "Providers": {
        "Claude": {
          "ApiKey": "sk-ant-api03-YOUR-PERSONAL-KEY-HERE"
        },
        "OpenAI": {
          "ApiKey": "sk-YOUR-OPENAI-KEY",
          "Model": "gpt-4",
          "MaxTokens": 4096
        }
      }
    },
    "Plugins": {
      "Directories": ["./plugins", "~/.aide/plugins"],
      "Enabled": ["hello-world", "system-info", "my-custom-plugin"]
    }
  }
}
```

**Environment Variables (Deployment/CI):**
```bash
# Override via environment
export AIDE_Llm__Providers__Claude__ApiKey="sk-ant-api03-production-key"
export AIDE_Llm__DefaultProvider="Claude"
export AIDE_AuditLog__StorageType="Database"
```

**Benefits:**
- ✅ Secrets never committed to git (use ~/.aide/ or env vars)
- ✅ Team settings in appsettings.json (shared defaults)
- ✅ Personal settings in ~/.aide/ (your API keys, custom plugins)
- ✅ Production settings via environment variables
- ✅ Local development with appsettings.Development.json

**Time estimate:** 30 minutes

---

## Phase 6: MAUI Blazor Hybrid UI

### 6.1 Project Setup
**File:** `src/Aide.Ui/MauiProgram.cs`

- [ ] Create .NET MAUI Blazor Hybrid project
- [ ] Install MudBlazor NuGet package
- [ ] Install Tailwind CSS (via CDN or build process)
- [ ] Configure MAUI app
- [ ] Register Blazor services
- [ ] Register MudBlazor services
- [ ] Register HTTP client for API calls
- [ ] Configure dependency injection

**File:** `src/Aide.Ui/wwwroot/index.html`

- [ ] Add MudBlazor CSS/JS
- [ ] Add Tailwind CSS (via build output)
- [ ] Configure fonts and icons (Material Icons)

**Tailwind Setup (using Bun):**

**File:** `src/Aide.Ui/wwwroot/package.json`
```json
{
  "scripts": {
    "dev": "concurrently \"bun run styles:watch\" \"bun run scripts:watch\"",
    "build": "concurrently \"bun run styles:build\" \"bun run scripts:build\"",
    "styles:watch": "tailwindcss -i ./styles/app.css -o ./dist/app.bundle.css --watch",
    "styles:build": "tailwindcss -i ./styles/app.css -o ./dist/app.bundle.css --minify"
  },
  "devDependencies": {
    "tailwindcss": "^4.1.0",
    "concurrently": "^9.0.0"
  }
}
```

**File:** `src/Aide.Ui/wwwroot/tailwind.config.js`
```javascript
export default {
    content: ["../**/*.{razor,cs}"],
    plugins: [],
};
```

**File:** `src/Aide.Ui/wwwroot/styles/app.css`
```css
@config "./tailwind.config.js";
@import "tailwindcss";

/* Custom CSS variables */
:root {
    --aide-primary: #2563eb;
    --aide-secondary: #7c3aed;
}

/* MudBlazor overrides */
[&_.mud-tabs-tabbar-wrapper] {
    @apply !max-w-full;
}
```

**Installation:**
```bash
cd src/Aide.Ui/wwwroot
bun install
bun run dev  # Development with watch
bun run build  # Production build
```

**Time estimate:** 1.5 hours

### 6.2 Layout and Navigation
**File:** `src/Aide.Ui/Components/Shared/MainLayout.razor`

- [ ] Create MudLayout with drawer
- [ ] Navigation menu (Chat, Capabilities, Settings)
- [ ] App bar with title
- [ ] Theme toggle (light/dark mode)

**File:** `src/Aide.Ui/Components/Shared/NavMenu.razor`

- [ ] Navigation links using MudNavMenu
- [ ] Icons for each section

**Time estimate:** 1 hour

### 6.3 Chat Page
**File:** `src/Aide.Ui/Components/Pages/Chat.razor`

- [ ] Create chat interface using MudBlazor components
- [ ] Message list (MudPaper for message bubbles)
- [ ] User vs assistant message styling
- [ ] MudTextField for input
- [ ] MudButton for send
- [ ] Auto-scroll to bottom on new message
- [ ] Loading indicator while waiting for response
- [ ] Tailwind classes for positioning and animations

**Example structure:**
```razor
@page "/chat"

<MudContainer MaxWidth="MaxWidth.Large" Class="h-screen flex flex-col">
    <!-- Messages -->
    <div class="flex-1 overflow-y-auto p-4">
        @foreach (var message in Messages)
        {
            <MudPaper Class="@GetMessageClass(message) mb-3 p-3">
                @message.Text
            </MudPaper>
        }
    </div>

    <!-- Input -->
    <div class="border-t p-4">
        <MudTextField @bind-Value="InputText"
                      Label="Type a message..."
                      Variant="Variant.Outlined"
                      OnKeyPress="HandleKeyPress" />
        <MudButton OnClick="SendMessage"
                   Color="Color.Primary"
                   Variant="Variant.Filled">
            Send
        </MudButton>
    </div>
</MudContainer>
```

**File:** `src/Aide.Ui/Components/Pages/Chat.razor.cs`

- [ ] Implement code-behind
- [ ] Message collection (ObservableCollection or List)
- [ ] Handle send message
- [ ] Call API via AideApiClient
- [ ] Update UI with response
- [ ] Error handling with MudSnackbar

**Time estimate:** 2-3 hours

### 6.4 App State Service
**File:** `src/Aide.Ui/Services/AppStateService.cs`

- [ ] Create state management service (based on Buriza pattern)
- [ ] `IsDarkMode` property with change notification
- [ ] `OnChanged` event for UI reactivity
- [ ] Thread-safe state updates

**Implementation:**
```csharp
public class AppStateService
{
    private bool _isDarkMode = true;
    public bool IsDarkMode
    {
        get => _isDarkMode;
        set
        {
            if (_isDarkMode != value)
            {
                _isDarkMode = value;
                NotifyChanged();
            }
        }
    }

    private bool _isSidebarOpen = false;
    public bool IsSidebarOpen
    {
        get => _isSidebarOpen;
        set
        {
            if (_isSidebarOpen != value)
            {
                _isSidebarOpen = value;
                NotifyChanged();
            }
        }
    }

    public event Action? OnChanged;
    private void NotifyChanged() => OnChanged?.Invoke();
}
```

**Registration:**
```csharp
builder.Services.AddScoped<AppStateService>();
```

**Usage in Components:**
```csharp
[Inject]
public required AppStateService AppStateService { get; set; }

protected override void OnInitialized()
{
    AppStateService.OnChanged += HandleStateChanged;
}

private async void HandleStateChanged()
{
    await InvokeAsync(StateHasChanged);
}

public void Dispose()
{
    AppStateService.OnChanged -= HandleStateChanged;
}
```

**Time estimate:** 30 minutes

### 6.5 API Client
**File:** `src/Aide.Ui/Services/AideApiClient.cs`

- [ ] Create HTTP client wrapper
- [ ] `SendMessageAsync(string sessionId, string message)` method
- [ ] `GetCapabilitiesAsync()` method
- [ ] `GetAuditLogsAsync()` method (optional)
- [ ] Error handling

**Time estimate:** 45 minutes

### 6.6 Audit Log Viewer Page (Optional for MVP)
**File:** `src/Aide.Ui/Components/Pages/AuditLog.razor`

- [ ] Create audit log viewer using MudTable
- [ ] Display log entries with filtering
- [ ] Expandable rows for detailed view
- [ ] Color coding by event type (user message, tool call, error)
- [ ] Search by session, tool name, date range
- [ ] Export functionality
- [ ] Useful for debugging and transparency

**Time estimate:** 2 hours (optional, can be post-MVP)

### 6.7 Styling
**File:** `src/Aide.Ui/wwwroot/css/app.css`

- [ ] Import Tailwind base, components, utilities
- [ ] Custom CSS for chat bubbles
- [ ] Animations (fade-in for messages)
- [ ] Responsive design utilities

**File:** `tailwind.config.js` (if using Tailwind build process)

- [ ] Configure content paths
- [ ] Custom theme colors
- [ ] Animation configurations

**Time estimate:** 1 hour

---

## Phase 7: Testing & Documentation

### 7.1 Manual Testing
- [ ] Run API and UI together
- [ ] Test hello world capability
- [ ] Test system info capability
- [ ] Test error handling
- [ ] Test on multiple platforms (Windows, macOS if available)

**Time estimate:** 1 hour

### 7.2 Documentation
- [ ] Update README.md with setup instructions
- [ ] Add "Getting Started" section
- [ ] Document API endpoints
- [ ] Document how to create new capabilities
- [ ] Add screenshots (optional)

**Time estimate:** 1 hour

---

## Phase 8: Future Enhancements (Post-MVP)

### 8.1 Additional LLM Providers
- [ ] Implement `OpenAiProvider` (GPT-4, etc.)
- [ ] Implement `GeminiProvider` (Google)
- [ ] Implement `OllamaProvider` (local models)
- [ ] Provider selection in settings

### 8.2 MCP Integration
- [ ] Implement MCP protocol client
- [ ] MCP server lifecycle management (launch, connect, shutdown)
- [ ] Bridge MCP capabilities to `ICapability`
- [ ] Configuration-driven MCP server discovery

**MCP Configuration (same config system):**

```json
// In appsettings.json or ~/.aide/appsettings.json
{
  "Aide": {
    "McpServers": {
      "filesystem": {
        "command": "npx",
        "args": ["-y", "@modelcontextprotocol/server-filesystem", "/Users/you/projects"],
        "env": {},
        "enabled": true
      },
      "binance": {
        "command": "python",
        "args": ["./mcp-servers/binance/server.py"],
        "env": {
          "BINANCE_API_KEY": "${BINANCE_API_KEY}",  // Read from env var
          "BINANCE_API_SECRET": "${BINANCE_API_SECRET}"
        },
        "enabled": true
      },
      "email": {
        "command": "node",
        "args": ["./mcp-servers/email/server.js"],
        "env": {
          "GMAIL_API_KEY": ""  // Override in ~/.aide/appsettings.json
        },
        "enabled": false  // Disabled by default
      }
    }
  }
}
```

**User-level MCP config** (`~/.aide/appsettings.json`):
```json
{
  "Aide": {
    "McpServers": {
      "binance": {
        "env": {
          "BINANCE_API_KEY": "your-personal-key",
          "BINANCE_API_SECRET": "your-personal-secret"
        },
        "enabled": true
      },
      "my-custom-mcp-server": {
        "command": "~/my-mcp-servers/custom/server",
        "args": [],
        "env": {},
        "enabled": true
      }
    }
  }
}
```

**Benefits:**
- Same configuration system for C# plugins and MCP servers
- Secrets in user-level config (never committed)
- Enable/disable servers without deleting files
- Environment variable substitution for CI/deployment

### 8.3 Plugin Loading
- [ ] Implement `AssemblyLoadContext`-based loader
- [ ] Load C# DLLs from configured directories
- [ ] Reflection-based `ICapability` discovery
- [ ] Configuration-based plugin enable/disable
- [ ] Hot reload support (optional)

**Configuration-Driven Plugin Loading:**

```json
// In appsettings.json or ~/.aide/appsettings.json
{
  "Aide": {
    "Plugins": {
      "Directories": [
        "./plugins",           // Local project plugins
        "~/.aide/plugins"      // User-level plugins
      ],
      "Enabled": [
        "hello-world",
        "system-info",
        "binance-trader"       // Your custom plugin
      ],
      "Disabled": [
        "experimental-feature" // Temporarily disable without deleting DLL
      ]
    }
  }
}
```

**Plugin Loader Implementation:**

```csharp
public class PluginLoader
{
    private readonly IConfiguration _config;

    public IEnumerable<ICapability> LoadPlugins()
    {
        var capabilities = new List<ICapability>();
        var directories = _config.GetSection("Aide:Plugins:Directories").Get<string[]>();
        var enabled = _config.GetSection("Aide:Plugins:Enabled").Get<string[]>()?.ToHashSet();
        var disabled = _config.GetSection("Aide:Plugins:Disabled").Get<string[]>()?.ToHashSet();

        foreach (var dir in directories)
        {
            var expandedDir = Environment.ExpandEnvironmentVariables(dir);
            if (!Directory.Exists(expandedDir)) continue;

            var dllFiles = Directory.GetFiles(expandedDir, "*.dll", SearchOption.AllDirectories);

            foreach (var dllPath in dllFiles)
            {
                try
                {
                    // Load assembly with isolation
                    var context = new PluginLoadContext(dllPath, isCollectible: true);
                    var assembly = context.LoadFromAssemblyPath(dllPath);

                    // Find ICapability implementations via reflection
                    var capabilityTypes = assembly.GetTypes()
                        .Where(t => typeof(ICapability).IsAssignableFrom(t)
                                && !t.IsInterface
                                && !t.IsAbstract);

                    foreach (var type in capabilityTypes)
                    {
                        var capability = (ICapability)Activator.CreateInstance(type);

                        // Check if enabled/disabled
                        if (disabled?.Contains(capability.Name) == true) continue;
                        if (enabled != null && !enabled.Contains(capability.Name)) continue;

                        capabilities.Add(capability);
                    }
                }
                catch (Exception ex)
                {
                    // Log and continue
                    Console.WriteLine($"Failed to load plugin from {dllPath}: {ex.Message}");
                }
            }
        }

        return capabilities;
    }
}
```

**No manifest files needed** - plugins are discovered via reflection and controlled via configuration.

### 8.4 Real Capabilities
- [ ] Email capability (Gmail/Outlook)
- [ ] Discord capability
- [ ] File system capability
- [ ] Task management capability
- [ ] Binance trading capability

### 8.5 Voice Interaction
- [ ] Implement `IVoiceProvider` abstraction
- [ ] Implement `ElevenLabsProvider` for TTS (text-to-speech)
- [ ] Implement speech-to-text capability (OpenAI Whisper, Azure Speech, etc.)
- [ ] Voice input in MAUI UI (microphone button)
- [ ] Voice output with audio playback
- [ ] Voice settings (voice selection, speed, pitch)
- [ ] Conversation mode (hands-free voice chat)
- [ ] Support for other voice providers (OpenAI TTS, Azure Neural Voices, etc.)

**Voice Providers to Consider:**
- **ElevenLabs** - High-quality, natural voices
- **OpenAI TTS** - Built-in with GPT API
- **Azure Neural Voices** - Enterprise-grade
- **Google Cloud TTS** - Multi-language support
- **AWS Polly** - Cost-effective

**Speech-to-Text Options:**
- **OpenAI Whisper** - Excellent accuracy
- **Azure Speech Services** - Real-time transcription
- **Google Speech-to-Text** - Multi-language
- **AssemblyAI** - Developer-friendly

### 8.6 Advanced Features
- [ ] Conversation history persistence
- [ ] PostgreSQL integration
- [ ] Vector database for memory
- [ ] User authentication
- [ ] Multi-user support
- [ ] Plugin marketplace

---

## Summary

**MVP Scope:**
- Core capability system ✓
- LLM orchestration with Claude ✓
- Multi-provider abstraction ✓
- 2 sample capabilities ✓
- ASP.NET Core API ✓
- .NET MAUI UI ✓
- End-to-end working demo ✓

**Total Estimated Time:**
- **Experienced .NET developer:** 20-25 hours
- **Learning MAUI/Blazor:** 30-40 hours

**Time Breakdown:**
- Phase 1 (Foundation): 0.5h
- Phase 2 (Abstractions): 1.5h
- Phase 3 (Services): 7h
- Phase 4 (Capabilities): 0.83h
- Phase 5 (API): 2.5h
- Phase 6 (UI): 8-10h
- Phase 7 (Testing): 2h

**Out of Scope (Post-MVP):**
- MCP integration
- Plugin loading from DLLs
- Additional LLM providers
- Real-world capabilities (email, Discord, etc.)
- Database persistence
- Advanced UI features

**Success Criteria:**
1. User can open MAUI app
2. User can type a message
3. Claude decides which capability to use
4. Capability executes
5. Response appears in chat
6. Architecture supports adding new capabilities easily
7. Code uses modern C# syntax throughout

**Next Steps After Plan Approval:**
1. Set up solution structure
2. Implement core abstractions
3. Build LLM orchestration layer
4. Create sample capabilities
5. Build API
6. Build UI
7. Test end-to-end
8. Document
