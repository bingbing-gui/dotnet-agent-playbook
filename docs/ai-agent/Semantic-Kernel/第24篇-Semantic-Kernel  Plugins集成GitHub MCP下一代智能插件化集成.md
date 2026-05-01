## MCP 简介

MCP（Model Context Protocol，模型上下文协议）是一个开放协议，旨在让 AI 应用能够更轻松地扩展额外能力。通过 MCP，你可以让 AI 无缝调用外部服务或插件，从而突破模型本身的限制。

在 Semantic Kernel 中，你可以直接将 MCP Server 提供的插件接入到 Agent 中，极大提升应用的可扩展性和自动化能力。

目前，Semantic Kernel 支持多种 MCP 插件类型，包括：

- **MCPStdioPlugin**：连接本地 MCP Server
- **MCPStreamableHttpPlugin**：通过 HTTPS + SSE 连接远程 MCP Server

---

## GitHub MCP 配置

以 GitHub MCP 为例，演示如何快速配置和使用：

### Step 1：进入 GitHub 账户 Settings

![进入 GitHub 账户 Settings](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-setting1.png)

### Step 2：选择 Developer Settings

![选择 Developer Settings](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-setting2.png)

### Step 3：选择 Fine-grained tokens

![选择 Fine-grained tokens](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-setting3.png)

### Step 4：生成 MCP Token

![生成 MCP Token](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-setting4.png)

### Step 5：配置仓库和访问权限

![配置仓库和访问权限](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-setting5.png)

### Step 6：复制生成的 Token

![复制生成的 Token](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-setting6.png)

---

## 创建 ASP.NET Core MVC 应用

1. 在 Azure AI Foundry 部署好所需模型（不会操作可参考前文）。
2. 在应用中配置并创建 MCPClient。MCPClient 会从 GitHub MCP Server 拉取一系列函数（Tools），这里使用 MCPStreamableHttpPlugin 方式接入。

```csharp
builder.Services.AddSingleton((serviceProvider) =>
{
    var kernelBuilder = Kernel.CreateBuilder();

    kernelBuilder.Services
        .AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-5-mini",
            apiKey: "",
            endpoint: "https://rg-mcp.cognitiveservices.azure.com/");

    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var github_mcp_key = "your_github_pat_token";

    if (!string.IsNullOrWhiteSpace(github_mcp_key))
    {
        var mcpClient = McpClientFactory.CreateAsync(new SseClientTransport(new()
        {
            Name = "GitHub",
            Endpoint = new Uri("https://api.githubcopilot.com/mcp/"),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {github_mcp_key}"
            }
        })).GetAwaiter().GetResult();

        var tools = mcpClient.ListToolsAsync().GetAwaiter().GetResult();
        kernelBuilder.Plugins.AddFromFunctions("GithubMCP", tools.Select(skf => skf.AsKernelFunction()));
    }

    return kernelBuilder.Build();
});
```

---

## 封装 McpToolService

为了更好地使用，创建 `McpToolService` 封装函数列表查询和对话调用：

**接口定义：**

```csharp
// Services/IMcpToolService.cs
public interface IMcpToolService
{
    Task<IEnumerable<string>> ListToolNamesAsync(CancellationToken ct = default);
    IAsyncEnumerable<StreamingKernelContent> AskWithAutoToolsAsync(string prompt, CancellationToken ct = default);
}
```

**实现：**

```csharp
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
```

---

## 控制器：Chat2BugController

创建 `Chat2BugController`，用于接收用户输入并调用 GitHub MCP 自动提交 Issue：

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SK.MCP.Plugins.Services;
using System.Text;
using System.Text.Json;

namespace SK.MCP.Plugins.Controllers
{
    public class Chat2BugController : Controller
    {
        private readonly IMcpToolService _mcpTools;
        private readonly Kernel _kernel;

        public Chat2BugController(IMcpToolService mcpTools, Kernel kernel)
        {
            _mcpTools = mcpTools;
            _kernel = kernel;
        }

        // GET /chat2bug/index
        [HttpGet("/chat2bug/index")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var names = await _mcpTools.ListToolNamesAsync(ct);
            return View("Index", names);
        }

        // POST /chat2bug/ask  （模型可自动选择工具）
        [HttpPost("/chat2bug/ask")]
        public async Task Ask([FromBody] string prompt, CancellationToken ct)
        {
            HttpResponse response = HttpContext.Response;
            response.ContentType = "text/event-stream";
            response.Headers.Add("Cache-Control", "no-cache");
            response.Headers.Add("Connection", "keep-alive");

            await response.StartAsync();

            try
            {
                await foreach (var chunk in _mcpTools.AskWithAutoToolsAsync(prompt, ct))
                {
                    byte[] messageBytes = Encoding.UTF8.GetBytes(chunk.ToString());
                    await response.Body.WriteAsync(messageBytes, 0, messageBytes.Length);
                    await response.Body.FlushAsync();
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"data: {JsonSerializer.Serialize(new { error = "Error processing the request", details = ex.Message })}\n\n";
                await response.WriteAsync(errorMessage);
            }
            finally
            {
                await response.CompleteAsync();
            }
        }
    }
}
```

> 具体 View 层代码请查看 GitHub 仓库，因篇幅限制未贴出。

---

## 实际演示

启动应用后，可以看到 GitHub MCP 工具函数已成功加载：

![github mcp tools](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-tools.png)

输入消息：

> 帮我在 bingbing-gui/aspnetcore-developer 仓库下创建一个 Issue，描述为：点击请求按钮时应用程序崩溃

AI 会自动调用 GitHub MCP 工具，为你在仓库里创建 Issue：

![github mcp tools](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-issue.png)

最终在 GitHub 中可以看到效果：

![github mcp tools](/aspnetcore-developer/docs/sk-agent-framework/Materials/github-mcp-issue-01.png)

---

## 示例项目源码

查看完整的示例项目与代码实现：

[https://github.com/bingbing-gui/aspnetcore-developer/tree/master/src/09-AI-Agent/SemanticKernel/SK.MCP.Plugins](https://github.com/bingbing-gui/aspnetcore-developer/tree/master/src/09-AI-Agent/SemanticKernel/SK.MCP.Plugins)



## 总结

通过 Semantic Kernel × GitHub MCP 的结合，让 AI 真正参与到研发工作流中：

- 从对话到工具调用
- 从提示到自动化操作
- 从 Bug 描述到 GitHub Issue

这一整套流程，让 AI 辅助研发不再只是“对话”，而是可以直接驱动开发工具链。
未来，AI + MCP + Semantic Kernel 将是软件研发提效的重要方向。
