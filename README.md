# Aide - Personal AI Assistant

## Overview
Aide is a .NET-native AI agent runtime and personal assistant that helps manage your digital communications and tasks through natural language interaction. Designed to support the Model Context Protocol (MCP), Aide provides a high-performance, extensible platform for AI-powered automation with first-class .NET ecosystem integration.

## Relationship to Moltbot/OpenClaw

Aide shares similar goals with [Moltbot/OpenClaw](https://github.com/moltbot/moltbot), the popular self-hosted AI assistant. Both projects aim to provide:
- Self-hosted, privacy-first personal AI assistants
- Plugin/capability-based extensibility
- Multi-service integration (email, Discord, etc.)
- Action-oriented intelligence that "actually does things"

**Aide's Differentiation:**
- **Native .NET Performance** - Built on .NET 10 for maximum performance and type safety
- **.NET Ecosystem Integration** - First-class support for .NET libraries, Active Directory, Windows services
- **Enterprise Focus** - Advanced security, compliance, audit logging, and enterprise deployment
- **MCP + C# Plugins** - Supports both Model Context Protocol and native C# plugins
- **Advanced Memory Architecture** - PostgreSQL + Vector DB for sophisticated context management
- **.NET Developer Experience** - Familiar tooling and patterns for C# developers

Aide is designed to be the best-in-class AI agent runtime for the .NET ecosystem while leveraging the broader MCP community.

## Architecture

Aide is built as a **capability runtime** rather than a monolithic application:

### Core Components
- **Runtime Engine** - .NET-based orchestration and execution environment
- **LLM Gateway** - Multi-provider support (Claude, GPT, Gemini, local models)
- **Capability System** - Dual plugin architecture:
  - **Native C# Plugins** - High-performance compiled capabilities for .NET developers (MVP)
  - **MCP Capabilities** - Leverage 100+ existing Model Context Protocol integrations (Planned)
- **Memory Service** - Advanced context persistence using PostgreSQL + Vector DB
- **Security Layer** - Plugin sandboxing, permission model, and audit logging
- **Audit System** - Comprehensive logging of all LLM actions, tool calls, and results
  - Track what AI decided and why
  - Monitor costs (token usage, API calls)
  - Compliance for financial/trading operations
  - Debugging and transparency
  - Immutable, searchable audit trail

### Capability Examples
Communication capabilities (via MCP or native plugins):
- **Email Integration** - Gmail, Outlook, IMAP/SMTP
- **Discord Integration** - Server monitoring, message management
- **Slack, Teams, etc.** - Additional messaging platforms

Intelligence capabilities:
- **Task Management** - Create, track, and complete tasks
- **Smart Filtering** - Spam detection and priority routing
- **Context Analysis** - Message categorization and insights
- **Automation** - Custom workflows and batch operations

Interaction capabilities:
- **Natural Language Chat** - Text-based interface
- **Voice Input** - Speech-to-text (OpenAI Whisper, Azure Speech, etc.)
- **Voice Output** - AI-powered text-to-speech (ElevenLabs, OpenAI TTS, Azure Neural Voices)
- **Hands-free Mode** - Continuous voice conversation
- **Desktop Integration** - Windows native features

## Technology Stack

### Core Runtime
- **.NET 10** - High-performance runtime
- **ASP.NET Core** - Web API and services
- **C# 13** - Utilizing the most modern C# syntax and language features available
  - Primary constructors
  - Collection expressions
  - Required members
  - File-scoped types
  - Raw string literals
  - Pattern matching enhancements

### Capability System
- **Model Context Protocol (MCP)** - Standard protocol for AI tool integration
- **AssemblyLoadContext** - Dynamic plugin loading and isolation
- **Native C# Plugins** - Compiled capabilities using `ICapability` interface

### LLM Integration (Multi-Provider)
- **Anthropic Claude** - Claude Opus, Sonnet, Haiku
- **OpenAI** - GPT-4, GPT-4 Turbo, GPT-3.5
- **Google Gemini** - Gemini Pro, Ultra
- **Local Models** - Ollama, LM Studio
- **Custom** - Any OpenAI-compatible API

### Data Layer
- **PostgreSQL** - Structured data, tasks, audit logs
- **Vector Database** - Pinecone, Weaviate, or Chroma for semantic memory
- **Redis** - Caching, sessions, real-time state

### Voice Integration (Planned)
- **Text-to-Speech** - ElevenLabs (primary), OpenAI TTS, Azure Neural Voices, Google Cloud TTS
- **Speech-to-Text** - OpenAI Whisper, Azure Speech Services, Google Speech-to-Text
- **Multi-provider support** - Abstraction layer for switching voice providers

### Frontend
- **.NET MAUI Blazor Hybrid** - Cross-platform mobile and desktop (iOS, Android, Windows, macOS)
- **Blazor** - Web-based UI components in native app shell
- **MudBlazor** - Material Design component library for styling and UI components
- **Tailwind CSS** - Utility-first CSS for positioning, layouts, and animations
- **Platform Integration** - Native features per platform (microphone, audio playback)

## Project Structure (Planned)
```
Aide/
├── src/
│   ├── Aide.Core/                    # Core runtime and abstractions
│   │   ├── Abstractions/             # ICapability, ILlmProvider, etc.
│   │   ├── Runtime/                  # Plugin loader, orchestrator
│   │   ├── Services/                 # Memory, LLM gateway, security
│   │   └── Mcp/                      # Model Context Protocol client
│   │
│   ├── Aide.Api/                     # ASP.NET Core Web API
│   │   ├── Controllers/
│   │   ├── SignalR/                  # Real-time communication
│   │   └── Program.cs
│   │
│   ├── Aide.Capabilities/            # Built-in native capabilities
│   │   ├── Email/
│   │   ├── Discord/
│   │   ├── Tasks/
│   │   └── FileSystem/
│   │
│   ├── Aide.Sdk/                     # SDK for plugin developers
│   │   ├── CapabilityBase.cs
│   │   ├── Attributes/
│   │   └── Testing/
│   │
│   ├── Aide.Voice/                  # Voice interaction services (planned)
│   │   ├── Abstractions/
│   │   │   ├── ITextToSpeechProvider.cs
│   │   │   └── ISpeechToTextProvider.cs
│   │   ├── Providers/
│   │   │   ├── ElevenLabsProvider.cs
│   │   │   ├── OpenAiTtsProvider.cs
│   │   │   └── WhisperProvider.cs
│   │   └── VoiceOrchestrator.cs
│   │
│   └── Aide.Ui/                     # .NET MAUI Blazor Hybrid UI
│       ├── Platforms/               # iOS, Android, Windows, macOS
│       ├── Components/              # Blazor components
│       │   ├── Pages/               # Chat, Settings, Capabilities
│       │   ├── Shared/              # Layout, NavMenu
│       │   └── UI/                  # Reusable UI components
│       ├── wwwroot/                 # Static assets, Tailwind CSS
│       │   ├── css/
│       │   ├── js/
│       │   └── index.html
│       ├── Services/                # API client, state management
│       └── MauiProgram.cs
│
├── plugins/                          # External plugins (MCP + C# DLLs)
│   ├── mcp-servers/                 # MCP server configurations
│   └── custom/                      # Custom C# plugin DLLs
│
├── docs/                            # Documentation
│   ├── architecture.md
│   ├── plugin-development.md
│   └── mcp-integration.md
│
└── tests/                           # Unit and integration tests
```

## Development Roadmap

### MVP (Phase 1-7)
**Goal:** Working AI agent with Claude integration, sample capabilities, and audit logging

- [ ] Solution and project structure setup
- [ ] Core abstractions (`ICapability`, `ILlmProvider`)
- [ ] Capability registry and LLM orchestrator
- [ ] Claude provider implementation
- [ ] Audit logging service
- [ ] Sample capabilities (Hello World, System Info)
- [ ] ASP.NET Core API with chat endpoint
- [ ] MAUI Blazor Hybrid UI with MudBlazor + Tailwind
- [ ] End-to-end testing and documentation

**Estimated Time:** 20-25 hours (experienced .NET developer), 30-40 hours (learning)

### Post-MVP Enhancements

**Phase 8.1: Additional LLM Providers**
- [ ] OpenAI (GPT-4) provider
- [ ] Google Gemini provider
- [ ] Ollama (local models) provider

**Phase 8.2: MCP Integration**
- [ ] MCP protocol client implementation
- [ ] MCP server discovery and lifecycle management
- [ ] Bridge MCP capabilities to `ICapability` interface
- [ ] Test with existing MCP servers

**Phase 8.3: Plugin Loading**
- [ ] `AssemblyLoadContext`-based dynamic plugin loader
- [ ] Load C# DLLs from directories
- [ ] Plugin manifest files and hot reload

**Phase 8.4: Real Capabilities**
- [ ] Email capability (Gmail/Outlook)
- [ ] Discord capability
- [ ] File system capability
- [ ] Task management capability
- [ ] Binance trading capability

**Phase 8.5: Voice Interaction**
- [ ] Voice provider abstraction
- [ ] ElevenLabs TTS integration
- [ ] OpenAI Whisper STT integration
- [ ] Voice UI in MAUI app

**Phase 8.6: Advanced Features**
- [ ] PostgreSQL + EF Core integration
- [ ] Vector database for semantic memory
- [ ] Multi-user support and authentication
- [ ] Plugin marketplace

## Core Principles
- **Self-hosted** - Run entirely on your own infrastructure
- **Privacy-first** - No data sharing with third parties
- **Extensible** - MCP + native C# plugin support
- **Multi-LLM** - Not locked into a single provider
- **Type-safe** - Leverage C# and .NET's type system
- **Secure** - Plugin sandboxing, encrypted credentials, audit logging
- **Enterprise-ready** - Active Directory, compliance, deployment tools

## Getting Started
*Coming soon - initial setup instructions will be added as the project develops*

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on:
- Conventional commits
- Pull request process
- Code standards
- Development workflow

**Repository:** `https://github.com/saib-inc/aide`

## License
*To be determined*

## Development Status
🚧 **Planning Phase** - Architecture and technology decisions in progress
