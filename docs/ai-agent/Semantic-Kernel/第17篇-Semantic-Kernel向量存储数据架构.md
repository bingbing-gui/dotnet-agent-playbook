Semantic Kernel 向量存储数据架

## 向量存储核心组件

在 Semantic Kernel 中，向量存储抽象包含三个主要组件：

- **向量存储（Vector Store）**：对应一个数据库实例。
- **集合（Collection）**：一组记录的集合，包括查询或筛选这些记录所需的索引。
- **记录（Record）**：数据库中的单个数据条目。

---

### 不同数据库中的集合

集合在不同数据库中的实现方式不同，取决于数据库如何组织和索引记录。  
大多数数据库都具备“记录集合”的概念，与向量存储抽象中的集合天然映射，但底层数据库未必称为“集合（collection）”。

---

### Semantic Kernel 向量存储与 Postgres 映射关系

| Semantic Kernel 抽象 | 概念解释 | 在 Postgres 中的对应 |
|----------------------|----------|----------------------|
| **Vector Store**     | 整个向量存储，包含多个集合 | 数据库实例（如 postgres 或 mydb） |
| **Collection**       | 一组记录的集合，带有索引，可查询/过滤 | 表（Table）（如 Hotels、Profiles） |
| **Record**           | 集合中的单条数据，包含键、普通字段、向量字段 | 表中的一行（Row），字段对应列，向量字段对应 vector 列 |

## 数据模型

Semantic Kernel 的向量存储连接器采用模型优先（model first）方式与数据库交互。  
插入或获取记录的方法依赖强类型模型类，属性通过特性（attributes）标注用途。

示例模型：

```csharp
using Microsoft.Extensions.VectorData;

public class Hotel
{
    [VectorStoreKey]
    public ulong HotelId { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public string HotelName { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Description { get; set; }

    [VectorStoreVector(Dimensions: 4, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
    public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }

    [VectorStoreData(IsIndexed = true)]
    public string[] Tags { get; set; }
}
```

### VectorStoreKey 特性

- 标识属性为实体主键。
- `StorageName`（可选）为数据库列提供替代名称，属性名与字段名可不同。
- 注意：并非所有连接器都支持该参数，部分场景可用 `JsonPropertyNameAttribute` 替代。

### VectorStoreData 特性

- 标识属性为普通数据字段（非主键、非向量）。
- 可选参数：
  - **IsIndexed**：是否建立索引，加速查询和筛选，默认 `false`。
  - **IsFullTextIndexed**：是否建立全文索引，用于模糊或关键字搜索，默认 `false`，仅在数据库支持时有效。
  - **StorageName**：数据库列替代名称，部分连接器支持。

### VectorStoreVector 特性

- 标识属性包含向量。

示例：

```csharp
[VectorStoreVector(Dimensions: 4, DistanceFunction = DistanceFunction.CosineSimilarity, IndexKind = IndexKind.Hnsw)]
public ReadOnlyMemory<float>? DescriptionEmbedding { get; set; }
```

参数说明：

- **Dimensions（必填）**：向量维度数，创建索引时必须指定。
- **IndexKind（可选）**：向量索引类型，默认值依赖具体实现。
- **DistanceFunction（可选）**：向量搜索时的相似度函数，如余弦相似度、欧氏距离等。
- **StorageName（可选）**：数据库列替代名称，部分连接器支持。

常见索引类型（IndexKind）和距离函数（DistanceFunction）由 `Microsoft.SemanticKernel.Data.IndexKind` 和 `Microsoft.SemanticKernel.Data.DistanceFunction` 提供静态值。  
部分数据库支持特殊索引或距离函数，具体实现可能有额外类型。

## 拓展

`Microsoft.SemanticKernel.Data.IndexKind` 和 `Microsoft.SemanticKernel.Data.DistanceFunction` 说明如下：

`IndexKind` 类定义：

```csharp
public static class IndexKind
{
    /// <summary>
    /// Hierarchical Navigable Small World（HNSW），用于执行近似最近邻（ANN）搜索。
    /// </summary>
    /// <remarks>
    /// 这种搜索的准确率低于穷举的 k 最近邻搜索，但速度更快、效率更高。
    /// </remarks>
    public const string Hnsw = nameof(Hnsw);

    /// <summary>
    /// 指定使用暴力搜索来查找最近邻。
    /// </summary>
    /// <remarks>
    /// 这种搜索会计算所有数据点之间的距离，因此其时间复杂度是线性的，并且与数据点数量成正比。  
    /// 在某些数据库中，它也被称为“穷举 k 最近邻”。  
    /// 这种搜索具有很高的召回准确率，但比 HNSW 更慢、开销更大。  
    /// 更适合小型数据集。
    /// </remarks>
    public const string Flat = nameof(Flat);

    /// <summary>
    /// 指定使用倒排文件（Inverted File）加扁平压缩的索引。
    /// </summary>
    /// <remarks>
    /// 这种搜索通过使用邻居分区或聚类来缩小搜索范围，从而提高检索效率。  
    /// 也被称为近似最近邻（ANN）搜索。
    /// </remarks>
    public const string IvfFlat = nameof(IvfFlat);

    /// <summary>
    /// 指定基于磁盘的近似最近邻（DiskANN）算法，
    /// 该算法专为在高维空间中高效搜索近似最近邻而设计。
    /// </summary>
    /// <remarks>
    /// DiskANN 的主要目标是处理无法完全放入内存的大规模数据集，  
    /// 它利用磁盘存储来保存数据，同时保持较快的搜索速度。
    /// </remarks>
    public const string DiskAnn = nameof(DiskAnn);

    /// <summary>
    /// 指定一种索引，它使用基于 DiskANN 的量化方法对向量进行压缩，
    /// 以提高 kNN 搜索的效率。
    /// </summary>
    public const string QuantizedFlat = nameof(QuantizedFlat);

    /// <summary>
    /// 指定一种动态索引，它会在 <see cref="Flat"/> 和 <see cref="Hnsw"/> 索引之间自动切换。
    /// </summary>
    public const string Dynamic = nameof(Dynamic);
}
```

`DistanceFunction`类定义

```csharp
public static class DistanceFunction
{
    /// <summary>
    /// 指定用于计算两个向量之间余弦（角度）相似度的函数。
    /// </summary>
    /// <remarks>
    /// 余弦相似度只衡量两个向量的夹角，不考虑向量的长度。  
    /// CosineSimilarity = 1 - CosineDistance。  
    /// -1 表示向量方向相反。  
    /// 0 表示向量正交。  
    /// 1 表示向量完全相同。
    /// </remarks>
    public const string CosineSimilarity = nameof(CosineSimilarity);

    /// <summary>
    /// 指定用于计算两个向量之间余弦（角度）距离的函数。
    /// </summary>
    /// <remarks>
    /// CosineDistance = 1 - CosineSimilarity。  
    /// 2 表示向量方向相反。  
    /// 1 表示向量正交。  
    /// 0 表示向量完全相同。
    /// </remarks>
    public const string CosineDistance = nameof(CosineDistance);

    /// <summary>
    /// 指定点积相似度函数，同时考虑向量的长度和夹角。
    /// </summary>
    /// <remarks>
    /// 值越大，表示两个向量越相似。
    /// </remarks>
    public const string DotProductSimilarity = nameof(DotProductSimilarity);

    /// <summary>
    /// 指定负点积相似度函数，同时考虑向量的长度和夹角。
    /// </summary>
    /// <remarks>
    /// NegativeDotProduct = -1 * DotProductSimilarity。  
    /// 值越大，表示向量之间的距离越远，相似度越低。
    /// </remarks>
    public const string NegativeDotProductSimilarity = nameof(NegativeDotProductSimilarity);

    /// <summary>
    /// 指定欧几里得距离函数，用于计算两个向量之间的直线距离。
    /// </summary>
    /// <remarks>
    /// 也称为 l2 范数。
    /// </remarks>
    public const string EuclideanDistance = nameof(EuclideanDistance);

    /// <summary>
    /// 指定欧几里得平方距离函数。
    /// </summary>
    /// <remarks>
    /// 也称为 l2-squared。
    /// </remarks>
    public const string EuclideanSquaredDistance = nameof(EuclideanSquaredDistance);

    /// <summary>
    /// 指定汉明距离函数，用于衡量两个向量在各维度上的差异个数。
    /// </summary>
    public const string HammingDistance = nameof(HammingDistance);

    /// <summary>
    /// 指定曼哈顿距离函数，计算两个向量各维度差值的绝对值之和。
    /// </summary>
    public const string ManhattanDistance = nameof(ManhattanDistance);
}

```

## 总结

我们这节主要介绍了向量存储中的核心组件`向量存储（Vector Store）`和`集合（Collection）`和`记录（Record）`
以及数据模型中使用的特性作用，以及各个特性中有那些参数以及作用。



