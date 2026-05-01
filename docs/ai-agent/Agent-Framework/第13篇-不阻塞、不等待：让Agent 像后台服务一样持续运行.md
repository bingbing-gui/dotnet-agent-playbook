## 新年寄语

新的一年，先向正在阅读这篇文章的你，说一声：新年好。

过去一年里，我们见证了 GenAI 技术的飞速变化，也在真实的项目和代码中反复碰壁、反复推翻、反复重来。很多想法不是一开始就想清楚的，而是在一次次实践中逐渐变得清晰。

新的一年，我依然希望通过这个公众号，分享一些来自一线实践的思考：
- 不追热点
- 不讲概念
- 尽量把问题说清楚，把原理讲明白，把代码写实在

如果这些内容，能在你某一次项目选型、架构设计，或者调试深夜的报错时，带来一点参考或共鸣，那就已经足够了。

愿新的一年，我们都能：
- 少一点焦虑，多一点笃定
- 少一点“跟风”，多一点属于自己的判断

---

## 序言

在开发 GenAI 应用时，我们经常会遇到一个很现实、也很尴尬的场景。用户发来一个复杂指令，比如：

- “写一本关于火星殖民的长篇小说”
- “分析这 50 份 PDF 文档，给我总结结论”

然后前端就开始 loading。 如果这个任务要跑一两分钟，普通的 HTTP 请求基本已经超时，用户也很可能已经关掉页面走人了。

这个问题在 Demo 里并不明显，但一旦进入真实业务场景，几乎是绕不开的。

---

## 1. 核心痛点：无状态 Web vs 长任务 AI

Web 服务通常是无状态的。  
如果 AI 正在写小说写到一半，这时服务重启，或者遇到其他不可抗拒的因素，上下文就会直接丢失。

同样地，如果 AI 正在执行任务的过程中，客户端断开连接，当前的执行状态也无法继续保留。

而 GenAI 恰恰最常见的需求是：

> 一次任务，持续很久。

这就和无状态 Web 的执行模型产生了天然的冲突。

那么，有没有一种方法，能够在不依赖长连接的情况下，维持 AI 任务的运行状态？  
答案是有的，我们继续往下看。

---

## 2. 开启“后台模式”

在开始之前我们仍然需要引入如下包：

```bash
dotnet add package Azure.AI.OpenAI --version 2.8.0-beta.1
dotnet add package Azure.Identity --version 1.17.1
dotnet add package Microsoft.Agents.AI.Hosting.OpenAI --version 1.0.0-alpha.251219.1
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.251219.1
```

针对这种场景，Agent Framework 提供了相对便捷的处理方式。

在初始化 Agent 运行时，这里需要稍微注意一点：  
我们使用的是 `GetResponsesClient()` 方法（后面会单独解释），同时需要将 `AllowBackgroundResponses` 设置为 `true`，以允许 Agent 在后台运行。

```csharp
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetResponsesClient(deploymentName)
     .CreateAIAgent(
        name: "SpaceNovelWriter",
        instructions: @"你是一名太空题材小说作家。
在写作之前，始终先研究相关的真实背景资料，并为主要角色生成角色设定。
写作时直接完成完整章节，不要请求批准或反馈。
不要向用户询问语气、风格、节奏或格式偏好——只需根据请求直接创作小说。",
        tools: [
            AIFunctionFactory.Create(ResearchSpaceFactsAsync),
            AIFunctionFactory.Create(GenerateCharacterProfilesAsync)
        ]);

// 允许 Agent 在后台运行
AgentRunOptions options = new()
{
    AllowBackgroundResponses = true
};
```

## 3. 轮询与“存盘”（The Loop）

这是整个 Demo 中最关键的部分。

它不再是简单地 `await agent.RunAsync()` 然后一直等待结果，而是通过一个循环，把一次长任务拆解成多次可恢复的执行过程：

```csharp
// 发起任务
AgentRunResponse response = await agent.RunAsync("写一本超长的太空小说...", thread, options);

// 只要还有 ContinuationToken，说明任务没结束
while (response.ContinuationToken is not null)
{
    // 1. 存盘：把当前线程状态和令牌存起来（比如存到数据库或 Redis）
    PersistAgentState(thread, response.ContinuationToken);

    // 2. 休息：这里模拟客户端断开连接，或者 Serverless 函数释放资源
    await Task.Delay(TimeSpan.FromSeconds(10));

    // 3. 读盘：重新恢复 Agent 状态
    RestoreAgentState(agent, out thread, out ResponseContinuationToken? continuationToken);

    // 4. 继续：带着令牌去问 AI "你写完了吗？"
    options.ContinuationToken = continuationToken;
    response = await agent.RunAsync(thread, options); // 继续运行
}
```

## 4. 状态持久化 (Persistence)

注意看 `PersistAgentState` 和 `RestoreAgentState`。在这个 Demo 里它用了一个 Dictionary 模拟数据库，这就把一个长连接任务，拆成了多次极短的无状态请求。

## 5. 后台运行下的工具调用

即使 Agent 在后台运行，依然可以正常触发工具调用。

在这个 Demo 中，Agent 在写小说之前，会自动调用：

- `ResearchSpaceFactsAsync`（查资料）
- `GenerateCharacterProfilesAsync`（生成角色设定）

这些操作本身可能就比较耗时（示例中模拟了 10 秒延迟）。  
但由于我们引入了“存盘 / 读盘”机制，即使中途网络断开，Agent 在恢复之后，依然能够记得自己已经完成了哪些步骤，而不需要从头再来。

## 6. 请求链路变化

细心的朋友可能发现我们这里创建 Agent 的方式和之前不太一样。

```csharp
GetResponsesClient(deploymentName).CreateAIAgent(...)
```

而不是

```csharp
GetChatClient(deploymentName).CreateAIAgent(...)
```

那么这是为什么呢? 我们就抛开迷雾，看本质！

## 7. microsoft/agent-framework框架中OpenAI 集成

microsoft/agent-framework 框架允许你通过兼容 OpenAI 协议的 HTTP 接口来暴露 AI Agent，同时支持 Chat Completions API 和 Responses API。  
这使你可以将你的 Agent 与任何兼容 OpenAI 协议的客户端或工具进行集成。

## 8. 什么是 OpenAI 协议（OpenAI Protocols）？

microsoft/agent-framework 支持两种 OpenAI 协议：

- Chat Completions API：标准的、无状态的请求 / 响应格式，用于聊天交互
- Responses API：更高级的格式，支持对话管理、流式输出以及长时间运行的 Agent 过程

### 什么时候使用哪种协议？

根据 OpenAI 官方文档，Responses API 已成为默认且推荐的方式。  
它提供了更完整、功能更丰富的接口，适合构建现代 AI 应用，内置：

- 会话管理
- 流式输出
- 长时间运行任务支持

### 使用 Responses API 的场景（推荐）

- 构建新应用（默认推荐）
- 需要服务端对话管理
- 但不是强制的：Responses API 也可以以无状态方式使用
- 需要持久化的对话历史
- 构建长时间运行的 Agent
- 需要更高级的流式能力（包含详细事件类型）
- 需要跟踪和管理单个 Response
  - 例如：通过 ID 获取某次响应、检查状态、取消正在运行的响应

### 使用 Chat Completions API 的场景

- 迁移依赖 Chat Completions 格式的旧系统
- 只需要简单、无状态的请求 / 响应
- 状态管理完全由客户端负责
- 集成只支持 Chat Completions 的现有工具
- 需要最大程度兼容遗留系统

### Chat Completions API

Chat Completions API 提供了一个简单、无状态的接口，使用标准 OpenAI Chat 格式与 Agent 交互。

## 9. 框架背后是如何处理的

### Responses API 调用链

下面是部分源代码，我们调用了 `GetResponsesClient` 方法来实际返回 `ResponsesClient`。

下面代码位于 Azure.AI.OpenAI 包中的 `AzureOpenAIClient.cs` 类中，例子中我们只拿出了一个方法，其它方法省略：

```csharp
public partial class AzureOpenAIClient : OpenAIClient
{
    public override ResponsesClient GetResponsesClient(string deploymentName)
    {
        Argument.AssertNotNullOrEmpty(deploymentName, nameof(deploymentName));
        return new AzureResponsesClient(Pipeline, deploymentName, _endpoint, _options);
    }
}
// 而 AzureOpenAIClient 的父类 OpenAIClient 类位于 OpenAI NuGet 包中
```

下面代码是 `AzureResponsesClient` 的源码，位于 Azure.AI.OpenAI NuGet 包中的 `AzureResponsesClient.cs` 类中：

```csharp
internal partial class AzureResponsesClient : ResponsesClient
{
    private readonly Uri _aoaiEndpoint;
    private readonly string _deploymentName;
    private readonly string _apiVersion;

    internal AzureResponsesClient(
        ClientPipeline pipeline,
        string deploymentName,
        Uri endpoint,
        AzureOpenAIClientOptions options)
        : base(
            pipeline,
            model: deploymentName,
            new OpenAIClientOptions() { Endpoint = endpoint })
    {
        Argument.AssertNotNull(pipeline, nameof(pipeline));
        Argument.AssertNotNull(endpoint, nameof(endpoint));
        options ??= new();

        _aoaiEndpoint = endpoint;
        _deploymentName = deploymentName;
        _apiVersion = options.GetRawServiceApiValueForClient(this);
    }
}
// 而 ResponsesClient 类位于 OpenAI NuGet 包中的 ResponsesClient.cs 类中
```

接下来我们继续看 `OpenAIResponseClientExtensions.cs` 类。  
该类是 `ResponsesClient` 的扩展类，内部定义了两个重载的 `CreateAIAgent` 方法，其中包含 `client.AsIChatClient()` 方法。该方法返回一个 `IChatClient` 接口，`OpenAIResponsesChatClient` 实现了该接口。

```csharp
/// OpenAIResponseClientExtensions 源码
public static class OpenAIResponseClientExtensions
{
    public static ChatClientAgent CreateAIAgent(
        this ResponsesClient client,
        string? instructions = null,
        string? name = null,
        string? description = null,
        IList<AITool>? tools = null,
        Func<IChatClient, IChatClient>? clientFactory = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)
    {
        Throw.IfNull(client);

        return client.CreateAIAgent(
            new ChatClientAgentOptions()
            {
                Name = name,
                Description = description,
                ChatOptions =
                    tools is null && string.IsNullOrWhiteSpace(instructions)
                        ? null
                        : new ChatOptions()
                        {
                            Instructions = instructions,
                            Tools = tools,
                        }
            },
            clientFactory,
            loggerFactory,
            services);
    }

    public static ChatClientAgent CreateAIAgent(
        this ResponsesClient client,
        ChatClientAgentOptions options,
        Func<IChatClient, IChatClient>? clientFactory = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)
    {
        Throw.IfNull(client);
        Throw.IfNull(options);

        var chatClient = client.AsIChatClient();

        if (clientFactory is not null)
        {
            chatClient = clientFactory(chatClient);
        }

        return new ChatClientAgent(chatClient, options, loggerFactory, services);
    }
}
```

下面代码是 Microsoft.Extensions.AI NuGet 包中  
`OpenAIClientExtensions.cs` 类的 `AsIChatClient` 方法源码：

```csharp
public static IChatClient AsIChatClient(
    this ResponsesClient responseClient) =>
    new OpenAIResponsesChatClient(responseClient);
```

下面类位于 Microsoft.Extensions.AI.OpenAI 包中，是 `OpenAIResponsesChatClient` 的具体实现：

```csharp
namespace Microsoft.Extensions.AI;

/// <summary>
/// Represents an <see cref="IChatClient"/> for an <see cref="ResponsesClient"/>.
/// </summary>
internal sealed class OpenAIResponsesChatClient : IChatClient
{
    ........
}
/// IChatClient 接口位于 Microsoft.Extensions.AI.Abstractions 包中
```

### Chat Completions API 调用链

下面是部分源代码，我们调用了 `GetChatClient` 方法来实际返回 `AzureChatClient`：

下面代码位于 Azure.AI.OpenAI 包中的 `AzureOpenAIClient.cs` 类中，例子中我们只拿出了一个方法，别的方法省略：

```csharp
public partial class AzureOpenAIClient : OpenAIClient
{
    public override ChatClient GetChatClient(string deploymentName)
        => new AzureChatClient(Pipeline, deploymentName, _endpoint, _options);
}
// 而 AzureOpenAIClient 的父类 OpenAIClient 类位于 OpenAI NuGet 包中
```

下面代码是 `AzureChatClient` 的源码，位于 Azure.AI.OpenAI NuGet 包中的 `AzureChatClient.cs` 类中：

```csharp
internal partial class AzureChatClient : ChatClient
{
    private readonly string _deploymentName;
    private readonly Uri _endpoint;
    private readonly string _apiVersion;

    internal AzureChatClient(
        ClientPipeline pipeline,
        string deploymentName,
        Uri endpoint,
        AzureOpenAIClientOptions options)
        : base(
            pipeline,
            model: deploymentName,
            new OpenAIClientOptions() { Endpoint = endpoint })
    {
        Argument.AssertNotNull(pipeline, nameof(pipeline));
        Argument.AssertNotNullOrEmpty(deploymentName, nameof(deploymentName));
        Argument.AssertNotNull(endpoint, nameof(endpoint));
        options ??= new();

        _deploymentName = deploymentName;
        _endpoint = endpoint;
        _apiVersion = options.Version;
    }

    ....................
}
// 而 ChatClient 类位于 OpenAI NuGet 包中 ChatClient.cs 类中
```

接下来我们继续看 `OpenAIChatClientExtensions` 类。  
该类是 `ChatClient` 的扩展类，内部同样定义了两个重载的 `CreateAIAgent` 方法，其中包含 `client.AsIChatClient()` 方法。

下面类位于 Microsoft.Agents.AI.OpenAI 包中，`OpenAIChatClientExtensions.cs` 源码如下：

```csharp
public static class OpenAIChatClientExtensions
{
    public static ChatClientAgent CreateAIAgent(
        this ChatClient client,
        string? instructions = null,
        string? name = null,
        string? description = null,
        IList<AITool>? tools = null,
        Func<IChatClient, IChatClient>? clientFactory = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null) =>
        client.CreateAIAgent(
            new ChatClientAgentOptions()
            {
                Name = name,
                Description = description,
                ChatOptions =
                    tools is null && string.IsNullOrWhiteSpace(instructions)
                        ? null
                        : new ChatOptions()
                        {
                            Instructions = instructions,
                            Tools = tools,
                        }
            },
            clientFactory,
            loggerFactory,
            services);

    public static ChatClientAgent CreateAIAgent(
        this ChatClient client,
        ChatClientAgentOptions options,
        Func<IChatClient, IChatClient>? clientFactory = null,
        ILoggerFactory? loggerFactory = null,
        IServiceProvider? services = null)
    {
        Throw.IfNull(client);
        Throw.IfNull(options);

        var chatClient = client.AsIChatClient();

        if (clientFactory is not null)
        {
            chatClient = clientFactory(chatClient);
        }

        return new ChatClientAgent(chatClient, options, loggerFactory, services);
    }
}
```

下面代码位于 Microsoft.Extensions.AI.OpenAI 包中，  
是 `OpenAIClientExtensions` 类的 `AsIChatClient` 方法源码（其它方法省略）：

```csharp
public static class OpenAIClientExtensions
{
    public static IChatClient AsIChatClient(
        this ChatClient chatClient) =>
        new OpenAIChatClient(chatClient);
}
// Microsoft.Extensions.AI.OpenAI 包中
```

上面方法返回一个 `OpenAIChatClient` 类，  
下面是该类的源码，可以看到它实现了 `IChatClient` 接口：

```csharp
internal sealed partial class OpenAIChatClient : IChatClient
{
}
```

到这里，我们已经理清了两种不同的 OpenAI 协议调用链路，以及它们是如何在 microsoft/agent-framework 框架中被封装和使用的。


