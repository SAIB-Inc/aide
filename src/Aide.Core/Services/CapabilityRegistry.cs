using Aide.Core.Abstractions;

namespace Aide.Core.Services;

/// <summary>
/// In-memory registry for managing capabilities/skills available to the AI agent.
/// Provides capability lookup and conversion to LLM tool definitions.
/// </summary>
public class CapabilityRegistry
{
    private readonly Dictionary<string, ICapability> _capabilities = [];
    private readonly Lock _lock = new();

    /// <summary>
    /// Register a capability with the registry
    /// </summary>
    /// <param name="capability">The capability to register</param>
    /// <exception cref="ArgumentNullException">Thrown when capability is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when a capability with the same name is already registered</exception>
    public void Register(ICapability capability)
    {
        ArgumentNullException.ThrowIfNull(capability);

        lock (_lock)
        {
            if (_capabilities.ContainsKey(capability.Name))
            {
                throw new InvalidOperationException(
                    $"A capability with name '{capability.Name}' is already registered.");
            }

            _capabilities[capability.Name] = capability;
        }
    }

    /// <summary>
    /// Register multiple capabilities at once
    /// </summary>
    /// <param name="capabilities">Collection of capabilities to register</param>
    public void RegisterRange(IEnumerable<ICapability> capabilities)
    {
        ArgumentNullException.ThrowIfNull(capabilities);

        foreach (var capability in capabilities)
        {
            Register(capability);
        }
    }

    /// <summary>
    /// Get a capability by name
    /// </summary>
    /// <param name="name">Name of the capability</param>
    /// <returns>The capability instance</returns>
    /// <exception cref="KeyNotFoundException">Thrown when capability is not found</exception>
    public ICapability Get(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        lock (_lock)
        {
            if (!_capabilities.TryGetValue(name, out var capability))
            {
                throw new KeyNotFoundException(
                    $"No capability with name '{name}' is registered.");
            }

            return capability;
        }
    }

    /// <summary>
    /// Try to get a capability by name
    /// </summary>
    /// <param name="name">Name of the capability</param>
    /// <param name="capability">The capability if found, null otherwise</param>
    /// <returns>True if capability was found, false otherwise</returns>
    public bool TryGet(string name, out ICapability? capability)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            capability = null;
            return false;
        }

        lock (_lock)
        {
            return _capabilities.TryGetValue(name, out capability);
        }
    }

    /// <summary>
    /// Get all registered capabilities
    /// </summary>
    /// <returns>Collection of all registered capabilities</returns>
    public IReadOnlyCollection<ICapability> GetAll()
    {
        lock (_lock)
        {
            return _capabilities.Values.ToList().AsReadOnly();
        }
    }

    /// <summary>
    /// Convert all registered capabilities to LLM tool definitions
    /// </summary>
    /// <returns>List of tool definitions for LLM use</returns>
    public List<ToolDefinition> ToToolDefinitions()
    {
        lock (_lock)
        {
            return
            [
                .._capabilities.Values.Select(capability => new ToolDefinition
                {
                    Name = capability.Name,
                    Description = capability.Description,
                    InputSchema = capability.GetInputSchema()
                })
            ];
        }
    }

    /// <summary>
    /// Check if a capability is registered
    /// </summary>
    /// <param name="name">Name of the capability</param>
    /// <returns>True if registered, false otherwise</returns>
    public bool IsRegistered(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        lock (_lock)
        {
            return _capabilities.ContainsKey(name);
        }
    }

    /// <summary>
    /// Unregister a capability
    /// </summary>
    /// <param name="name">Name of the capability to remove</param>
    /// <returns>True if capability was removed, false if it wasn't registered</returns>
    public bool Unregister(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        lock (_lock)
        {
            return _capabilities.Remove(name);
        }
    }

    /// <summary>
    /// Clear all registered capabilities
    /// </summary>
    public void Clear()
    {
        lock (_lock)
        {
            _capabilities.Clear();
        }
    }

    /// <summary>
    /// Get the count of registered capabilities
    /// </summary>
    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _capabilities.Count;
            }
        }
    }
}
