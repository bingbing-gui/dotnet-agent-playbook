# Semantic Kernel 中向量存储

向量数据库在自然语言处理（NLP）、计算机视觉（CV）、推荐系统（RS）等需要语义理解与匹配的数据应用场景中有广泛用途。

---

## 💡 应用场景：辅助大语言模型生成更优内容

其中一个使用向量数据库的典型场景是：**帮助大语言模型（LLMs）生成更相关、更连贯的回答**。

大语言模型常面临以下问题：

- 生成不准确或无关的信息
- 缺乏事实一致性或常识
- 重复或自相矛盾
- 出现偏见或冒犯性内容

为了解决这些问题，可以使用向量数据库存储与你目标领域或内容相关的：

- 主题
- 关键词
- 事实
- 观点
- 信息来源等

向量数据库能高效检索出与特定问题相关的信息子集，并将其与提示词（prompt）一同传递给大语言模型，生成更准确、更具上下文的回答。

---

## ✍️ 示例

> 如果你想写一篇关于“AI 最新趋势”的博客文章，可以先将与该主题相关的最新资讯存入向量数据库。  
> 然后将这些信息与生成请求一并发送给大语言模型，就能生成内容丰富、基于最新资料的文章。

---

## 🧠 Semantic Kernel 与 .NET 的支持

Semantic Kernel 和 .NET 提供了与向量存储交互的抽象接口，并内置了一系列适配多种数据库的实现，功能包括：

- 创建、列出、删除向量集合（Collections）
- 上传、读取、删除向量记录（Records）

通过这些抽象接口，开发者可以先使用免费或本地向量存储进行开发测试，再无缝切换至云端服务进行扩展，无需重构代码。

---

## 使用向量存储实现 RAG（Retrieval Augmented Generation）

向量存储抽象层是一个底层 API，用于向向量数据库中**添加和检索数据**。Semantic Kernel 原生支持通过任何一种向量存储实现来完成 RAG（Retrieval Augmented Generation）工作流。这是通过将 `IVectorSearchable<TRecord>` 包装为一个**文本搜索（Text Search）**实现来实现的。

---

## 向量存储抽象层（Vector Store Abstraction）

向量存储的抽象接口由 NuGet 包 [`Microsoft.Extensions.VectorData.Abstractions`](https://www.nuget.org/packages/Microsoft.Extensions.VectorData.Abstractions) 提供。该包定义了与向量数据库交互的核心基类和接口，便于实现统一的存储与检索逻辑。

---

## 📦 主要抽象类与接口

### `Microsoft.Extensions.VectorData.VectorStore`

- 表示整个向量存储系统的入口，对应一个数据库实例，包含若干集合。
- 提供跨集合（Collection）的操作，例如：
  - `ListCollectionNames()`：列出所有集合名称
- 也可以用来获取指定集合的实例：
  - `VectorStoreCollection<TKey, TRecord>`

---

### `Microsoft.Extensions.VectorData.VectorStoreCollection<TKey, TRecord>`

`VectorStoreCollection<TKey, TRecord>` 表示一个向量集合（Collection）。这个抽象基类提供了用于检查集合是否存在、创建集合或删除集合的方法。

此外，该基类还提供了插入或更新（Upsert）、获取（Get）以及删除（Delete）记录的方法。

最后，该抽象基类继承自 `IVectorSearchable<TRecord>` 接口，从而具备了向量搜索功能。

---

### `Microsoft.Extensions.VectorData.IVectorSearchable<TRecord>`

此接口定义了基于向量的搜索操作，核心方法：

```csharp
Task<IList<TRecord>> SearchAsync(...);
```

---

## Semantic Kernel支持的Vector Store Connectors

每种向量数据库的具体实现都位于其各自独立的 NuGet 包中。

Semantic Kernel 支持的向量数据库连接器一览（Vector Store Connectors）

| 向量数据库连接器              | 支持 C# | 使用官方 SDK | 维护者 / 提供方                        |
|------------------------------|---------|---------------|----------------------------------------|
| Azure AI Search              | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Cosmos DB MongoDB（vCore）   | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Cosmos DB NoSQL              | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Couchbase                    | ✅      | ✅            | Couchbase                              |
| Elasticsearch                | ✅      | ✅            | Elastic                                |
| Chroma                       | 🚧 计划中 | -             | -                                      |
| In-Memory                    | ✅      | N/A           | Microsoft Semantic Kernel Project      |
| Milvus                       | 🚧 计划中 | -             | -                                      |
| MongoDB                     | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Neon Serverless Postgres     | ✅（通过 Postgres） | ✅ | Microsoft Semantic Kernel Project      |
| Pinecone                     | ✅      | ❌            | Microsoft Semantic Kernel Project      |
| Postgres                     | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Qdrant                       | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Redis                        | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| SQL Server                   | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| SQLite                       | ✅      | ✅            | Microsoft Semantic Kernel Project      |
| Volatile（内存模式，已废弃） | ❌ 已废弃 | N/A           | Microsoft Semantic Kernel Project      |
| Weaviate                     | ✅      | ✅            | Microsoft Semantic Kernel Project      |

---

✅ 表示支持，❌ 表示不支持，🚧 表示开发中，N/A 表示不适用。

> ℹ️ **说明：**
> - 大多数连接器由 Microsoft Semantic Kernel 官方维护。
> - Pinecone 尽管支持 C#，但当前并未使用官方 SDK。
> - Volatile 模式已废弃，请改用新的 In-Memory 实现。

## 使用Postgres Vector Store connector

### 安装 NuGet 包

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.PgVector --prerelease
```

### 在 ASP.NET Core 中通过依赖注入注册

```csharp
builder.Services.AddPostgresVectorStore(
    builder.Configuration.GetConnectionString("Postgres")!
);

/*
这里我们使用AzureOpenAI模型把数据的向量
这里需要在配置文件中配置模型部署的信息
  "Embedding": {
    "ApiKey": "",
    "DeploymentName": "",
    "Endpoint": ""
  }
*/
builder.Services.Configure<AzureEmbeddingOptions>(
    builder.Configuration.GetSection("Embedding"));

builder.Services.AddSingleton(sp =>
{
    var endpoint = builder.Configuration["Embedding:Endpoint"]
        ?? throw new InvalidOperationException("Missing AzureOpenAI:Endpoint");
    var key = builder.Configuration["Embedding:ApiKey"]
        ?? throw new InvalidOperationException("Missing AzureOpenAI:ApiKey");
    return new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(key));
});
```

### 数据映射模型
默认映射规则：类属性名 ↔ 数据库列名
可通过 StorageName 特性覆盖属性名
```csharp
using Microsoft.Extensions.VectorData;

  public class Hotel
  {
      [VectorStoreKey(StorageName = "hotel_id")]
      public int HotelId { get; set; }

      [VectorStoreData(StorageName = "hotel_name")]
      public string? HotelName { get; set; }

      [VectorStoreData(StorageName = "hotel_description")]
      public string? Description { get; set; }

      [VectorStoreVector(Dimensions: EmbeddingDims.Description, DistanceFunction = DistanceFunction.CosineSimilarity, StorageName = "description_embedding")]
      public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
  }
```

### VectorStoreVectorAttribute 参数说明

| 参数            | 必须 | 描述                                                         |
|-----------------|------|--------------------------------------------------------------|
| `Dimensions`    | ✅   | 向量的维度数。为集合创建向量索引时必须指定。                  |
| `IndexKind`     | ❌   | 向量索引的类型。默认值由具体的 Vector Store 类型决定。        |
| `DistanceFunction` | ❌ | 向量检索时使用的相似度度量函数。默认值由具体的 Vector Store 类型决定。 |
| `StorageName`   | ❌   | 为数据库中列提供替代名称（并非所有连接器都支持）。            |

---

### 创建表

这一步需要我们手动在Postgres中创建存储数据的表，首先要安装pgvector插件，具体安装方法请看上一节内容

```csharp
CREATE TABLE public.hotels (
    hotel_id INTEGER NOT NULL,
    hotel_name TEXT,
    hotel_description TEXT,
    description_embedding VECTOR(1536),
    PRIMARY KEY ("hotel_id")
);
```

### 演示效果

创建酒店

![酒店向量存储示意图](/docs/智能体开发框架/SemanticKernel/Materials/create-query.png)

酒店向量查询

![酒店向量存储示意图](/docs/智能体开发框架/SemanticKernel/Materials/hotel-query.png)

---

## 📂 示例项目源码

查看完整的示例项目与代码实现：

[https://github.com/bingbing-gui/aspnetcore-developer/tree/master/src/09-AI-Agent/SemanticKernel/SK.VectorStores](https://github.com/bingbing-gui/aspnetcore-developer/tree/master/src/09-AI-Agent/SemanticKernel/SK.VectorStores)

## ✅ 总结

通过 .NET + Semantic Kernel + 向量存储 的组合，你可以快速实现一个完整的 RAG 系统，支持：

- 使用模型注解方式定义数据结构
- 高效进行向量插入、检索、删除
- 与 AI 模型自然集成（如 OpenAI、Azure OpenAI）
