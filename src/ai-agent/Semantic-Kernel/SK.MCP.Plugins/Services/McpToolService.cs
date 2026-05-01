using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using ModelContextProtocol.Client;

namespace SK.MCP.Plugins.Services
{
    public class McpToolService : IMcpToolService
    {
        private readonly Kernel _kernel;
        public McpToolService(Kernel kernel)
        {
            _kernel = kernel;
        }

        public async Task<IEnumerable<string>> ListToolNamesAsync(CancellationToken ct = default)
        {
            var functionNames = _kernel.Plugins["GitHubMCP"]
                 .AsAIFunctions()
                 .Select(fn => fn.Name)
                 .ToList();
            return functionNames;
        }

        public async IAsyncEnumerable<StreamingKernelContent> AskWithAutoToolsAsync(string prompt, CancellationToken ct = default)
        {

            var exec = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            var args = new KernelArguments(exec);

            await foreach (var chunk in _kernel.InvokePromptStreamingAsync($"用户：{prompt}", arguments: args, cancellationToken: ct))
            {
                yield return chunk;
            }
        }
    }
}
