
Semantic Kernel 在其向量存储抽象中提供了混合搜索能力。该功能支持过滤器以及更多选项，本文将详细说明。
> 目前支持的混合搜索类型是 **向量搜索 + 关键词搜索**，两者并行执行，然后返回结果集的并集。  
> 暂时不支持稀疏向量的混合搜索。
> ⚠️ **注意**：当前版本中 Postgres 连接器不支持混合搜索，未来可能会添加该功能。

## 数据库模式要求

要执行混合搜索，数据库模式需要包含：

- 一个向量字段
- 一个启用了全文检索能力的字符串字段

如果你使用 Semantic Kernel 的向量存储连接器创建集合，请确保在需要做关键词搜索的字符串字段上启用了 `IsFullTextIndexed` 选项。

> 💡 **提示**：更多关于启用 `IsFullTextIndexed` 的信息，请参考 `VectorStoreDataAttribute` 参数或 `VectorStoreDataProperty` 配置设置。

---

## 混合搜索

`HybridSearchAsync` 方法允许你同时使用一个向量和一个字符串关键词集合来进行搜索。它还可以接受一个可选的 `HybridSearchOptions<TRecord>` 类作为输入。该方法定义在接口：

```
IKeywordHybridSearchable<TRecord>
```

只有那些当前支持“向量+关键词混合搜索”的数据库连接器实现了这个接口。

假设你已经有一个包含数据的集合，就可以轻松执行混合搜索。以下是一个向量数据库 Qdrant 示例。

### C# 示例

```csharp
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;

// 生成Embedding的占位方法
async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string textToVectorize)
{
    // 你的Embedding逻辑
}

// 创建 Qdrant VectorStore，并选择一个已有的集合
VectorStore vectorStore = new QdrantVectorStore(new QdrantClient("localhost"), ownsClient: true);
IKeywordHybridSearchable<Hotel> collection = (IKeywordHybridSearchable<Hotel>)vectorStore.GetCollection<ulong, Hotel>("skhotels");

// 生成搜索向量
ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync("I'm looking for a hotel where customer happiness is the priority.");

// 执行混合搜索，限制只返回一个结果
var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 1);

// 输出搜索结果
await foreach (var record in searchResult)
{
    Console.WriteLine("Found hotel description: " + record.Record.Description);
    Console.WriteLine("Found record score: " + record.Score);
}
```

> 💡 **提示**：关于如何生成 Embeddings，请参考 embedding generation 文档。

---

## 支持的向量类型

`HybridSearchAsync` 方法接收一个泛型参数作为搜索向量。不同数据存储支持的向量类型可能不同。

⚠️ **注意**：搜索向量类型必须和目标字段中的向量类型一致。例如，如果一条记录有两个不同类型的向量字段，必须确保提供的搜索向量与目标字段匹配。

可使用 `VectorProperty` 和 `AdditionalProperty` 来指定目标向量或目标全文字段。

---

## 混合搜索选项

通过 `HybridSearchOptions<TRecord>` 可以配置以下内容：

### 1. VectorProperty 和 AdditionalProperty

- **VectorProperty**：指定要搜索的向量字段
- **AdditionalProperty**：指定要搜索的全文检索字段

如果未指定：

- 向量字段只有一个 → 默认使用该字段
- 没有或存在多个 → 抛出异常
- 全文字段只有一个 → 默认使用该字段
- 没有或存在多个 → 抛出异常

**示例：**

```csharp
var hybridSearchOptions = new HybridSearchOptions<Product>
{
    VectorProperty = r => r.DescriptionEmbedding,
    AdditionalProperty = r => r.Description
};

var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 3, hybridSearchOptions);
```

**数据模型：**

```csharp
public sealed class Product
{
    [VectorStoreKey]
    public int Key { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Name { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Description { get; set; }

    [VectorStoreData]
    public List<string> FeatureList { get; set; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> DescriptionEmbedding { get; set; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> FeatureListEmbedding { get; set; }
}
```

---

### Top 和 Skip

- **Top**：返回前 n 条结果
- **Skip**：跳过前 n 条结果

可用于分页。

```csharp
var hybridSearchOptions = new HybridSearchOptions<Product>
{
    Skip = 40
};

var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 20, hybridSearchOptions);
```

---

### IncludeVectors

- 是否返回向量字段
- 默认 `false`（提高性能，减少数据传输量）

```csharp
var hybridSearchOptions = new HybridSearchOptions<Product>
{
    IncludeVectors = true
};
```

---

### 4. Filter

- 可以在搜索前对集合进行过滤

**优点：**

- 减少延迟与开销
- 用于权限控制

⚠️ **注意**：某些向量数据库要求字段必须先建立索引才能用于过滤。  
在数据模型上，可以通过 `IsFilterable` 或 `IsIndexed` 来开启。

**示例：**

```csharp
var hybridSearchOptions = new HybridSearchOptions<Glossary>
{
    Filter = r => r.Category == "External Definitions" && r.Tags.Contains("memory")
};
```

**数据模型：**

```csharp
sealed class Glossary
{
    [VectorStoreKey]
    public ulong Key { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public string Category { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public List<string> Tags { get; set; }

    [VectorStoreData]
    public string Term { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Definition { get; set; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}
```

---

## 总结

👉 `HybridSearchAsync` 允许你结合 **向量搜索** 和 **关键词搜索**，支持分页（Top/Skip）、返回向量（IncludeVectors）、字段过滤（Filter），并可通过 `VectorProperty`/`AdditionalProperty` 精确指定目标字段。