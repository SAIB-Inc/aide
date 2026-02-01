using System.Runtime.InteropServices;
using System.Text;
using Aide.Core.Abstractions;

namespace Aide.Capabilities;

/// <summary>
/// Retrieves system information including OS, architecture, memory, and runtime details.
/// Useful for diagnostics and environment verification.
/// </summary>
public class SystemInfoCapability : ICapability
{
    public string Name => "system_info";

    public string Description => "Get information about the system including OS, architecture, memory, and runtime version.";

    public Task<CapabilityResult> ExecuteAsync(CapabilityContext context)
    {
        try
        {
            var info = GetSystemInformation();

            return Task.FromResult(new CapabilityResult
            {
                Success = true,
                Output = info,
                Data = new
                {
                    os = RuntimeInformation.OSDescription,
                    architecture = RuntimeInformation.ProcessArchitecture.ToString(),
                    framework = RuntimeInformation.FrameworkDescription,
                    osArchitecture = RuntimeInformation.OSArchitecture.ToString(),
                    machineName = Environment.MachineName,
                    processorCount = Environment.ProcessorCount,
                    is64Bit = Environment.Is64BitOperatingSystem,
                    dotnetVersion = Environment.Version.ToString(),
                    workingSet = GC.GetTotalMemory(false) / 1024 / 1024 // MB
                }
            });
        }
        catch (Exception ex)
        {
            return Task.FromResult(new CapabilityResult
            {
                Success = false,
                ErrorMessage = $"Failed to retrieve system information: {ex.Message}",
                ErrorCode = "SYSINFO_ERROR"
            });
        }
    }

    public ToolSchema GetInputSchema()
    {
        return new ToolSchema(
            Type: "object",
            Properties: [],
            Required: []
        );
    }

    private static string GetSystemInformation()
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== System Information ===");
        sb.AppendLine();

        // Operating System
        sb.AppendLine($"OS: {RuntimeInformation.OSDescription}");
        sb.AppendLine($"OS Architecture: {RuntimeInformation.OSArchitecture}");
        sb.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine();

        // Machine Info
        sb.AppendLine($"Machine Name: {Environment.MachineName}");
        sb.AppendLine($"Processor Count: {Environment.ProcessorCount}");
        sb.AppendLine($"Process Architecture: {RuntimeInformation.ProcessArchitecture}");
        sb.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
        sb.AppendLine();

        // Runtime Info
        sb.AppendLine($".NET Runtime: {RuntimeInformation.FrameworkDescription}");
        sb.AppendLine($".NET Version: {Environment.Version}");
        sb.AppendLine();

        // Memory Info
        var workingSet = GC.GetTotalMemory(false) / 1024 / 1024;
        sb.AppendLine($"Current Working Set: {workingSet} MB");
        sb.AppendLine();

        // User Info
        sb.AppendLine($"User: {Environment.UserName}");
        sb.AppendLine($"User Domain: {Environment.UserDomainName}");
        sb.AppendLine($"Current Directory: {Environment.CurrentDirectory}");

        return sb.ToString();
    }
}
