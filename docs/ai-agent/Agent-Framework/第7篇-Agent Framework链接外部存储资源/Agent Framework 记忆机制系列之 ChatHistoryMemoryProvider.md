
大语言模型本身不会主动记住之前的对话。

聊天应用(ChatGPT等)之所以能够表现出连续的对话能力，是因为应用会在每次调用模型时，将当前会话中的部分或全部历史消息重新放入请求上下文。模型重新看到这些历史消息后，才会表现得像是“记得”之前说过的话。

在Agent Framework中，会话状态通常与 AgentSession 关联。当创建一个新的AgentSession时，新会话默认不会包含旧 Session 中的聊天历史。

如果希望 Agent 在新会话中仍然能够找回用户过去说过的信息，就需要将历史消息保存到外部存储中，并在后续对话中重新检索。

ChatHistoryMemoryProvider 正是基于这种机制实现跨会话记忆的：

> 它将聊天消息写入向量存储，并根据当前问题检索语义相关的历史消息，再将这些消息补充到当前上下文中。

在正式介绍 ChatHistoryMemoryProvider 之前，我们先了解它所依赖的向量存储。

## 什么是向量存储

向量存储（Vector Store）是一种能够保存向量数据并执行相似度搜索的存储系统。

在大语言模型应用中，一段文本通常会先通过 Embedding 模型转换为一组数字，也就是向量。

例如：

```text
我是一名教育工作者
    ↓
Embedding 模型
    ↓
[0.12, -0.37, 0.81, ...]
```

这些向量可以反映文本在语义上的特征。

当用户之后输入：

我是一名老师

系统也会将这句话转换成向量，然后在向量存储中查找语义上最相似的历史消息。

即使两句话没有使用完全相同的关键词，只要语义接近，也有可能被检索出来。

因此，向量存储非常适合以下场景：

- 检索增强生成，也就是 RAG；
- 保存 Agent 的长期记忆；
- 搜索相关的聊天历史；
- 语义搜索；
- 混合搜索。

ChatHistoryMemoryProvider 正是使用向量存储保存和检索聊天消息。

## Agent Framework 的向量存储抽象

不同向量数据库提供的接口并不相同。Qdrant、Redis、Postgres、Azure AI Search 和 MongoDB 都有自己的客户端和搜索方式。

为了统一不同向量存储的使用方式，Agent Framework 依赖 `Microsoft.Extensions.VectorData.Abstractions` 包。这个包提供了一套用于在 .NET 中操作向量存储的统一抽象。

应用可以面向这套抽象编写代码，而不必直接依赖某一种数据库。底层向量存储发生变化时，上层组件通常不需要进行大范围修改。

可以简单理解为，Agent Framework 调用的是 `Microsoft.Extensions.VectorData.Abstractions` 定义的接口，具体连接器再把这些调用转换成对应数据库能够识别的请求。

如果`Microsoft.Extensions.VectorData.Abstractions`提供的是统一接口，那么每一种向量存储都需要提供相应的连接器来实现这些接口。连接器也可以理解为适配器，它负责把抽象层中的写入、删除和搜索操作映射到底层数据库的 API。

Agent Framework 支持的向量存储实现如下。表格中的 Implementation 表示支持的向量数据库或存储方案，C# 表示是否提供 C# 连接器，Uses officially supported SDK 表示连接器底层是否使用数据库厂商正式支持的 SDK，Maintainer / Vendor 则表示这个连接器由谁维护。

| Implementation | C# | Uses officially supported SDK | Maintainer / Vendor |
| --- | --- | --- | --- |
| Azure AI Search | ✅ | ✅ | Microsoft |
| Cosmos DB MongoDB (vCore) | ✅ | ✅ | Microsoft |
| Cosmos DB No SQL | ✅ | ✅ | Microsoft |
| Couchbase | ✅ | ✅ | Couchbase |
| Elasticsearch | ✅ | ✅ | Elastic |
| In-Memory | ✅ | N/A | Microsoft |
| MongoDB | ✅ | ✅ | Microsoft |
| Neon Serverless Postgres | Use Postgres Connector | ✅ | Microsoft |
| Oracle | ✅ | ✅ | Oracle |
| Pinecone | ✅ | ❌ | Microsoft |
| Postgres | ✅ | ✅ | Microsoft |
| Qdrant | ✅ | ✅ | Microsoft |
| Redis | ✅ | ✅ | Microsoft |
| SQL Server | ✅ | ✅ | Microsoft |
| SQLite | ✅ | ✅ | Microsoft |
| Volatile (In-Memory) | Deprecated（use In-Memory） | N/A | Microsoft |
| Weaviate | ✅ | ✅ | Microsoft |

这里需要注意，Maintainer / Vendor 表示的是连接器的维护方，并不是数据库本身的开发者。

以 Qdrant 为例：

| Implementation | C# | Uses officially supported SDK | Maintainer / Vendor |
| --- | --- | --- | --- |
| Qdrant | ✅ | ✅ | Microsoft |

Qdrant 数据库、数据库接口以及官方 SDK 仍然由 Qdrant 提供。Microsoft 负责的是 Qdrant 与 Microsoft.Extensions.VectorData 之间的集成，也就是实现并维护对应的连接器。

Elasticsearch 的情况有所不同，它的连接器由 Elastic 自己维护，因此表格中的 Maintainer / Vendor 显示为 Elastic。

不同连接器的维护方和底层 SDK 支持情况可能不同。在生产环境中使用之前，还需要根据具体数据库的文档确认版本兼容性、许可证和技术支持情况。

关于 .NET 向量存储的更多内容，可以参考微软官方文档：

<https://learn.microsoft.com/en-us/dotnet/ai/vector-stores/overview>


实际上我们在去年讲解Semantic Kernel 的讲解过相关的内容：

https://mp.weixin.qq.com/s/CFEbma2YN2CNg5aU6RIFyw
https://mp.weixin.qq.com/s/m3Gwbgrqp2g-4ZHUZZs_0A
https://mp.weixin.qq.com/s/7azAYaAmYQweYm2rQ9F3VA
https://mp.weixin.qq.com/s/l6xsXSlxN37o8Tq5ephxwQ
https://mp.weixin.qq.com/s/nScl2YUZY3KRgNftb0O5ug

## ChatHistoryMemoryProvider

了解向量存储之后，再来看 ChatHistoryMemoryProvider 就比较容易了。

ChatHistoryMemoryProvider 是一个 AIContextProvider。它会将聊天历史存储到向量存储中，并在后续对话中检索相关消息，用来补充当前请求的上下文。

它与普通聊天历史最大的区别，是不会简单地把所有历史消息重新发送给模型，而是根据当前输入，从过去的消息中找出语义上相关的内容。

该提供器以两个阶段运行：

1. 存储(Storage)：在每次 agent 调用之后，新的请求和响应消息会被存储到向量存储中，并根据它们的内容生成 embedding。

2. 检索(Retrieve)：在每次调用之前（或通过函数调用按需进行），该提供器会在向量存储中搜索与当前用户输入语义相似的消息，并将它们作为上下文注入。

存储的消息可以根据标识符来划分作用域。比如（应用(application)、代理(agent)、用户(user)、会话(session)）进行作用域划分，从而可以对哪些历史被存储和可搜索进行细粒度控制。

## 创建向量存储

我们在下面示例中使用的是 `InMemoryVectorStore`，向量则由 Foundry 项目中部署的 Embedding 模型生成。这里需要提前准备一个能够将文本转换为向量的 Embedding 模型。

常见的云端 Embedding 模型如下：

| 厂商 | 模型名称 | 说明 |
|---|---|---|
| OpenAI | `text-embedding-3-small`、`text-embedding-3-large` | OpenAI 提供的文本 Embedding 模型 |
| 阿里云 Model Studio | `text-embedding-v4` | 阿里云当前提供的文本向量模型之一 |
| Google Gemini | `gemini-embedding-2` | Google 提供的多模态 Embedding 模型 |
| Cohere | `embed-v4.0` | 支持文本、图像以及图文混合内容 |
| Voyage AI | `voyage-4`、`voyage-4-lite`、`voyage-code-3` | 其中 `voyage-code-3` 主要面向代码检索场景 |

除了通过云端 API 使用 Embedding 模型，也可以选择在本地部署开放权重模型，例如 Qwen3-Embedding 系列。

三种 Qwen3-Embedding 模型的特点可以简单理解为：

| 模型 | 特点 |
|---|---|
| `Qwen3-Embedding-0.6B` | 模型体积较小，推理速度较快，适合开发测试和资源有限的环境 |
| `Qwen3-Embedding-4B` | 在检索效果和资源消耗之间相对均衡 |
| `Qwen3-Embedding-8B` | 模型规模更大，通常能够提供更好的检索效果，但需要更多显存和计算资源 |

需要注意，`0.6B`、`4B` 和 `8B` 表示的是模型的参数规模，并不是最终生成的向量维度。写入向量存储时使用的 `vectorDimensions`，仍然需要与所选 Embedding 模型实际输出的向量维度保持一致。

云端模型通常接入更简单，不需要自行维护推理服务；本地部署则可以让数据保留在自己的环境中，但需要自行准备计算资源，并负责模型服务的部署和维护。

实际使用时，可以根据语言支持、向量维度、检索效果、调用成本和部署条件选择合适的 Embedding 模型。

### 向量维度

可以先从二维向量理解“维度”这个概念。

例如：

```text
[0.12, -0.37]
```
这是一个二维向量，因为它包含两个数值。可以把它想象成二维坐标系中的一个点：0.12 是第一个方向上的坐标，-0.37 是第二个方向上的坐标。

如果向量中有三个数值：

```text
[0.12, -0.37, 0.81]
```
它就是三维向量，可以把它理解成三维空间中的一个点。

按照同样的方式，下面这个向量包含四个数值，因此是四维向量：

```text
[0.12, -0.37, 0.81, 0.25]
```

二维和三维向量可以在坐标系中直观地表示出来，但 Embedding 模型生成的向量通常包含数百个甚至数千个数值，已经无法直接用图形展示。

例如，一个模型输出 1536 维向量，意味着每段文本都会被转换成由 1536 个数值组成的向量：

```text
[0.12, -0.37, 0.81, ..., 0.25]
```

其中，每个数值都可以看作文本在某个向量方向上的坐标。这些值通常是正数、负数或者接近零的小数，由 Embedding 模型根据文本内容计算得出。

需要注意的是，某一个数值通常没有能够单独解释的固定含义。不能简单地认为第一维表示“主题”、第二维表示“情感”，或者某个数值越大就代表某种语义越强。文本的语义是由整个向量中的所有维度共同表达的。

当两段文本的意思比较接近时，Embedding 模型通常会让它们在高维向量空间中的位置也比较接近。系统再通过余弦相似度、点积或欧氏距离等方式比较两个向量，从而找到语义相关的内容。

向量维度还会影响存储空间和检索计算量。维度越高，每条记录需要保存的数值越多，计算向量相似度时需要处理的数据也越多。

不过，向量维度越高并不代表检索效果一定越好。实际效果主要取决于 Embedding 模型本身的训练质量，以及该模型是否适合当前的语言和业务场景。

> **注意：**向量维度不是随意设置的，它由所使用的 Embedding 模型及其输出配置决定。不同模型生成的向量维度可能不同；部分模型还允许在调用时指定输出维度。向量存储中配置的维度必须与模型实际生成的向量维度保持一致，否则可能无法创建集合或写入数据。同一个集合中的向量通常也必须具有相同的维度。

### 创建 InMemoryVectorStore

```csharp
VectorStore vectorStore = new InMemoryVectorStore(
    new InMemoryVectorStoreOptions
    {
        EmbeddingGenerator = aiProjectClient
            .GetProjectOpenAIClient()
            .GetEmbeddingClient(embeddingDeploymentName)
            .AsIEmbeddingGenerator()
    });
```

EmbeddingGenerator 负责将聊天消息转换成向量，InMemoryVectorStore 则负责在内存中保存并搜索这些向量。

这里使用 In-Memory 主要是为了简化示例。应用进程退出后，保存在内存中的数据也会消失，因此它只能说明 ChatHistoryMemoryProvider 是如何工作的，不能提供生产环境所需要的持久化能力。

在实际项目中，可以根据需要替换为 Qdrant、Postgres、Redis、Azure AI Search 或其他支持持久化的向量存储实现。

## 配置存储范围和搜索范围

创建向量存储后，就可以配置 ChatHistoryMemoryProvider。

```csharp
AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());
// 创建一个向量存储来存储聊天消息。
// 为了演示目的，我们使用内存中的向量存储。
// 将其替换为您选择的可以长期保存聊天历史记录的向量存储实现。

VectorStore vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions()
{
    EmbeddingGenerator = aiProjectClient
        .GetProjectOpenAIClient()
        .GetEmbeddingClient(embeddingDeploymentName)
        .AsIEmbeddingGenerator()
});
var memoryOptions = new ChatHistoryMemoryProviderOptions
{
    MaxResults = 5,

    // 仅用于本地调试。
    // 开启后，日志中会显示用户 ID、搜索文本和检索结果。
    EnableSensitiveTelemetryData = true,

    ContextPrompt =
        "下面是从该用户过去的对话中检索到的信息。" +
        "回答当前问题时，请优先使用其中明确表达的用户偏好："
};

// 创建代理并添加 ChatHistoryMemoryProvider 以将聊天消息存储在向量存储中。
AIAgent agent = aiProjectClient
    .AsAIAgent(new ChatClientAgentOptions
    {

        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "你是一个擅长讲笑话的助手。"
        },
        Name = "Joker",

        AIContextProviders = [new ChatHistoryMemoryProvider(
            vectorStore,
            collectionName: "chathistory",
            vectorDimensions: 3072,
            // 回调以配置 ChatHistoryMemoryProvider 的初始状态。
            // ChatHistoryMemoryProvider 将其状态存储在 AgentSession 中，并且每当ChatHistoryMemoryProvider
            // 无法在会话中找到现有状态时，将调用此回调，
            // 通常是在与新会话首次使用时。
            session => new ChatHistoryMemoryProvider.State(
                // 配置聊天消息将被存储的范围值。
                // 在这种情况下，我们使用固定的用户ID和每个新会话的唯一会话ID。
                storageScope: new() { UserId = "UID1", SessionId = Guid.NewGuid().ToString() },
                // 配置将用于搜索相关先前消息的范围。
                // 在这种情况下，我们正在搜索该用户在所有会话中的任何消息。
                searchScope: new() { UserId = "UID1" }),
               options: memoryOptions,
               loggerFactory: loggerFactory
            )]
    });

```

这个示例中，collectionName指定聊天历史保存到名为chathistory的集合中。vectorDimensions 表示 Embedding 向量的维度，这个值需要与所使用的Embedding 模型保持一致。

更值得关注的是 storageScope 和 searchScope。

storageScope 决定消息以什么范围写入。示例同时保存了 UserId 和 SessionId，因此每条消息不仅能够知道属于哪个用户，也能够知道来自哪一次会话。

```csharp
storageScope: new()
{
    UserId = "UID1",
    SessionId = Guid.NewGuid().ToString()
}
```
searchScope 决定后续查询时允许搜索哪些消息。这里仅指定了 UserId，没有指定 SessionId。

```csharp
searchScope: new()
{
    UserId = "UID1"
}
```

这意味着消息在写入时仍然按照不同 Session 进行区分，但在搜索时，只要消息属于同一个用户，就可以被检索出来。

换句话说，写入时记录的是“这个用户在哪次会话中说了什么”，查询时关心的则是“这个用户以前说过什么”。

这也是跨会话记忆能够产生的关键。

## 跨会话记忆是如何产生的

首先创建第一个会话：

```csharp
AgentSession session = await agent.CreateSessionAsync();
```

用户在这个会话中告诉 Agent，自己喜欢发生在茶馆里的笑话：

```csharp
await agent.RunAsync(
    "我喜欢程序员的笑话，给我讲一个。",
    session);
```

调用完成后，用户请求和 Agent 的响应会被写入向量存储，并标记为属于用户 UID1。

随后创建一个全新的会话：

```csharp
AgentSession session2 = await agent.CreateSessionAsync();
```

新的 AgentSession 本身并不包含第一个会话中的聊天历史。用户在新会话中提出：

```csharp
await agent.RunAsync(
    "给我讲一个我喜欢的笑话。",
    session2);
```

这句话没有直接提到程序员，但 ChatHistoryMemoryProvider 会根据当前请求执行语义检索。由于搜索范围只限定了 UserId，它可以搜索用户 UID1 在其他 Session 中留下的消息。

此前的这条消息：

> 我喜欢程序员的笑话，给我讲一个。

模型得到用户过去的偏好后，就有机会继续讲一个发生在茶馆里的笑话。

这里并不是新创建的 AgentSession 自动继承了旧会话，而是 ChatHistoryMemoryProvider 从外部向量存储中找回了相关消息。


## 总结

ChatHistoryMemoryProvider 的作用，是把聊天历史保存到向量存储中，并在后续请求中找回与当前问题相关的消息。

它依赖 Microsoft.Extensions.VectorData.Abstractions 提供的统一向量存储接口，因此底层可以使用 InMemoryVectorStore，也可以替换成 Qdrant、Postgres、Redis、Azure AI Search 等持久化实现。

在配置过程中，storageScope 决定消息如何保存，searchScope 决定消息如何被搜索。当写入范围包含 SessionId，而搜索范围只限定 UserId 时，Agent 就可以在保留会话边界的同时，检索同一个用户在其他会话中留下的信息。