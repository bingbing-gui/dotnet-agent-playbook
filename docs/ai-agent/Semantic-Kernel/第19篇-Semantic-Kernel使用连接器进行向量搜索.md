
Semantic Kernel 提供了向量存储（Vector Store）抽象层中的向量搜索功能，支持多种选项如过滤、分页等。本文将详细介绍其用法。

## 向量搜索（Vector Search）

`SearchAsync` 方法允许基于已向量化的数据进行搜索。该方法接收一个向量和可选的 `VectorSearchOptions<TRecord>` 作为输入，适用于以下类型：

- `IVectorSearchable<TRecord>`
- `VectorStoreCollection<TKey, TRecord>`

> **注意**：`VectorStoreCollection<TKey, TRecord>` 实现了 `IVectorSearchable<TRecord>`。

如下是VectorStoreCollection类的定义:

```csharp
public abstract class VectorStoreCollection<TKey, TRecord> : IVectorSearchable<TRecord>, IDisposable
    where TKey : notnull
    where TRecord : class
{
    .....
}
```
---

## 支持的向量类型

`SearchAsync` 的向量参数为泛型类型。每个数据存储支持的向量类型不同，请参阅各连接器文档。

> ⚠️ 搜索向量类型必须与目标向量类型一致。例如，同一条记录中有两个不同类型的向量，需确保提供的搜索向量与目标向量类型匹配。若有多个向量，可通过 `VectorProperty` 指定目标。

---

## 向量搜索选项

通过 `VectorSearchOptions<TRecord>` 可配置以下选项：

### VectorProperty

指定要搜索的向量属性。如果未指定且模型仅包含一个向量，则使用该向量。若没有或有多个向量而未指定，则会抛出异常。

```csharp
 public async IAsyncEnumerable<(Hotel Record, double? Score)> SearchByTextAsync(
     string query, int topK = 5, CancellationToken ct = default)
 {
     var queryVector = await _emb.CreateAsync(query, ct);

     var col = GetCollection();

     var options = new VectorSearchOptions<Hotel>
     {
         //Filter = h => h.HotelName == "Tokyo",
         VectorProperty = h => h.DescriptionEmbedding,
         Skip = 0,
         IncludeVectors = false
     };

     await foreach (var r in col.SearchAsync(queryVector, topK, options, ct))
     {
         yield return (r.Record, r.Score);
     }
 }
```
---

### Top 和 Skip

用于分页。

- **Top**：返回前 N 条结果
- **Skip**：跳过前 N 条结果

```csharp
var vectorSearchOptions = new VectorSearchOptions<Product>
{
    Skip = 40
};

var searchResult = collection.SearchAsync(searchVector, top: 20, vectorSearchOptions);
```

---

### IncludeVectors

指定是否返回结果中的向量属性。

- 默认值：`false`（节省带宽与处理成本）
- 若为 `true`，则返回完整向量数据

```csharp
var vectorSearchOptions = new VectorSearchOptions<Product>
{
    IncludeVectors = true
};
```

---

### Filter

用于在向量搜索前先对记录进行过滤。

**好处：**

- 降低延迟和计算开销
- 用于访问控制，过滤掉用户无权限的数据

> ⚠️ 很多存储需要字段设置为 `IsIndexed = true` 才能参与过滤。

```csharp
 public async IAsyncEnumerable<(Hotel Record, double? Score)> SearchByTextAsync(
     string query, int topK = 5, CancellationToken ct = default)
 {
     var queryVector = await _emb.CreateAsync(query, ct);

     var col = GetCollection();

     var options = new VectorSearchOptions<Hotel>
     {
         Filter = h => h.HotelName == "Tokyo",
         VectorProperty = h => h.DescriptionEmbedding,
         Skip = 0,
         IncludeVectors = false
     };

     await foreach (var r in col.SearchAsync(queryVector, topK, options, ct))
     {
         yield return (r.Record, r.Score);
     }
 }
```

---

## 小结

Semantic Kernel 的 Vector Store 连接器提供了强大的向量搜索功能：

- **SearchAsync**：执行相似度搜索
- **VectorProperty**：选择目标向量
- **Top / Skip**：支持分页
- **IncludeVectors**：是否返回向量
- **Filter**：先过滤后搜索，提高性能和安全性

这些功能让你能够在不同存储（如 InMemory、Qdrant 等）上轻松实现向量化搜索和检索。