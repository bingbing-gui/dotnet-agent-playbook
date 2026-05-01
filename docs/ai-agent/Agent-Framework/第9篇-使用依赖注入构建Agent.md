
在构建现代 .NET 应用程序时，依赖注入（Dependency Injection, DI）是一种核心设计模式，有助于实现解耦、可测试性和模块化。Microsoft Agent Framework 原生支持这一模式，使得将 AI Agent 集成到现有 .NET 生态变得简单。

在这个例子中，深度解析如何通过 .NET 通用主机 (Generic Host) 将 AI Agent 注册为服务，并在后台服务中优雅地使用它。

## 示例概览

该示例展示了一个名为 “Joker” 的 AI Agent，用来讲发生在茶馆里的 笑话。核心演示：

- 如何配置 HostApplicationBuilder
- 如何将 AIAgent 及其依赖项注册到 DI 容器中
- 如何在 IHostedService 中通过构造函数注入使用 Agent

## 代码深度解析

### 环境准备与主机构建

首先从环境变量中获取 Azure OpenAI 配置，并创建 Host 构建器。

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-4o-mini";
// 创建 Host 构建器，这是 .NET 应用依赖注入的基础
HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);
```

### 核心：依赖注入注册

拆分并注册 Agent 的各个组件。

#### 注册 Agent 选项

将 Agent 配置（如名称、角色指令）注册为单例，做到配置与逻辑分离。

```csharp
builder.Services.AddSingleton(new ChatClientAgentOptions
{
    Name = "Joker",
    ChatOptions = new() { Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。" }
});
```

#### 注册 Chat Client（使用 Keyed Service）

- 使用 .NET 的 Keyed Services（AddKeyedChatClient）
- 便于区分多个模型或后端（通过不同的 Key）
- 使用 AzureCliCredential 无密钥认证

```csharp
builder.Services.AddKeyedChatClient("AzureOpenAI", sp => new AzureOpenAIClient(
        new Uri(endpoint),
        new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient());
```

#### 注册 AI Agent

- 从容器获取 Keyed Service（IChatClient）
- 获取配置选项（ChatClientAgentOptions）
- 组装并返回 ChatClientAgent

```csharp
builder.Services.AddSingleton<AIAgent>(sp => new ChatClientAgent(
    chatClient: sp.GetRequiredKeyedService<IChatClient>("AzureOpenAI"),
    options: sp.GetRequiredService<ChatClientAgentOptions>()));
```

### 消费服务：构建 Hosted Service

注册后台服务并运行主机。

```csharp
builder.Services.AddHostedService<SampleService>();

using IHost host = builder.Build();
await host.RunAsync().ConfigureAwait(false);
```

### 服务实现细节（SampleService）

#### 构造函数注入

利用主构造函数语法声明对 AIAgent 的依赖，启动时由 DI 自动注入 “Joker”。

```csharp
internal sealed class SampleService(AIAgent agent, IHostApplicationLifetime appLifetime) : IHostedService
{
    private AgentThread? _thread;
    // ...
}
```

#### 生命周期管理与线程

在 StartAsync 中创建 AgentThread，用于保存会话上下文。

```csharp
public async Task StartAsync(CancellationToken cancellationToken)
{
    this._thread = agent.GetNewThread();
    _ = this.RunAsync(appLifetime.ApplicationStopping);
}
```

- AgentThread 表示对话上下文（历史）
- 服务存活期间对话记忆保持

#### 交互循环与流式输出

进入循环读取输入，并以流式方式输出模型回复。

```csharp
// 核心交互逻辑
await foreach (var update in agent.RunStreamingAsync(input, this._thread, cancellationToken: cancellationToken))
{
    Console.Write(update);
}
```

- RunStreamingAsync 接收用户输入和上下文，逐字流式返回结果

## 关键知识点总结

- 配置与逻辑分离：使用 ChatClientAgentOptions 解耦提示词与代码
- 模块化设计：KeyedChatClient 管理多个 AI 后端（如 Azure OpenAI、Ollama）
- 可测试性：依赖抽象 AIAgent，单测可替换为 Mock
- 标准范式：基于 Microsoft.Extensions.Hosting，适用于控制台、ASP.NET Core、Worker 等