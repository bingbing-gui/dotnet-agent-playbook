
在构建智能 Agent 的过程中，我们不希望它只是一个“陪聊机器人”。我们更希望它能够获取实时天气、查询数据库、调用企业内部 API，真正融入业务系统。

Microsoft Agent Framework 提供了 Plugins（插件）机制，用于将业务能力封装为 AI 可调用的工具（Tools），并且与 .NET 的 Dependency Injection（依赖注入，DI）体系深度融合，使 Agent 的能力扩展具备良好的工程可维护性。

## 为什么要使用 Plugins？

大语言模型（LLM）的能力受限于训练数据，无法直接访问实时信息或企业内部系统。要让 Agent 具备这些能力，必须为它提供可调用的外部工具（Tools）。

在 Microsoft Agent Framework 中：
- Tool（AITool）：AI 实际可调用的函数单元
- Plugin：一种工程化封装方式，负责
    - 组织一组相关 Tool
    - 承载依赖注入（DI）
    - 实现业务逻辑与 AI 行为的解耦

需要注意的是，Plugin 并不是 AI 能力本身。AI 实际调用的是 Tool，而 Plugin 的价值在于：用熟悉的 .NET 工程模式，把 AI 能力“接入”现有系统。

## 代码核心解析

下面通过一个简化示例，解析 Program.cs 中的关键设计点。

```csharp

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

ServiceCollection services = new();
services.AddSingleton<WeatherProvider>();
services.AddSingleton<CurrentTimeProvider>();
services.AddSingleton<AgentPlugin>();

IServiceProvider serviceProvider = services.BuildServiceProvider();

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "你是一个乐于助人的助手，帮助人们查找信息。",
        name: "Assistant",
        tools: [.. serviceProvider.GetRequiredService<AgentPlugin>().AsAITools()],
        services: serviceProvider);

Console.WriteLine(await agent.RunAsync("告诉我西雅图的当前时间和天气。"));


internal sealed class AgentPlugin(WeatherProvider weatherProvider)
{
    public string GetWeather(string location)
    {
        return weatherProvider.GetWeather(location);
    }

    public DateTimeOffset GetCurrentTime(IServiceProvider sp, string location)
    {
        var currentTimeProvider = sp.GetRequiredService<CurrentTimeProvider>();

        return currentTimeProvider.GetCurrentTime(location);
    }

    public IEnumerable<AITool> AsAITools()
    {
        yield return AIFunctionFactory.Create(this.GetWeather);
        yield return AIFunctionFactory.Create(this.GetCurrentTime);
    }
}

internal sealed class WeatherProvider
{
    public string GetWeather(string location)
    {
        return $"{location}天气是多云高于15°C.";
    }
}

internal sealed class CurrentTimeProvider
{
    public DateTimeOffset GetCurrentTime(string location)
    {
        return DateTimeOffset.Now;
    }
}

```

### 1. 依赖服务的注册

在企业级 .NET 应用中，我们通常使用依赖注入管理服务生命周期。示例中通过 ServiceCollection 注册三个组件：

```csharp
ServiceCollection services = new();
services.AddSingleton<WeatherProvider>();      // 提供天气服务
services.AddSingleton<CurrentTimeProvider>();  // 提供时间服务
services.AddSingleton<AgentPlugin>();          // 插件逻辑
```

这样做的好处是：
- Plugin 不直接依赖具体实现
- 服务可以被统一替换、测试或扩展
- 与 ASP.NET Core 的 DI 模式保持一致

示例中将 AgentPlugin 注册为 Singleton，前提是插件本身不维护请求级状态。若插件依赖用户上下文或会话状态，应考虑使用 Scoped 生命周期。

### 2. 插件类的实现（AgentPlugin）

AgentPlugin 是连接 AI 与业务逻辑的桥梁。下面展示两种常见的依赖使用方式。

- 方式一：构造函数注入（推荐）

```csharp
internal sealed class AgentPlugin(WeatherProvider weatherProvider)
{
        public string GetWeather(string location)
        {
                return weatherProvider.GetWeather(location);
        }
}
```

优点：
- 依赖关系清晰
- 易于单元测试
- 符合 .NET 领域的最佳实践

- 方式二：方法参数中的服务定位

```csharp
public DateTimeOffset GetCurrentTime(IServiceProvider sp, string location)
{
        var currentTimeProvider = sp.GetRequiredService<CurrentTimeProvider>();
        return currentTimeProvider.GetCurrentTime(location);
}
```

在工具方法被调用时，框架会自动注入 IServiceProvider，允许在方法内部按需解析服务。

注意：这种方式属于 Service Locator 模式，仅适用于依赖高度动态的场景。在大多数业务代码中，仍应优先使用构造函数注入，以保持可维护性和可测试性。

### 3. 将方法显式暴露为 AI 工具

并不是 Plugin 中的所有方法都会被 AI 感知。只有通过 AIFunctionFactory.Create 显式注册的方法，才会作为 Tool 暴露给模型。

```csharp
public IEnumerable<AITool> AsAITools()
{
        yield return AIFunctionFactory.Create(this.GetWeather);
        yield return AIFunctionFactory.Create(this.GetCurrentTime);
}
```

好处：
- 精准控制 AI 可调用能力
- 避免无意中暴露内部方法
- 更符合安全与合规要求

### 4. 组装并运行 Agent

最后，将工具集与服务容器一并装配到 Agent 中：

```csharp
AIAgent agent = new AzureOpenAIClient(/* credentials */)
        .GetChatClient(deploymentName)
        .CreateAIAgent(
                instructions: "你是一个乐于助人的助手。",
                name: "Assistant",
                // 注入工具集
                tools: [.. serviceProvider.GetRequiredService<AgentPlugin>().AsAITools()],
                // 注入服务容器
                services: serviceProvider);

var response = await agent.RespondAsync(
        "告诉我西雅图当前的时间和天气。");
Console.WriteLine(response);
```

当用户提出问题时：
- Agent 根据模型判断是否以及如何调用 Tool
- 框架执行对应的 C# 方法，并通过 DI 获取依赖
- Agent 汇总结果，生成自然语言回答

在实际生产环境中，是否调用、调用顺序以及参数选择，均由模型决策，不应假设其行为是完全确定的。


## 扩展：Function Call、MCP Tools 与 Plugins 的关系

在 AI Agent 体系中，Function Call、MCP Tools、Plugins 并不是三种并列的能力，而是处在不同层级、解决不同问题的机制。

### 1️⃣ Function Call 是什么？（它为什么出现）

Function Call 的出现，是为了解决一个核心问题：如何让大语言模型稳定地产出“机器可执行的调用意图与参数”。

它本质上是一种用于表达工具调用意图的模型级协议，规定了：
- 调用哪个函数
- 参数以什么结构（JSON Schema）输出
- 由模型决定是否触发调用

Function Call 只负责“意图与参数的结构化表达”，它不关心也不规定：
- 函数是否在当前进程
- 是否远程调用
- 依赖注入、生命周期、工程结构

因此，Function Call 既可以用于进程内，也可以用于进程外，
但跨进程能力并非其协议职责，执行方式完全由宿主系统决定。

### 2️⃣ MCP Tools 是什么？（它为什么需要）

在真实系统中，仅有 Function Call 还不够。当工具数量增多、系统变复杂时，会遇到新的问题：
- 工具如何被统一发现
- 如何跨进程、跨语言复用
- 如何集中做权限、治理与边界控制

MCP（Model Context Protocol）为这些问题提供了解决方案：
- 提供标准化的工具发布与调用协议
- Tool 通过 MCP Server 对外暴露
- Agent / 模型可动态发现并调用
- 天然支持跨进程、跨系统

这些能力并未继续叠加到 Function Call 中，是因为它们已超出“模型输出语义”的职责边界。

### 3️⃣ Plugins 在其中扮演什么角色？

在 Microsoft Agent Framework 中，Plugin 是宿主框架提供的工程层组织方式，而不是调用协议。它解决的是：在 .NET 应用中，如何优雅地管理和暴露一组可被 AI 调用的能力。

Plugin 的职责包括：
- 聚合多个 Tool
- 承载依赖注入（DI）
- 明确哪些方法可以暴露给 AI
- 屏蔽业务实现细节

`AIFunctionFactory.Create(...)` 的本质，是把一个 C# 方法包装成可被 Function Call 调用的 Tool。

### 4️⃣ 一句话总结

- Function Call：模型层，解决“如何表达调用意图与参数”
- MCP Tools：接入层，解决“工具如何标准化对外发布与被发现”
- Plugins：工程层，解决“.NET 中如何组织、注入和暴露这些能力”


## 总结与启示

- 模块化设计：通过 Plugins 将业务逻辑与 AI 行为解耦
- 拥抱 .NET 生态：天然兼容 Microsoft.Extensions.DependencyInjection
- 能力可控暴露：显式注册 Tool，提升安全性与可维护性

从架构角度看，Plugin + DI 本质上是将 ASP.NET Core 的分层与依赖管理思想，引入到 AI Agent 体系中。这使得 AI 不再是一个孤立的黑盒，而是企业应用架构中的一等公民。

在 .NET 中构建可演进的 AI 应用，善用 Plugins 与依赖注入，是一条可靠且工程化的路径。
