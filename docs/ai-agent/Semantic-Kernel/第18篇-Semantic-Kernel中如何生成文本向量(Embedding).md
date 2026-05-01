
在前面介绍 **SK 和 Postgres 向量** 的示例中，我们使用了 `AzureOpenAIClient` 类来生成 Embedding。  
实际上，**Semantic Kernel** 为我们提供了更方便的接口来处理文本向量化。

---

## 接口说明

> **注意：官方文档更新存在滞后**

- **已废弃接口**  
  `ITextEmbeddingGenerationService` 定义在 **Microsoft.SemanticKernel.Abstractions** 包中。  

- **推荐使用的新接口**  
  `IEmbeddingGenerator<in TInput, TEmbedding>` 定义在 **Microsoft.Extensions.AI.Abstractions** 包中。  

通过 **SK 的扩展方法** 注册一个 Embedding 生成器后，会自动把  
`IEmbeddingGenerator<string, Embedding<float>>` 加入到依赖注入（DI）容器里。

---

## 如何配置这个接口

### 使用 Azure OpenAI

下面是官方文档提供的示例（版本较老，文档尚未更新）：
```csharp
#pragma warning disable SKEXP0010
builder.Services.AddAzureOpenAITextEmbeddingGeneration(
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT", // 部署名称，例如 "text-embedding-ada-002"
    endpoint: "YOUR_AZURE_ENDPOINT",           // Azure OpenAI 服务地址，例如 https://myaiservice.openai.azure.com
    apiKey: "YOUR_API_KEY",
    modelId: "MODEL_ID",          // 可选：底层模型名称（当部署名和模型名不一致时）
    serviceId: "YOUR_SERVICE_ID", // 可选：用于在 Semantic Kernel 中定位特定服务
    dimensions: 1536              // 可选：生成向量的维度
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```

推荐写法（更简洁现代）：

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var builder = WebApplication.CreateBuilder(args);

// 注册 OpenAI Embedding 服务
builder.Services.AddOpenAITextEmbeddingGeneration(
    modelId: "text-embedding-3-small", // 选择的 OpenAI 模型
    apiKey: builder.Configuration["OpenAI:ApiKey"] // 从配置读取 API Key
);
```

## 在业务服务中使用 IEmbeddingGenerator

在完成前面步骤的注册后，就可以在业务服务类中直接使用 IEmbeddingGenerator。

```csharp
using Azure;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using SK.TextEmbeddingGeneration.Models;

namespace SK.TextEmbeddingGeneration.Services
{
    public sealed class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        //private readonly AzureOpenAIClient _client;
        //private readonly ITextEmbeddingGenerationService _textEmbeddingGenerationService;

        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
        private readonly string _deploymentName;
        private readonly int _expectedDim;

        public AzureOpenAIEmbeddingService(
            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
            IOptions<AzureEmbeddingOptions> options)
        {
            _embeddingGenerator = embeddingGenerator ?? throw new ArgumentException("");
            var opt = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(opt.DeploymentName))
                throw new ArgumentException("Embedding deployment name is required.", nameof(options));

            if (opt.ExpectedDimension <= 0)
                throw new ArgumentException("ExpectedDimension must be a positive integer.", nameof(options));

            _deploymentName = opt.DeploymentName;
            _expectedDim = opt.ExpectedDimension;
        }

        public async Task<ReadOnlyMemory<float>> CreateAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text to embed cannot be null or empty.", nameof(text));

            try
            {
                // 旧写法（已注释）
                // EmbeddingClient embClient = _client.GetEmbeddingClient(_deploymentName);
                // var clientResult = await embClient.GenerateEmbeddingAsync(text, cancellationToken: ct);
                // var vec = clientResult.Value.ToFloats();

                // 新写法（推荐）
                var vec = await _embeddingGenerator.GenerateVectorAsync(text);

                // 维度校验（强烈推荐，避免与 pgvector 列定义不一致）
                if (vec.Length != _expectedDim)
                {
                    throw new InvalidOperationException(
                        $"Embedding dimension mismatch. Expected: {_expectedDim}, Actual: {vec.Length}. " +
                        $"Make sure your model and database column (VECTOR({_expectedDim})) match.");
                }

                return vec;
            }
            catch (RequestFailedException ex)
            {
                // Azure SDK 的标准异常，抛出更友好的信息
                throw new InvalidOperationException($"Failed to generate embedding from Azure OpenAI: {ex.Message}", ex);
            }
        }
    }
}
```

## 示例项目源码

查看完整的示例项目与代码实现：

[https://github.com/bingbing-gui/aspnetcore-developer/tree/master/src/09-AI-Agent/SemanticKernel/SK.TextEmbeddingGeneration](https://github.com/bingbing-gui/aspnetcore-developer/tree/master/src/09-AI-Agent/SemanticKernel/SK.TextEmbeddingGeneration)


## 总结

旧接口ITextEmbeddingGenerationService 已被弃用。
推荐接口 IEmbeddingGenerator<string, Embedding<float>> 已成为标准做法。
