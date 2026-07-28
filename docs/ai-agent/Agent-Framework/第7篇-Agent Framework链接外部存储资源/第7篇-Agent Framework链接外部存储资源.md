在构建生成式 AI 应用时，我们经常面临一个关键挑战：“记忆管理（Memory Management）”。在简单的 Demo 中，我们通常把聊天记录（Chat History）直接存在内存的 `List<ChatMessage>` 中，这很容易。但在实际的生产环境，尤其是构建无状态（Stateless）的 Web API 时，这种方式就完全不够用了：

- 服务器重启，内存里的数据丢失
- 负载均衡导致请求落在不同服务器，导致上下文无法共享
- 用户刷新浏览器，session 消失
- 多终端（App / Web）无法共享对话历史

因此，我们需要将“记忆”托管到一个外部存储中，例如：

- 向量数据库（Azure AI Search / pgvector）
- Redis
- Cosmos DB
- SQL / NoSQL 数据库
- 任意持久化服务

本节我们将使用 Microsoft Agent Framework 来演示如何通过实现自定义的 `ChatMessageStore`，将 AI 的记忆托管给外部存储。示例中我们采用 InMemory VectorStore（仅用于演示），你可以替换为任意数据库。

## 引用包

- 需要的 NuGet 包：
    - Azure.AI.OpenAI (2.1.0)
    - Azure.Identity (1.18.0-beta.2)
    - Microsoft.Agents.AI.OpenAI (1.0.0-preview.251125.1)
    - Microsoft.Extensions.AI.OpenAI (10.0.1-preview.1.25571.5)
    - Microsoft.SemanticKernel.Connectors.InMemory (1.67.1-preview)

可选：使用命令行安装
```bash
dotnet add package Azure.AI.OpenAI --version 2.1.0
dotnet add package Azure.Identity --version 1.18.0-beta.2
dotnet add package Microsoft.Agents.AI.OpenAI --version 1.0.0-preview.251125.1
dotnet add package Microsoft.Extensions.AI.OpenAI --version 10.0.1-preview.1.25571.5
dotnet add package Microsoft.SemanticKernel.Connectors.InMemory --version 1.67.1-preview
```

我们这一节中使用 `Microsoft.SemanticKernel.Connectors.InMemory` 包来实现一个简单的内存存储。关于更多的第三方存储实现，可以参考：
https://mp.weixin.qq.com/s?__biz=MzkyMDI1MjE5MA==&mid=2247484547&idx=1&sn=e2fa0cb4ce315a5708e46a966674954e&scene=21&poc_token=HAstOGmjA7rrbIjz2cui1yP21XIv9JTUMMnHJLGq

引用外部包后，我们就可以开始编写代码了。老生常谈，基础配置请参考：https://mp.weixin.qq.com/s/tBOMo1AXqzZEjeBwirIRFQ

## 组装 Agent：注入自定义 ChatMessageStore

在创建 Agent 时，通过 `ChatMessageStoreFactory` 参数，告诉框架如何为每个 `AgentThread` 创建消息存储器。

```csharp
AIAgent agent = new AzureOpenAIClient(
        new Uri(endpoint),
        new AzureCliCredential())
        .GetChatClient(deploymentName)
        .CreateAIAgent(new ChatClientAgentOptions
        {
                Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。",
                Name = "Joker",
                ChatMessageStoreFactory = ctx =>
                {
                        return new VectorChatMessageStore(vectorStore, ctx.SerializedState, ctx.JsonSerializerOptions);
                }
        });
```

接着我们就可以使用这个 Agent 来进行对话了。

```csharp
// 创建线程并运行对话
AgentThread thread = agent.GetNewThread();
// 运行代理，传入线程以存储对话历史记录在向量存储中。
Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));
// 序列化线程状态，以便稍后使用。
JsonElement serializedThread = thread.Serialize();
Console.WriteLine("\n--- Serialized thread ---\n");
Console.WriteLine(JsonSerializer.Serialize(serializedThread, new JsonSerializerOptions { WriteIndented = true }));
// 反序列化线程状态以恢复对话。
AgentThread resumedThread = agent.DeserializeThread(serializedThread);
// 继续与代理对话，传入恢复的线程以访问以前的对话历史记录。
Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", resumedThread));
// 我们能够通过线程的 GetService 方法访问 VectorChatMessageStore，如果我们需要读取存储线程的键。
var messageStore = resumedThread.GetService<VectorChatMessageStore>()!;

Console.WriteLine($"\n线程唯一ID存储在向量数据库中: {messageStore.ThreadDbKey}");

Console.WriteLine("\n--- 完成 ---\n");
```

接下来我们定义 `VectorChatMessageStore` 来实现存储逻辑。

```csharp
internal sealed class VectorChatMessageStore : ChatMessageStore
{
        private readonly VectorStore _vectorStore;

        public VectorChatMessageStore(VectorStore vectorStore, JsonElement serializedStoreState, JsonSerializerOptions? jsonSerializerOptions = null)
        {
                this._vectorStore = vectorStore ?? throw new ArgumentNullException(nameof(vectorStore));

                if (serializedStoreState.ValueKind is JsonValueKind.String)
                {
                        // Here we can deserialize the thread id so that we can access the same messages as before the suspension.
                        this.ThreadDbKey = serializedStoreState.Deserialize<string>();
                }
        }

        public string? ThreadDbKey { get; private set; }

        public override async Task AddMessagesAsync(IEnumerable<ChatMessage> messages, CancellationToken cancellationToken = default)
        {
                this.ThreadDbKey ??= Guid.NewGuid().ToString("N");

                var collection = this._vectorStore.GetCollection<string, ChatHistoryItem>("ChatHistory");
                await collection.EnsureCollectionExistsAsync(cancellationToken);

                await collection.UpsertAsync(messages.Select(x => new ChatHistoryItem()
                {
                        Key = this.ThreadDbKey + x.MessageId,
                        Timestamp = DateTimeOffset.UtcNow,
                        ThreadId = this.ThreadDbKey,
                        SerializedMessage = JsonSerializer.Serialize(x),
                        MessageText = x.Text
                }), cancellationToken);
        }

        public override async Task<IEnumerable<ChatMessage>> GetMessagesAsync(CancellationToken cancellationToken = default)
        {
                var collection = this._vectorStore.GetCollection<string, ChatHistoryItem>("ChatHistory");
                await collection.EnsureCollectionExistsAsync(cancellationToken);
                var records = await collection
                        .GetAsync(
                                x => x.ThreadId == this.ThreadDbKey, 10,
                                new() { OrderBy = x => x.Descending(y => y.Timestamp) },
                                cancellationToken)
                        .ToListAsync(cancellationToken);

                var messages = records.ConvertAll(x => JsonSerializer.Deserialize<ChatMessage>(x.SerializedMessage!)!);
                messages.Reverse();
                return messages;
        }

        public override JsonElement Serialize(JsonSerializerOptions? jsonSerializerOptions = null) =>
                // We have to serialize the thread id, so that on deserialization we can retrieve the messages using the same thread id.
                JsonSerializer.SerializeToElement(this.ThreadDbKey);

        /// <summary>
        /// The data structure used to store chat history items in the vector store.
        /// </summary>
        private sealed class ChatHistoryItem
        {
                [VectorStoreKey]
                public string? Key { get; set; }

                [VectorStoreData]
                public string? ThreadId { get; set; }

                [VectorStoreData]
                public DateTimeOffset? Timestamp { get; set; }

                [VectorStoreData]
                public string? SerializedMessage { get; set; }

                [VectorStoreData]
                public string? MessageText { get; set; }
        }
}
```

## 实现存储逻辑

需要继承 `ChatMessageStore` 并重写关键方法。

- 存（`AddMessagesAsync`）：不存内存，直接写库。
- 取（`GetMessagesAsync`）：通过 ID 去库里查，按时间排序。
- 序列化（`Serialize`）：当系统要求 Agent “序列化当前状态”时，只返回 ID。

```csharp
public override JsonElement Serialize(JsonSerializerOptions? options = null) =>
                // 哪怕聊了 100 句，序列化结果也只是一个轻量级的 ID 字符串
                JsonSerializer.SerializeToElement(this.ThreadDbKey);
```

## 代码执行逻辑序列

sequenceDiagram
    autonumber

    participant Main
    participant Agent as AIAgent
    participant Thread as AgentThread
    participant Factory as ChatMessageStoreFactory
    participant MsgStore as VectorChatMessageStore
    participant VStore as VectorStore
    participant Model

    %% ① GetNewThread
    Main->>Agent: ① GetNewThread()
    Agent->>Agent: 创建 AgentThread（尚未绑定 Store）
    Agent-->>Main: 返回 AgentThread(thread)

    %% ② RunAsync 第一次调用
    Main->>Agent: ② RunAsync(\"茶馆段子\", thread)
    Agent->>Thread: 2.1 获取 ChatMessageStore\nthread.GetService<ChatMessageStore>()
    Thread->>Factory: 如无实例 → 调用 ChatMessageStoreFactory(ctx)
    Factory->>MsgStore: new VectorChatMessageStore(vectorStore, serializedState: null)
    MsgStore-->>Thread: 绑定到当前 Thread

    Agent->>MsgStore: 2.2 GetMessagesAsync()
    MsgStore->>VStore: 查询 ChatHistory\n条件：ThreadId == null
    VStore-->>MsgStore: 返回空列表
    MsgStore-->>Agent: 历史消息：[]

    Agent->>Model: 2.3 调用模型生成“茶馆段子”
    Model-->>Agent: 返回 assistant 回复

    Agent->>MsgStore: 2.4 AddMessagesAsync([user, assistant])
    MsgStore->>MsgStore: ThreadDbKey 为空 → 生成 GUID
    MsgStore->>VStore: Upsert 两条记录\nKey = ThreadDbKey + MessageId\nThreadId = ThreadDbKey
    VStore-->>MsgStore: 写入成功
    Agent-->>Main: 返回第一次对话结果

    %% ③ Serialize
    Main->>Thread: ③ thread.Serialize()
    Thread->>MsgStore: 调用 Serialize()
    MsgStore-->>Thread: 返回 JsonElement(ThreadDbKey)
    Thread-->>Main: 返回 serializedThread（包含 ThreadDbKey）

    %% ④ DeserializeThread
    Main->>Agent: ④ DeserializeThread(serializedThread)
    Agent->>Agent: 创建新的 AgentThread(resumedThread)
    Agent->>Factory: 调用 ChatMessageStoreFactory(ctx.SerializedState)
    Factory->>MsgStore: new VectorChatMessageStore(vectorStore, serializedState: ThreadDbKey)
    MsgStore->>MsgStore: 构造函数中恢复 this.ThreadDbKey
    MsgStore-->>Agent: 绑定到 resumedThread
    Agent-->>Main: 返回 resumedThread

    %% ⑤ RunAsync 第二次调用
    Main->>Agent: ⑤ RunAsync(\"再讲一遍+表情\", resumedThread)
    Agent->>Thread: 5.1 获取 ChatMessageStore
    Thread-->>Agent: 返回 VectorChatMessageStore

    Agent->>MsgStore: 5.2 GetMessagesAsync()
    MsgStore->>VStore: 查询 ChatHistory\n条件：ThreadId == ThreadDbKey
    VStore-->>MsgStore: 返回之前两条消息
    MsgStore-->>Agent: 历史消息：[user, assistant]

    Agent->>Model: 5.3 带着历史 + 新用户消息调用模型
    Model-->>Agent: 返回新的 assistant 回复（带表情）

    Agent->>MsgStore: 5.4 AddMessagesAsync([user, assistant])
    MsgStore->>VStore: 继续用同一 ThreadDbKey 写入新消息
    VStore-->>MsgStore: 写入成功
    Agent-->>Main: 返回第二次对话结果

    %% ⑥ 获取存储器实例
    Main->>Thread: ⑥ GetService<VectorChatMessageStore>()
    Thread-->>Main: 返回 MsgStore 实例
    Main->>MsgStore: 访问 ThreadDbKey（GUID）


## 总结

通过解耦“计算”（Agent）与“存储”（VectorStore），让 AI 应用更健壮。

- 扩展性：可替换底层存储（Redis、CosmosDB、Postgres），可以使用不同的连接器。
- 轻量化：前端或客户端只需保存一个极小的 Thread ID。
- 云原生友好：无状态的服务端设计，便于水平扩展。
