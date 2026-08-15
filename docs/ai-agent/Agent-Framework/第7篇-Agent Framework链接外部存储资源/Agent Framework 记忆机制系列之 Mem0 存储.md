上一节介绍了 `ValkeyChatHistoryProvider`。它把 Agent 的聊天历史保存到 Valkey 中，适合做会话级别的上下文恢复。

这一节介绍另一种更偏“长期记忆”的存储方案：`Mem0`。

`Mem0` 不是简单地把每一条聊天记录原样保存下来，而是会从对话中提炼出对用户或任务真正有价值的事实，并将其持久化为可检索的记忆。换句话说，它更关注“记住什么、以后怎么找回来”，而不是“把所有内容逐条保存下来”。

## 什么是 Mem0？

Mem0（“mem-zero”）为 AI、Agent 提供了一层智能记忆层，能够实现更个性化的 AI 交互。它会记住用户偏好，适应不同用户需求，并随着时间不断学习，特别适合客服聊天机器人、AI 助手和自治系统等场景。

Mem0 不但可以给我们普通的 Agent 做记忆层，还可以作为插件植入到 Coding Agent 做记忆层，例如：Claude Code / Claude Cowork、Cursor、Codex、OpenCode。

Mem0 在 GitHub 上开源超过 60K Stars，地址为：
[https://github.com/mem0ai/mem0](https://github.com/mem0ai/mem0)

有关更多的 Mem0 的介绍以及部署方法，请查看 GitHub 官方地址。

同时，Agent Framework 也提供了 Mem0 的 SDK，方便 Agent 开发者快速集成。但是目前包还没有发布到 Nuget 上，我估计很快就要发布了。

## Agent Framework 集成 Mem0 的方式

我们这里使用基于 Mem0 的 Cloud Memory 服务来实现 Agent 的记忆功能。Agent Framework 提供了一个 Mem0 的 SDK，开发者可以通过它快速集成 Mem0 到自己的 Agent 中。

### 注册 Mem0 账号

我们需要注册 Mem0 的账号，注册地址：
[https://app.mem0.ai/login](https://app.mem0.ai/login)

注册完成之后，我们需要 Key 为我们集成 Agent Framework 的 Mem0 SDK 提供授权。

免费用户有一些限制，如下图所示：

### Mem0 SDK 集成到 Agent Framework

由于 Mem0 SDK 还没有发布到 Nuget 上，我们需要手动下载 Mem0 SDK 的源码，首先引用 `Microsoft.Agents.AI.Mem0` 项目到我们的 Agent Framework 解决方案中，该包中为我们提供了一个 `Mem0Provider` 类。

如下代码有两个需要注意：

1. 初始化 `Mem0Provider` 类时需要传入一个 `HttpClient` 对象，用于配置 Mem0 的 API 访问地址和 Key。
2. 初始化 `Mem0Provider` 类时需要传入一个 `Func<AgentSession?, State>`，用于在首次调用时初始化 Provider 的状态，并提供存储范围和搜索范围。
   
```csharp
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- DeepSeek configuration ---
var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? throw new InvalidOperationException("DEEPSEEK_API_KEY 未设置。");
var endpoint = Environment.GetEnvironmentVariable("DEEPSEEK_ENDPOINT") ?? "https://api.deepseek.com/v1";
var model = Environment.GetEnvironmentVariable("DEEPSEEK_MODEL") ?? "deepseek-v4-pro";
var mem0ServiceUri = "https://api.mem0.ai";
var mem0ApiKey = "m0-DRb44AheBuB6jJBlZEi0zHzQM5vaDBwyXNRRJuBn";
using HttpClient mem0HttpClient = new();
mem0HttpClient.BaseAddress = new Uri(mem0ServiceUri);
mem0HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", mem0ApiKey);

// 使用OpenAIClient来兼容DeepSeek客户端
var chatClient = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    .GetChatClient(model)
    .AsIChatClient();
AIAgent agent = chatClient
    .AsAIAgent(new ChatClientAgentOptions()
    {
        ChatOptions = new() { ModelId = model, Instructions = "你是一个友好的旅行助理。回答时请使用你已知的关于用户的信息，不要编造细节。" },

        AIContextProviders = [new Mem0Provider(mem0HttpClient, stateInitializer: _ => new(new Mem0ProviderScope() { ApplicationId = "getting-started-agents", UserId = "sample-user" }))]
    });

AgentSession session = await agent.CreateSessionAsync();

Mem0Provider mem0Provider = agent.GetService<Mem0Provider>()!;
await mem0Provider.ClearStoredMemoriesAsync(session);

Console.WriteLine(await agent.RunAsync("我叫桂兵兵，正计划在十一月去北海道旅行。", session));
Console.WriteLine(await agent.RunAsync("我会和朋友一起旅行，我们都喜欢寻找风景优美的观景点。", session));

Console.WriteLine("\n等待片刻，让 Mem0 索引新的记忆...\n");
await Task.Delay(TimeSpan.FromSeconds(2));

Console.WriteLine(await agent.RunAsync("关于我即将开始的旅行，你已经知道些什么？", session));

Console.WriteLine("\n>> 序列化和反序列化会话以演示持久化状态\n");
JsonElement serializedSession = await agent.SerializeSessionAsync(session);
AgentSession restoredSession = await agent.DeserializeSessionAsync(serializedSession);
Console.WriteLine(await agent.RunAsync("你能回顾一下你记得的关于我的个人信息吗？", restoredSession));

Console.WriteLine("\n>> 开始一个共享相同 Mem0 范围的新会话\n");
AgentSession newSession = await agent.CreateSessionAsync();
Console.WriteLine(await agent.RunAsync("总结一下你已经知道的关于我的信息。", newSession));
```

### 运行效果如下：





### Mem0 的Cloud Memory 服务







我们可以看出Agent Framework为我们提供了灵活的扩展，不仅模型册可以任意选择不同模型，Agent 记忆层也可以选择不同的存储方案。


