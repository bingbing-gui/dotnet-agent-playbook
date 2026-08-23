Model Context Protocol 是一个开放标准，它定义了应用程序如何向大型语言模型（LLMs）提供工具和上下文数据。它使外部工具能够以一致且可扩展的方式集成到模型工作流中。
Agent Framework 支持与 Model Context Protocol（MCP）服务器集成，使你的 agents 能够访问外部工具和服务。



## Agent Framework 中的 MCP 传输方式

我们在前面的文章中介在Agent Framework中的使用标准输入/输出方式调用MCP，文章如下：https://mp.weixin.qq.com/s/g2NP1EOGqKCcN9XIPhl51w。

这一节我们介绍另外的一种方式，基于HTTP的调用方式，补齐Agent Framework中 MCP 的两种通信/传输方式（Transport）。

首先我们来介绍一下这两个类

`StdioClientTransport` 类

提供一个通过`stdio`（标准输入/输出）实现的 `IClientTransport` 接口。该传输机制会启动一个外部进程，并通过标准输入和输出流与其进行通信。它用于连接在子进程中启动并托管的 MCP 服务器。

`HttpClientTransport` 类 

提供一种基于 HTTP 的 `IClientTransport`实现，使用 Server-Sent Events (SSE) 或流式 HTTP 协议。该传输方式通过 SSE 或流式 HTTP 连接到 MCP 服务器，从而利用标准 HTTP 请求实现服务器到客户端的实时通信。
与`StdioClientTransport`不同，该传输方式连接到现有的服务器，而不是启动新进程。


这两个类都继承自 `IClientTransport` 接口，提供了与 MCP 服务器通信的能力。

`IClientTransport` 定义如下

```csharp
public interface IClientTransport
{
    string Name { get; }

    Task<ITransport> ConnectAsync(CancellationToken cancellationToken = default);
}
```

## 使用 Agent Framework 调用 MCP 工具

创建Console 应用程序，添加以下 NuGet 包引用：

1. `Azure.Identity`
2. `Microsoft.Agents.AI.Foundry`
3. `ModelContextProtocol`


```

我们示例中使用ms learn中提供的MCP Server，地址如下：
`https://learn.microsoft.com/api/mcp`

首先我们创建`McpClient`用来获取`https://learn.microsoft.com/api/mcp`提供的工具，并把这些工具转换为本地可用的`AITool`，Agent Framework中提供了`McpClient`类来实现这一功能，整个过程非常简单。

```csharp
await using McpClient mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new()
{
    Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
    Name = "Microsoft Learn MCP",
}));

IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
Console.WriteLine($"可用的 MCP 工具：{string.Join(", ", mcpTools.Select(t => t.Name))}");

List<AITool> wrappedTools = mcpTools.Select(tool => (AITool)new LoggingMcpTool(tool)).ToList();
```

然后把这些工具赋值给Agent Framework中的`AIAgent`，就可以使用这些工具了，整个过程非常简单。

```csharp
ar endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("未设置 FOUNDRY_PROJECT_ENDPOINT。");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: agentInstructions,
    name: agentName,
    tools: wrappedTools);

Console.WriteLine($"智能体“{agent.Name}”已成功创建。");

var prompt1 = "如何使用 Azure CLI 创建 Azure 存储帐户？";
Console.WriteLine($"\n用户：{prompt1}\n");
AgentResponse response1 = await agent.RunAsync(prompt1);
Console.WriteLine($"智能体：{response1}");

Console.WriteLine("\n=======================================\n");


var prompt2 = "什么是 Microsoft Agent Framework？";
Console.WriteLine($"用户：{prompt2}\n");
AgentResponse response2 = await agent.RunAsync(prompt2);
Console.WriteLine($"智能体：{response2}");
```

运行效果如下：

图篇


## 总结

我们基本覆盖了MCP的两种调用方式：
1. 基于标准输入/输出的调用方式，使用`StdioClientTransport`类
2. 基于HTTP的调用方式，使用`HttpClientTransport`类

这部分只是我们作为调用方去调用第三方系统提供的MCP Server，我们还可以作为被调用方，其实Agent Framework 本身提供了MCP Server的能力，开发者可以在自己的系统中实现MCP Server，这部分我们会在后续的文章中介绍。
