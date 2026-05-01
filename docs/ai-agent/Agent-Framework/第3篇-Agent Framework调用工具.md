# 使用 Agent Framework 调用工具的实战解析

在现代 AI 应用场景中，Agent Framework 为开发者提供了强大的能力，能够集成多种工具，使智能体更智能、灵活，满足复杂业务需求。本文将围绕以下代码片段，讲解如何在 Agent Framework 中集成和调用工具，并通过具体案例分析其应用流程和核心优势。

## 示例代码解析

```csharp
// ... 引用与变量初始化略 ...

[Description("获取指定国家的最新新闻标题。")]
static string GetNews([Description("国家名称。")] string country)
    => $"来自 {country} 的头条新闻：AI 正在革新软件开发领域。";

// 创建 agent 并集成工具方法
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一个乐于助人的助手", tools: [AIFunctionFactory.Create(GetNews)]);

// 非流式调用
Console.WriteLine(await agent.RunAsync("美国的最新新闻头条有哪些？"));

// 流式调用
await foreach (var update in agent.RunStreamingAsync("美国的最新新闻头条有哪些？"))
{
    Console.WriteLine(update);
}
```

## 1. 定义工具函数

在 `Agent Framework` 中，所谓“工具”就是可被 AI 智能体调用的外部函数。例如上述以 `[Description]` 注解的 `GetNews` 方法，通过标签描述其用途和参数，为后续自动文档和可用性提供便利。

```csharp
[Description("获取指定国家的最新新闻标题。")]
static string GetNews([Description("国家名称。")] string country)
    => $"来自 {country} 的头条新闻：AI 正在革新软件开发领域。";
```

## 2. 创建 Agent 并集成工具

利用 AzureOpenAIClient 快速创建 Agent，并将工具方法通过 `AIFunctionFactory.Create` 包裹后传递给智能体。`instructions` 参数为智能体设定行为准则，例如“你是一个乐于助人的助手”，有利于模型更好理解上下文。

```csharp
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "You are a helpful assistant", tools: [AIFunctionFactory.Create(GetNews)]);
```

## 3. 智能体交互方式

Agent Framework 支持两种交互模式：
- **非流式调用**：一次性返回完整答案，适合简单场景。
- **流式调用**：以迭代方式返回智能体生成内容，适合实时反馈或生成较大文本片段的场景。

```csharp
// 非流式
Console.WriteLine(await agent.RunAsync("美国的最新新闻头条有哪些？"));

// 流式
await foreach (var update in agent.RunStreamingAsync("美国的最新新闻头条有哪些？"))
{
    Console.WriteLine(update);
}
```

## 优势与应用场景

1. **工具函数自动集成**：只需简单定义和注册，即可被智能体自动识别和调用，无需冗杂配置。
2. **自然语言驱动**：用户只需用自然语言与智能体对话，工具方法可自动补齐响应，极大简化开发流程。
3. **灵活扩展**：可动态添加多种工具，例如菜单推荐、设备控制等，充分释放智能体能力。
4. **多交互模式**：满足实时流式反馈、分步推理等高级需求，提升用户体验。

## 总结

Agent Framework 为 AI 智能体集成多种功能工具提供了标准化、易扩展的架构。只需简单步骤，即可实现让 AI 智能体通过自然语言自动调用工具方法，大幅提升企业智能助手、自动化流程的开发效率与智能化水平。未来，随着 Agent Framework 持续迭代，更多行业和业务场景都能够充分受益于智能体与工具的结合与协同。