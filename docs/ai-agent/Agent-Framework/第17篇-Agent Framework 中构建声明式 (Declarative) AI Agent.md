# 告别硬编码！在 Microsoft Agent Framework 中构建声明式（Declarative）AI Agent

在开发 AI 应用时，核心的 Prompt（提示词）、模型参数（如 Temperature）和输出格式常散落在 C# 代码中，既难维护，也不利于非技术同事参与调优。通过 Microsoft Agent Framework可以用“声明式”方式优雅构建 Agent。

## 什么是声明式 Agent？
- 命令式：在代码中逐步设置对象属性定义 Agent 行为。
- 声明式：将“定义”（是谁、做什么、参数）与“实现”（如何运行）分离，用 YAML/JSON 描述 Agent 大脑。

在该示例中，Agent 的定义放在一段 YAML 文本中。

## 核心代码解析

### 1. 配置

关于配置请参考[配置 Microsoft Agent Framework](../configuration/index.md)。
此外我们还需要额外添加一个包

```shell
dotnet add package Microsoft.Agents.AI.Declarative
``` 

### 2. 定义 Agent（YAML）

```yaml
kind: Prompt
name: Assistant
description: 乐于帮忙的小助手
instructions: 你是一个乐于帮忙的小助手。你会使用用户指定的语言来回答问题。你需要以 JSON 格式返回你的回答。
model:
    options:
        temperature: 0.9
        topP: 0.95
outputSchema:
    properties:
        language:
            type: string
            required: true
            description: 回答所使用的语言。
        answer:
            type: string
            required: true
            description: 回答的文本内容。
```

要点：
- 人设与指令（instructions）：助手角色，输出必须为 JSON。
- 模型参数（model.options）：在 YAML 中配置 temperature、topP。
- 结构化输出（outputSchema）：强制返回满足 Schema 的 JSON（language、answer），无需正则解析回复。

### 3. 创建与运行 Agent（C#）

```csharp
// 1. 准备基础的 ChatClient (Azure OpenAI)
IChatClient chatClient = new AzureOpenAIClient(...).GetChatClient(deploymentName).AsIChatClient();

// 2. 使用工厂从 YAML 创建 Agent
var agentFactory = new ChatClientPromptAgentFactory(chatClient);
var agent = await agentFactory.CreateFromYamlAsync(yamlText);

// 3. 运行 Agent
Console.WriteLine(await agent!.RunAsync("帮我讲一个发生在茶馆里面的笑话，用日语回答。"));
```

C# 仅负责“连接”和“执行”，业务逻辑完全由 YAML 定义。

## 为什么这种模式重要？

1. 关注点分离（Separation of Concerns）
     - 开发者：连接、鉴权、流式与异常处理。
     - Prompt 工程师/业务：YAML 中调提示、调参数、定结构。非技术同事也能读写。

2. 结构化输出（Structured Output）
     - 以 Schema 约束 LLM 输出，降低幻觉与解析错误，可直接反序列化为对象。

3. 可移植性与版本控制
     - YAML/JSON 可用 Git 管理与审阅，在多语言框架中复用同一定义。


### 输出效果

运行效果如下:



## 进阶：流式支持

即使由 YAML 创建的声明式 Agent，仍支持流式输出（Streaming）：

```csharp
await foreach (var update in agent!.RunStreamingAsync("帮我讲一个发生在茶馆里面的笑话，用日语回答。"))
{
        Console.WriteLine(update);
}
```
无需在“开发效率”和“用户体验”之间妥协。

## 总结
声明式AI Agent从 Hardcode 走向 Configuration 的关键一步。将 Prompt 与配置抽取为声明式 YAML，不仅让 Program.cs 更整洁，也为协作与演进提供更高效率。

