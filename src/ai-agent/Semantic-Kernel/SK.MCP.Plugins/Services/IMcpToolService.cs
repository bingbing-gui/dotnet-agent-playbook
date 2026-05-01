namespace SK.MCP.Plugins.Services
{
    // Services/IMcpToolService.cs
    using Microsoft.SemanticKernel;
        
    public interface IMcpToolService
    {
        Task<IEnumerable<string>> ListToolNamesAsync(CancellationToken ct = default);
        IAsyncEnumerable<StreamingKernelContent> AskWithAutoToolsAsync(string prompt, CancellationToken ct = default);
    }

}
