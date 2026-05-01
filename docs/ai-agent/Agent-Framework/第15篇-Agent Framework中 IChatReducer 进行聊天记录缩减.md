
## 序言

在多轮对话场景中，随着聊天次数增加，发送给大语言模型（LLM）的上下文会持续膨胀，带来 Token 成本上升与上下文溢出风险。
Microsoft Agent Framework 将这一问题抽象为 Chat Reduction（聊天记录缩减），并通过 `IChatReducer` 策略对聊天历史进行统一治理，而不是在业务代码中零散地裁剪或拼接历史消息。

本文基于“客户端本地存储聊天记录（Client-side history）”的典型场景，演示如何使用 `MessageCountingChatReducer` 自动限制历史长度，防止上下文无限增长，并观察在“历史被遗忘”后 Agent 行为的变化。

---

## 1. 代码关键实现步骤

### 引入必要的依赖
- `Microsoft.Extensions.AI`：提供统一的 AI 抽象（`ChatMessage`、`Reducer` 等）
- `Azure.AI.OpenAI`：用于连接 Azure OpenAI 服务
- `Microsoft.Agents.AI`：Agent Framework 核心能力

### 配置 Agent 与缩减策略（Reducer）
```csharp
AIAgent agent = new AzureOpenAIClient(
  new Uri(endpoint),
  new AzureCliCredential())
  .GetChatClient(deploymentName)
  .CreateAIAgent(new ChatClientAgentOptions
  {
    ChatOptions = new()
    {
      Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。"
    },
    Name = "Joker",
    // 关键点：自定义 ChatMessageStoreFactory
    ChatMessageStoreFactory = ctx => new InMemoryChatMessageStore(
      new MessageCountingChatReducer(2), // 仅保留最近 2 条 非 System 的 ChatMessage
      ctx.SerializedState,
      ctx.JsonSerializerOptions)
  });
```

### 组件说明
- `InMemoryChatMessageStore`
  - 聊天记录保存在客户端内存中
  - 适用于 Chat Completion / 本地上下文管理场景
- `MessageCountingChatReducer(2)`
  - 基于“消息数量”的缩减策略
  - 参数 2 表示仅保留最近 2 条非系统消息( 非 System 的 ChatMessage)
  - 超出部分的历史消息会被自动移除，而不是无限累积

---

## 2. 验证缩减效果

通过多轮连续对话，观察聊天记录在 Reducer 作用下的变化。在每一轮调用 `agent.RunAsync(...)` 后，读取当前线程中实际保留的聊天历史数量：

```csharp
AgentThread thread = agent.GetNewThread();


Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));

IList<ChatMessage>? chatHistory = thread.GetService<IList<ChatMessage>>();
Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");

// Invoke the agent a few more times.
Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", thread));
Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");
Console.WriteLine(await agent.RunAsync("保持刚才的语气，讲一个关于健忘冒险者的轻松小故事，像是在讲笑话一样。", thread));
Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");

// At this point, the chat history has exceeded the limit and the original message will not exist anymore,
// so asking a follow up question about it will not work as expected.
Console.WriteLine(await agent.RunAsync("接着刚才的氛围，讲一个发生在日常生活里的小乌龙事件，轻松随意一点。", thread));

Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");
```

### 对话过程说明
- 第一轮对话：「给我讲一个发生在茶馆里的段子，轻松一点的那种。」
  - 聊天历史较短，Reducer 尚未触发，历史消息正常累积
- 第二轮对话：「现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。」
  - 新消息加入，历史仍在阈值范围内，早期消息仍可访问
- 第三轮及之后：「保持刚才的语气，讲一个关于健忘冒险者的轻松小故事。」
  - 聊天记录达到缩减条件，`MessageCountingChatReducer` 开始生效
  - 最早的消息被自动移除，`chatHistory.Count` 保持在稳定范围内

---

## 3. 演示结果



---

## 4. 技术总结与适用场景

### 适用场景
- Client-side history：聊天历史由客户端或应用自身维护（如 OpenAI / Azure OpenAI Chat Completion API）

### 不适用场景
- Server-side history（如 Azure Foundry Agents）：聊天历史由服务端统一管理，客户端无法直接干预裁剪策略

### 可扩展性
`IChatReducer` 只是一个策略接口，可扩展更复杂的上下文治理逻辑：
- `TokenCountingChatReducer`：按 Token 数量而非消息条数进行缩减
- `SummaryChatReducer`：将旧消息压缩为摘要，而非直接删除

## 小结
- 聊天历史不应无限增长
- “遗忘”是一种主动、可控的系统设计
- 上下文治理应以策略形式存在，而非散落在业务代码中
- 合理使用 Chat Reduction，可在成本、稳定性与对话效果之间取得更好平衡


