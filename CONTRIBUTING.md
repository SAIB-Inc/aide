# Contributing to Aide

Thank you for your interest in contributing to Aide! This document outlines the development workflow and guidelines for contributing to the project.

## Repository

This project is maintained at: `https://github.com/saib-inc/aide`

## Development Workflow

### 1. Fork and Clone

```bash
# Fork the repository on GitHub
# Clone your fork
git clone https://github.com/YOUR_USERNAME/aide.git
cd aide

# Add upstream remote
git remote add upstream https://github.com/saib-inc/aide.git
```

### 2. Create a Feature Branch

```bash
# Update your main branch
git checkout main
git pull upstream main

# Create a feature branch
git checkout -b feat/your-feature-name
# or
git checkout -b fix/bug-description
```

### 3. Make Your Changes

- Write clean, maintainable code following C# 13 best practices
- Use modern C# syntax (primary constructors, collection expressions, required members)
- Add unit tests for new functionality
- Update documentation as needed

### 4. Commit Your Changes

We use **Conventional Commits** for all commit messages.

#### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

#### Types

- **feat**: A new feature
- **fix**: A bug fix
- **docs**: Documentation only changes
- **style**: Code style changes (formatting, missing semicolons, etc.)
- **refactor**: Code refactoring without adding features or fixing bugs
- **perf**: Performance improvements
- **test**: Adding or updating tests
- **build**: Changes to build system or dependencies
- **ci**: Changes to CI configuration
- **chore**: Other changes that don't modify src or test files

#### Scopes (Optional)

- `core` - Aide.Core project
- `api` - Aide.Api project
- `capabilities` - Aide.Capabilities project
- `ui` - Aide.Ui project
- `llm` - LLM provider implementations
- `audit` - Audit logging system
- `config` - Configuration management

#### Examples

```bash
# Feature
git commit -m "feat(llm): add OpenAI provider support"

# Bug fix
git commit -m "fix(api): resolve null reference in chat controller"

# Documentation
git commit -m "docs: update README with installation instructions"

# Breaking change
git commit -m "feat(core)!: change ICapability interface signature

BREAKING CHANGE: ICapability.ExecuteAsync now requires sessionId parameter"
```

#### Commit Message Guidelines

- Use the imperative mood ("add feature" not "added feature")
- First line should be 50 characters or less
- Reference issues and pull requests when relevant
- Use `!` after type/scope for breaking changes
- Provide detailed body for complex changes

### 5. Push Your Changes

```bash
git push origin feat/your-feature-name
```

### 6. Open a Pull Request

1. Go to https://github.com/saib-inc/aide
2. Click "New Pull Request"
3. Select your fork and branch
4. Fill out the PR template with:
   - **Title**: Use conventional commit format (e.g., `feat(llm): add OpenAI provider`)
   - **Description**: Explain what and why
   - **Testing**: Describe how you tested the changes
   - **Screenshots**: If applicable (UI changes)

#### PR Title Format

Use the same conventional commit format:

```
feat(scope): add new feature
fix(scope): resolve bug
docs: update contributing guide
```

### 7. Code Review

- Address review comments
- Push additional commits to your branch (they will appear in the PR)
- Use conventional commits for all commits in the PR

### 8. Merge

- PRs are merged using **squash merge** strategy
- All commits in the PR will be squashed into a single commit
- The PR title becomes the commit message
- Ensure your PR title follows conventional commit format

## Code Standards

### C# Style

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- **One type per file** - Each class, interface, record, or enum should be in its own file
- Use C# 13 features where appropriate
- Use `required` for mandatory DI properties
- Prefer primary constructors for simple classes
- Use collection expressions (`[]` instead of `new List<>()`)
- Use file-scoped namespaces

### Project-Specific Guidelines

**Abstractions (Aide.Core/Abstractions):**
- One type per file (e.g., `ICapability.cs`, `CapabilityContext.cs`, `CapabilityResult.cs`)
- Keep interfaces minimal and focused
- Use records for DTOs and immutable types
- Document public APIs with XML comments

**Services (Aide.Core/Services):**
- Follow dependency injection patterns
- Use `required` for injected dependencies
- Implement proper disposal for resources

**Capabilities:**
- Implement `ICapability` interface
- Provide clear tool schema via `GetInputSchema()`
- Handle errors gracefully with `CapabilityResult`
- Include XML documentation for capability purpose

**API Controllers:**
- Use minimal APIs or controllers consistently
- Return appropriate HTTP status codes
- Validate input with data annotations
- Use async/await for all I/O operations

**UI Components:**
- Follow Blazor component patterns
- Use MudBlazor components for consistency
- Apply Tailwind CSS utility classes
- Implement `IDisposable` for event subscriptions

## Code Quality and Analyzers

Before submitting a PR, ensure your code passes all quality checks:

### 1. Format Code

Run `dotnet format` to automatically fix formatting issues:

```bash
# Check for formatting issues
dotnet format Aide.slnx --verify-no-changes

# Apply formatting fixes
dotnet format Aide.slnx
```

### 2. Address Roslyn Analyzer Suggestions

The project uses Roslyn analyzers to enforce code quality. Address all analyzer hints, warnings, and errors:

**Common Roslyn Suggestions:**
- Use collection expressions: `[]` instead of `new List<>()`
- Use `System.Threading.Lock` instead of `object` for lock fields (in .NET 10+)
- Mark fields as `readonly` when they're not modified after initialization
- Remove unnecessary using directives
- Simplify LINQ expressions where possible

**Check Diagnostics in IDE:**
- Visual Studio: View > Error List
- VS Code: Problems panel (Ctrl+Shift+M / Cmd+Shift+M)
- Rider: Alt+6 (Problems tool window)

**Build with Code Analysis:**

```bash
# Build with code style enforcement
dotnet build Aide.slnx /p:EnforceCodeStyleInBuild=true

# Should output: 0 Warning(s), 0 Error(s)
```

### 3. Best Practices Checklist

Before submitting, verify your code follows these patterns:

- ✅ Collection expressions: `private readonly List<string> _items = [];`
- ✅ Modern lock type: `private readonly Lock _lock = new();`
- ✅ File-scoped namespaces: `namespace Aide.Core.Services;`
- ✅ Primary constructors for simple classes
- ✅ Required properties for DI: `public required ILogger Logger { get; init; }`
- ✅ XML documentation on public APIs
- ✅ Proper async/await patterns (no `async void` except event handlers)
- ✅ No compiler warnings or analyzer violations

## Testing

- Write unit tests for new functionality
- Use xUnit test framework
- Aim for >80% code coverage on core services
- Include integration tests for API endpoints

```bash
# Run tests
dotnet test

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Building Locally

```bash
# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project src/Aide.Api

# Run UI
dotnet run --project src/Aide.Ui
```

## Configuration

For local development, create `~/.aide/appsettings.json` with your API keys:

```json
{
  "Aide": {
    "Llm": {
      "Providers": {
        "Claude": {
          "ApiKey": "sk-ant-api03-YOUR-KEY-HERE"
        }
      }
    }
  }
}
```

Never commit API keys or secrets to the repository.

## Questions or Issues?

- Open an issue for bug reports or feature requests
- Use GitHub Discussions for questions and general discussion
- Tag maintainers for urgent security issues

## License

By contributing to Aide, you agree that your contributions will be licensed under the project's license (TBD).

---

Thank you for contributing to Aide! 🚀
