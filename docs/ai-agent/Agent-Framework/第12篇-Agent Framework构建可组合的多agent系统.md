在构建复杂的 AI 应用时，我们经常会遇到一个难题：如何让一个通用 Agent 调用另一个专业 Agent？

单一 Agent 往往无法胜任所有工作，我们需要明确的“分工”。本文通过 Microsoft Agent Framework 的一个示例，演示如何将一个 Agent 封装成标准的“函数工具”，供另一个 Agent 按需调用。

这种模式通常被称为 Agent-as-a-Function，它显著增强了多 Agent 系统的可组合性。

## 场景设定

我们构建一个“套娃”式的多 Agent 场景：

- 底层工具：一个查询天气的 C# 本地函数
- 子 Agent（WeatherAgent）：天气专家，持有底层工具
- 主 Agent（Main Agent）：负责用户交互，并且必须只用日语输出结果
- 任务：用户用中文提问，主 Agent 调用子 Agent 获取信息后，根据自身指令翻译并用日语回答

## 核心代码解析

### 1. 准备工作与本地函数

首先配置 Azure OpenAI 连接，并定义一个最基础的 GetWeather 本地函数。这里使用 [Description] 特性，是为了让 LLM 能更准确理解工具的用途与参数语义。

为了突出 Agent 组合模式，这里使用了简化的本地函数；在真实项目中，可以替换为 API / MCP / 插件调用。

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

[Description("获取指定地点的天气信息。")]
static string GetWeather([Description("要获取天气的地点。")] string location)
    => $"{location} 多云，最高气温 15°C。";
```

### 2. 创建子 Agent：WeatherAgent（天气专家）

接下来创建一个专注于天气问题的专家 Agent，并将 GetWeather 注册为它的工具能力。

此时，weatherAgent 仍然只是一个独立 Agent，尚未作为函数对外暴露。

```csharp
AIAgent weatherAgent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "WeatherAgent",
        instructions: "你专注于回答天气相关的问题，必要时调用工具获取信息后再回答。",
        description: "一个提供天气信息的智能体。",
        tools: [AIFunctionFactory.Create(GetWeather)]
    );
```

### 3. 关键一步：将 Agent 封装为函数（Agent-as-a-Function）

核心在于 `.AsAIFunction()`。

它并不是一次简单的函数调用，而是一次完整的 Agent 推理过程，只是对外表现为一个标准的 AI Function 契约。

```csharp
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: """
你是一个助手，必须只用日语回答。
工具返回的内容可能是中文，请将其翻译成自然的日语后再输出。
不要输出中文或英文。
""",
        tools: [weatherAgent.AsAIFunction()]
    );
```

对主 Agent 来说，WeatherAgent 就像一个普通工具函数，但其内部实际上包含完整的 Prompt、推理和工具调用逻辑。

### 4. 运行效果与调用流程

```csharp
Console.WriteLine(await agent.RunAsync("东京的天气如何？"));
```

一次典型的执行过程如下：

1. 用户用中文提问
2. 主 Agent 在推理过程中判断需要天气信息，并选择调用 WeatherAgent 工具
3. 子 Agent 调用 `GetWeather("东京")` 获取结果
4. 子 Agent 返回天气信息
5. 主 Agent 根据自身 System Prompt，将结果翻译并以日语输出

最终输出示例：

> 東京の天気は曇りで、最高気温は15℃です。

在真实系统中，超时控制、调用深度限制、错误回传与重试等逻辑，通常放在主 Agent 的工具调用包装层（tool policy / middleware）中统一处理，以避免多 Agent 调用链失控或变成黑盒。

## 为什么这种模式很重要？

- 封装与复用：复杂能力（如 SQL Agent、Code Agent）可以整体封装为工具
- 职责分离：主 Agent 负责交互与语言控制，子 Agent 专注垂直领域推理
- 可组合扩展：作为工具的 Agent 内部仍可调用其他 Agent，形成树状结构
- 轻量 vs 编排器：当只需要能力组合时，Agent-as-a-Function 更轻量；当需要复杂路由与多步工作流时，再引入 orchestrator 更合适

## 总结

`AsAIFunction()` 本质上是一座桥梁：它用 Function 作为契约，把一个完整的 Agent 能力模块暴露给其他 Agent 使用。

通过这种方式，我们不再构建一个全知全能的巨型 Brain，而是构建一组可组合、可复用、职责清晰的专家团队。

如果你正在使用 Microsoft Agent Framework，不妨尝试把你的通用能力抽取出来，封装成 Agent Tool，让系统能力真正“模块化”。
