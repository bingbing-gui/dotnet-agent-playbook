在用 OpenAI 模型开发应用时，最常见的方式就是通过 API 和模型交互。随着 OpenAI 平台不断发展，现在主要有两种调用方式：Chat Completions API 和 Responses API。

Chat Completions 是比较早的一套接口，很多聊天类应用都是基于它来实现的。而 Responses API 则是新推出的一种统一接口，定位更广，不只是聊天，还可以支持更多复杂的 AI 场景。

理解这两种 API 的设计思路，其实挺重要的，这样在实际项目里就能更清楚该用哪一种。

在 Azure OpenAI 里，这两种方式也都有对应支持。一般来说，如果只是做一个简单的对话应用，用 Chat Completions 会更直接；但如果你的场景需要更灵活的输入结构，或者后面还要扩展能力，比如工具调用、多模态等，那用 Responses API 会更合适一些。

在此基础上，还可以通过 Microsoft Agent Framework 对这两种调用方式进行进一步封装。我们分别来介绍这两种API

## Chat Completions API

根据 OpenAI 官方文档的说明：

> The Chat Completions API generates assistant responses based on a list of messages in a conversation.

也就是说，Chat Completions API 会根据一组对话消息生成模型回复。开发者需要以消息列表的形式向模型提供上下文，每条消息包含角色（如 `system`、`user`、`assistant`）以及对应的内容。

一个典型请求示例如下：

```http
POST /v1/chat/completions

{
  "model": "gpt-4o",
  "messages": [
    {"role": "system", "content": "You are a helpful assistant."},
    {"role": "user", "content": "Explain what an API is."}
  ]
}
```

在这种模式下，如果需要实现多轮对话，应用程序需要自行维护对话历史，并在每次请求时重新发送完整的消息列表。

Chat Completions API 的设计目标是提供一种简单直接的方式，让开发者可以构建聊天机器人或对话类应用。


## Chat Completions在Microsoft Agent Framework中的应用

### Dependencies

| Package | Version |
|--------|--------|
| Azure.AI.OpenAI | 2.8.0-beta.1 |
| Azure.Identity | 1.19.0 |
| Microsoft.Agents.AI.OpenAI | 1.0.0-rc4 |

```csharp
AIAgent chatCompletionPattern = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
     .GetChatClient(deploymentName)
     .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

// Invoke the agent and output the text result.
Console.WriteLine(await chatCompletionPattern.RunAsync("给我讲一个发生在茶馆里的段子。"));
```
---

## Responses API

OpenAI 在后续推出了 Responses API，用于统一不同类型的模型调用方式。官方文档中对它的描述是：

> The Responses API provides a unified interface for generating model responses.

也就是说，Responses API 提供了一个统一的接口，用于生成模型响应。

与 Chat Completions API 不同，Responses API 的输入结构更加通用，不再局限于聊天消息。开发者可以通过 `input` 字段直接提供文本或其他类型的数据。

例如：

```http
POST /v1/responses

{
  "model": "gpt-4.1",
  "input": "Write a short poem about the ocean."
}
```

Responses API 同时支持更多功能，例如：

- 多模态输入（文本、图片等）
- 工具调用（tool use）
- 结构化输出
- 更复杂的交互流程

因此，它不仅可以用于聊天，还可以支持更广泛的 AI 应用场景。

## Responses API在Microsoft Agent Framework中的应用

### Dependencies

| Package | Version |
|--------|--------|
| Azure.AI.OpenAI | 2.8.0-beta.1 |
| Azure.Identity | 1.19.0 |
| Microsoft.Agents.AI.OpenAI | 1.0.0-rc4 |

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
#pragma warning disable OPENAI001 // Suppress experimental API warning

AIAgent responsePattrern = new AzureOpenAIClient(new Uri(endpoint),
    new DefaultAzureCredential())
     .GetResponsesClient(deploymentName)
     .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

#pragma warning restore OPENAI001
// Invoke the agent and output the text result.
Console.WriteLine(await responsePattrern.RunAsync("给我讲一个发生在茶馆里的段子。"));
```


## Chat Completions API 与 Responses API 对比

| 能力 | Chat Completions API | Responses API |
|------|---------------------|---------------|
| 文本生成 | ✅ 支持 | ✅ 支持 |
| 音频 | ✅ 支持 | ⏳ 即将支持 |
| 视觉 | ✅ 支持 | ✅ 支持 |
| 结构化输出 | ✅ 支持 | ✅ 支持 |
| 函数调用 | ✅ 支持 | ✅ 支持 |
| Web 搜索 | ✅ 支持 | ✅ 支持 |
| 文件检索 | ❌ 不支持 | ✅ 支持 |
| 计算机操作 | ❌ 不支持 | ✅ 支持 |
| 代码解释器 | ❌ 不支持 | ✅ 支持 |
| MCP（模型上下文协议） | ❌ 不支持 | ✅ 支持 |
| 图像生成 | ❌ 不支持 | ✅ 支持 |
| 推理摘要 | ❌ 不支持 | ✅ 支持 |

## 使用GPT-5.4-Pro模型

我们发现切换到GPT-5.4-Pro模型时，Chat Completions报错。而使用 Responses API 则能正常运行，我们发信啊GPT-5.4-Pro模型可能在Chat Completions API上还没有完全支持，或者存在兼容性问题。建议在使用新模型时优先考虑 Responses API，以确保能够充分利用新模型的能力。


## 总结

因此，在选择 API 时，建议根据具体的应用需求和模型支持情况来决定使用Chat Completions 还是 Responses API。对于简单的对话应用，Chat Completions 可能更直接；而对于需要更复杂交互和多模态支持的应用，Responses API 则是更好的选择。

