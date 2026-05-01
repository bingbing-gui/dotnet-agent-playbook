# 第4章-Semantic-Kernel集成AI服务

Semantic Kernel 的一个主要功能是能够向Kernel添加不同的 AI 服务。这使您可以轻松切换不同的 AI 服务，以比较它们的性能，并利用最适合您需求的模型。在本节中，我们将提供示例代码，展示如何向内核添加不同的 AI 服务。

在 Semantic Kernel 内部，针对最常见的 AI 任务提供了相应的接口。下表列出了每个 SDK 支持的 AI 服务。


| **服务**                    | **C#** | **Python** | **Java**
|----------------------------------|:------:|:---------:|:-------:|
| **Chat completion**              | ✅     | ✅        | ✅      |
| **Text generation**              | ✅     | ✅        | ✅      |
 **Embedding generation** (Exp.)  | ✅     | ✅        | ✅      |
| **Text-to-image** (Exp.)         | ✅     | ✅        | ❌      |
| **Image-to-text** (Exp.)         | ✅     | ❌        | ❌      |
| **Text-to-audio** (Exp.)         | ✅     | ✅        | ❌      |
| **Audio-to-text** (Exp.)         | ✅     | ✅        | ❌      |
| **Realtime** (Exp.)              | ❌     | ✅        | ❌      |
## Chat Completion

通过聊天补全（Chat Completion），您可以模拟与AI代理的往返对话。这不仅适用于创建聊天机器人，还可以用于构建自主代理，以执行业务流程、生成代码等。作为 OpenAI、Google、Mistral、Facebook 等公司提供的主要模型类型，聊天补全是您在 Semantic Kernel 项目中最常添加的 AI 服务。

在选择**聊天补全（Chat Completion）**模型时，您需要考虑以下因素：

模型支持哪些形态（例如：文本、图像、音频等）？
是否支持函数调用（Function Calling）？
生成和接收 token 的速度如何？
每个 token 的成本是多少？
重要提示
在上述所有问题中，最关键的是模型是否支持函数调用。
如果不支持，您将无法使用该模型来调用现有代码。

目前，OpenAI、Google、Mistral 和 Amazon 的大部分最新模型均支持函数调用，但小型语言模型的支持仍然有限。

### 设置本地环境变量

某些 AI 服务可以本地部署，并可能需要一些设置。以下是支持本地部署的服务的相关指南。

### 可以在本地设置的AI服务

某些 AI 服务可以本地部署，并可能需要一些设置。以下是支持本地部署的服务的相关指南。


| Service               | 是否支持本地 |
|-----------------------|--------------------|
| **Azure OpenAI**      | 不支持    |
| **OpenAI**           | 不支持    |
| **Mistral**          | 不支持    |
| **Google**           | 不支持    |
| **Hugging Face**     | 不支持  |
| **Azure AI Inference** | 不支持    |
| **Ollama**           | ✅ 支持使用docker本地设置 |


### Ollama 本地设置

要使用 Docker 在本地运行 **Ollama**，请使用以下命令启动一个使用 CPU 的容器：
```bash
docker run -d -v "c:\temp\ollama:/root/.ollama" -p 11434:11434 --name ollama ollama/ollama
```

要使用 GPU 加速运行，请使用以下命令：

```bash
docker run -d --gpus=all -v "c:\temp\ollama:/root/.ollama" -p 11434:11434 --name ollama ollama/ollama
```

启动容器后，在 Docker 容器内打开终端：

如果使用 Docker Desktop，请从操作中选择“在终端中打开”。

然后，下载所需的模型，例如 phi3 模型：

```bash
ollama pull phi3
```

### 安装 AI 服务所需的包

在向您的 **Semantic Kernel** 添加Chat Completion之前，您需要安装必要的包。  
以下是每个 AI 服务提供商所需的包。

| AI Service       | Required Package |
|-----------------|-----------------|
| **Azure OpenAI** | `Microsoft.SemanticKernel.Connectors.AzureOpenAI` |
| **OpenAI**       | `Microsoft.SemanticKernel.Connectors.OpenAI` |
| **Mistral**      | `Microsoft.SemanticKernel.Connectors.MistralAI` |
| **Google**       | `Microsoft.SemanticKernel.Connectors.Google` |
| **Hugging Face** | `Microsoft.SemanticKernel.Connectors.HuggingFace` |
| **Azure AI Inference** | `Microsoft.SemanticKernel.Connectors.AzureAIInference` |
| **Ollama**       | `Microsoft.SemanticKernel.Connectors.Ollama` |
| **Anthropic**    | `Microsoft.SemanticKernel.Connectors.Anthropic` |

要安装 **Azure OpenAI** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.AzureOpenAI
```

要安装 **OpenAI** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.OpenAI
```

要安装 **Mistral** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.MistralAI
```

要安装 **Google** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.Google
```

要安装 **Hugging Face** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.HuggingFace
```

要安装 **Azure AI Inference** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.AzureAIInference
```

要安装 **Ollama** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.Ollama
```

要安装 **Anthropic** 所需的包，请使用以下命令：

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.Anthropic
```

## 创建Chat Completion服务

现在，您已经安装了必要的包，可以创建聊天补全（Chat Completion）服务。以下是使用 Semantic Kernel 创建聊天补全服务的几种方法。

### 直接添加到 Kernel

要添加聊天补全（Chat Completion）服务，可以使用以下代码将其添加到 Kernel 的内部服务提供程序。

**Azure OpenAI** 服务

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT",
    modelId: "gpt-4", 
    serviceId: "YOUR_SERVICE_ID", 
    httpClient: new HttpClient()
);
Kernel kernel = kernelBuilder.Build();
```

**OpenAI** 服务

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOpenAIChatCompletion(
    modelId: "gpt-4",
    apiKey: "YOUR_API_KEY",
    orgId: "YOUR_ORG_ID", // Optional
    serviceId: "YOUR_SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);
Kernel kernel = kernelBuilder.Build();
```

**Mistral** 服务

```csharp
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddMistralChatCompletion(
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    serviceId: "SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
    httpClient: new HttpClient() // Optional; for customizing HTTP client
);
Kernel kernel = kernelBuilder.Build();
```

**Google**服务

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

#pragma warning disable SKEXP0070
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddGoogleAIGeminiChatCompletion(
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    apiVersion: GoogleAIVersion.V1, // Optional
    serviceId: "SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
    httpClient: new HttpClient() // Optional; for customizing HTTP client
);
Kernel kernel = kernelBuilder.Build();
```

**HuggingFace**服务

```csharp
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddHuggingFaceChatCompletion(
    model: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    serviceId: "SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
    httpClient: new HttpClient() // Optional; for customizing HTTP client
);
Kernel kernel = kernelBuilder.Build();
```

**AzureAIInference**服务

```csharp
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureAIInferenceChatCompletion(
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    serviceId: "SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
    httpClient: new HttpClient() // Optional; for customizing HTTP client
);
Kernel kernel = kernelBuilder.Build();

```

**ollama**服务

```csharp
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddOllamaChatCompletion(
    modelId: "NAME_OF_MODEL",           // E.g. "phi3" if phi3 was downloaded as described above.
    endpoint: new Uri("YOUR_ENDPOINT"), // E.g. "http://localhost:11434" if Ollama has been started in docker as described above.
    serviceId: "SERVICE_ID"             // Optional; for targeting specific services within Semantic Kernel
);
Kernel kernel = kernelBuilder.Build();
```

**Bedrock**服务

```csharp
using Microsoft.SemanticKernel;

#pragma warning disable SKEXP0070
IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddBedrockChatCompletionService(
    modelId: "NAME_OF_MODEL",
    bedrockRuntime: amazonBedrockRuntime, // Optional; An instance of IAmazonBedrockRuntime, used to communicate with Azure Bedrock.
    serviceId: "SERVICE_ID"               // Optional; for targeting specific services within Semantic Kernel
);
Kernel kernel = kernelBuilder.Build();
```

### 使用依赖注入

如果您使用依赖注入（Dependency Injection），通常会希望将 AI 服务直接添加到 服务提供程序（Service Provider）。
这样做的好处是可以创建 单例（Singleton） AI 服务，并在 临时（Transient） Kernel 中重复使用它们。

**Azure OpenAI** 服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT",
    modelId: "gpt-4", // Optional name of the underlying model if the deployment name doesn't match the model name
    serviceId: "YOUR_SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```

**OpenAI** 服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOpenAIChatCompletion(
    modelId: "gpt-4",
    apiKey: "YOUR_API_KEY",
    orgId: "YOUR_ORG_ID", // Optional; for OpenAI deployment
    serviceId: "YOUR_SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```

**Mistral** 服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

#pragma warning disable SKEXP0070
builder.Services.AddMistralChatCompletion(
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    serviceId: "SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});

```

**Google**服务

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;

var builder = Host.CreateApplicationBuilder(args);

#pragma warning disable SKEXP0070
builder.Services.AddGoogleAIGeminiChatCompletion(
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    apiVersion: GoogleAIVersion.V1, // Optional
    serviceId: "SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});

```

**HuggingFace**服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

#pragma warning disable SKEXP0070
builder.Services.AddHuggingFaceChatCompletion(
    model: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    serviceId: "SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```

**AzureAIInference**服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

#pragma warning disable SKEXP0070
builder.Services.AddAzureAIInferenceChatCompletion(
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    serviceId: "SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```

**ollama**服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

#pragma warning disable SKEXP0070
builder.Services.AddOllamaChatCompletion(
    modelId: "NAME_OF_MODEL",           // E.g. "phi3" if phi3 was downloaded as described above.
    endpoint: new Uri("YOUR_ENDPOINT"), // E.g. "http://localhost:11434" if Ollama has been started in docker as described above.
    serviceId: "SERVICE_ID"             // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});

```

**Bedrock**服务

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

#pragma warning disable SKEXP0070
builder.Services.AddBedrockChatCompletionService(
    modelId: "NAME_OF_MODEL",
    bedrockRuntime: amazonBedrockRuntime, // Optional; An instance of IAmazonBedrockRuntime, used to communicate with Azure Bedrock.
    serviceId: "SERVICE_ID"               // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```

### 单独创建实例（Creating Standalone Instances）

最后，您可以直接创建服务的实例，以便：稍后添加到 Kernel，或者
直接在代码中使用，而无需将其注入到 Kernel 或服务提供程序中。

**Azure OpenAI** 服务

```csharp
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

AzureOpenAIChatCompletionService chatCompletionService = new (
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT",
    modelId: "gpt-4", // Optional name of the underlying model if the deployment name doesn't match the model name
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);
```

**OpenAI** 服务

```csharp
using Microsoft.SemanticKernel.Connectors.OpenAI;

OpenAIChatCompletionService chatCompletionService = new (
    modelId: "gpt-4",
    apiKey: "YOUR_API_KEY",
    organization: "YOUR_ORG_ID", // Optional
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);
```

**Mistral** 服务

```csharp
using Microsoft.SemanticKernel.Connectors.MistralAI;

#pragma warning disable SKEXP0070
MistralAIChatCompletionService chatCompletionService = new (
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Optional
    httpClient: new HttpClient() // Optional; for customizing HTTP client
);
```

**Google**服务

```csharp
using Microsoft.SemanticKernel.Connectors.Google;

#pragma warning disable SKEXP0070
GoogleAIGeminiChatCompletionService chatCompletionService = new (
    modelId: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    apiVersion: GoogleAIVersion.V1, // Optional
    httpClient: new HttpClient() // Optional; for customizing HTTP client
);
```

**HuggingFace**服务

```csharp
using Microsoft.SemanticKernel.Connectors.HuggingFace;

#pragma warning disable SKEXP0070
HuggingFaceChatCompletionService chatCompletionService = new (
    model: "NAME_OF_MODEL",
    apiKey: "API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT") // Optional
);
```

**AzureAIInference**服务

```csharp
using Microsoft.SemanticKernel.Connectors.AzureAIInference;

#pragma warning disable SKEXP0070
AzureAIInferenceChatCompletionService chatCompletionService = new (
    modelId: "YOUR_MODEL_ID",
    apiKey: "YOUR_API_KEY",
    endpoint: new Uri("YOUR_ENDPOINT"), // Used to point to your service
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);
```

**ollama**服务

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
using OllamaSharp;

#pragma warning disable SKEXP0070
using var ollamaClient = new OllamaApiClient(
    uriString: "YOUR_ENDPOINT"    // E.g. "http://localhost:11434" if Ollama has been started in docker as described above.
    defaultModel: "NAME_OF_MODEL" // E.g. "phi3" if phi3 was downloaded as described above.
);

IChatCompletionService chatCompletionService = ollamaClient.AsChatCompletionService();
```

**Bedrock**服务

```csharp
using Microsoft.SemanticKernel.Connectors.Amazon;

#pragma warning disable SKEXP0070
BedrockChatCompletionService chatCompletionService = new BedrockChatCompletionService(
    modelId: "NAME_OF_MODEL",
    bedrockRuntime: amazonBedrockRuntime // Optional; An instance of IAmazonBedrockRuntime, used to communicate with Azure Bedrock.
);
```

## 获取ChatCompletion服务

一旦您将 聊天补全（Chat Completion） 服务添加到 Kernel，就可以使用GetRequiredService方法来检索它。
以下是从 Kernel 中检索 聊天补全服务 的示例代码。

``` csharp
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
```

## 使用Chat Completion服务

现在您已经拥有 聊天补全（Chat Completion） 服务，可以使用它来从 AI 代理 生成响应。
主要有两种使用方式：

1. 非流式（Non-streaming）：等待服务生成完整的响应后，再返回给用户。
2. 流式（Streaming）：响应会被逐步生成，并在创建的同时返回给用户。

以下是使用 聊天补全服务 生成响应的两种方式。

### 非流式ChatCompletion

要使用 非流式（Non-streaming） 聊天补全，您可以使用以下代码从 AI 代理 生成响应。

```csharp
ChatHistory history = [];
history.AddUserMessage("Hello, how are you?");

var response = await chatCompletionService.GetChatMessageContentAsync(
    history,
    kernel: kernel
);
```

### 流式ChatCompletion

要使用 流式（Streaming） 聊天补全，您可以使用以下代码从 AI 代理 生成响应。

```csharp
ChatHistory history = [];
history.AddUserMessage("Hello, how are you?");
var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
    chatHistory: history,
    kernel: kernel
);

await foreach (var chunk in response)
{
    Console.Write(chunk);
}
```

## 聊天历史

### 创建一个ChatList对象

聊天历史对象本质上是一个列表，因此可以轻松创建并向其中添加消息。

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
// 创建一个聊天历史对象
ChatHistory chatHistory = [];
// 添加系统消息
chatHistory.AddSystemMessage("你是一个乐于助人的助手。");
// 添加用户消息
chatHistory.AddUserMessage("有什么可以点的？");
// 添加助手消息（AI 回复）
chatHistory.AddAssistantMessage("我们有披萨、意大利面和沙拉可供点餐。你想点什么？");
// 添加用户消息
chatHistory.AddUserMessage("我想要第一个选项，谢谢。");
```

### 丰富聊天消息内容

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
// 添加系统消息
chatHistory.Add(
    new() {
        Role = AuthorRole.System,
        Content = "你是一个乐于助人的助手"
    }
);
// 添加带图片的用户消息
chatHistory.Add(
    new() {
        Role = AuthorRole.User,
        AuthorName = "Laimonis Dumins",
        Items = [
            new TextContent { Text = "这个菜单上有什么可选的？" },
            new ImageContent { Uri = new Uri("https://example.com/menu.jpg") }
        ]
    }
);
// 添加助手消息
chatHistory.Add(
    new() {
        Role = AuthorRole.Assistant,
        AuthorName = "餐厅助手",
        Content = "我们有披萨、意大利面和沙拉可供点餐。您想点什么？"
    }
);

// 添加来自另一位用户的额外消息
chatHistory.Add(
    new() {
        Role = AuthorRole.User,
        AuthorName = "Ema Vargova",
        Content = "我想要第一个选项，谢谢。"
    }
);

```


### 检查聊天历史对象

当你把聊天历史对象传递给支持自动函数调用的聊天服务时，它会自动添加函数调用和返回结果，你不需要手动去加这些信息。
这样做的好处是，你可以随时查看聊天记录，直接看到 AI 调用了哪些函数，以及返回了什么结果。
不过，你还是需要手动添加最终的聊天消息，让对话完整。下面是一个示例，展示如何检查聊天记录，看看 AI 都做了哪些函数调用和返回了什么数据。

### 聊天历史优化

管理聊天历史对于保持上下文感知的对话至关重要，同时还能确保高效的性能。随着对话的进行，聊天历史对象会不断增长，
可能超出模型的上下文窗限制，从而影响回复质量并降低处理速度。通过结构化的方法来优化聊天历史，可以确保保留最相关的信息，
同时避免不必要的开销。

### 为什么要优化聊天历史？

1. 性能优化（Performance Optimization）
聊天记录过大会增加处理时间，影响AI生成回复的速度。适当缩减历史记录可以保持交互流畅、响应快速。

2. 上下文窗口管理（Context Window Management）
语言模型的上下文窗口是固定的，一旦聊天记录超过这个限制，旧消息会被丢弃。合理管理历史记录，可以确保最重要的上下文信息不会丢失。

3. 内存效率（Memory Efficiency）
在资源受限的环境（如移动应用、嵌入式系统）中，过长的聊天历史可能导致内存占用过大，影响系统性能。适当减少聊天记录可以避免性能下降。

4. 隐私与安全（Privacy and Security）
保留过多的聊天记录 可能会增加敏感信息暴露的风险。结构化的优化策略 可以减少不必要的数据存储，同时保留关键对话信息，提高隐私保护和数据安全性。

### 减少聊天历史的策略

为了在保留关键信息的同时，让聊天历史保持可管理的状态，可以使用以下几种方法：

1. 截断（Truncation）
当聊天历史超过预设的限制时，删除最早的消息，确保只保留最近的对话记录。适用于不需要长期上下文的对话，如客服聊天、即时问答等。

2. 摘要（Summarization）
将较旧的消息压缩成摘要，保留关键内容，同时减少存储的消息数量。适用于长时间的连续对话，比如咨询、技术支持等场景。

3. 基于 Token 的优化（Token-Based）
计算聊天记录的 Token 数量，确保总 Token 量不超过模型的上下文窗口限制。当超出 Token 限制时，删除或总结旧消息，确保 AI 仍然能理解当前上下文。
适用于LLM 生成式 AI 交互，如 OpenAI、Azure AI 等。

```csharp
namespace Microsoft.SemanticKernel.ChatCompletion;

[Experimental("SKEXP0001")]
public interface IChatHistoryReducer
{
    Task<IEnumerable<ChatMessageContent>?> ReduceAsync(IReadOnlyList<ChatMessageContent> chatHistory, CancellationToken cancellationToken = default);
}
```

该接口允许自定义实现聊天历史优化。此外，Semantic Kernel 提供了内置的聊天历史优化器（Reducers），包括：

1. ChatHistoryTruncationReducer（聊天历史截断优化器）
作用：将聊天历史截断到指定大小，并直接丢弃被移除的消息。
触发条件：当聊天历史的长度超过设定的限制时，自动执行截断操作。

2. ChatHistorySummarizationReducer（聊天历史摘要优化器）
作用：在截断聊天历史的同时，对被移除的消息进行摘要总结，然后将该摘要作为一条消息添加回聊天历史。
优点：不会完全丢失上下文，而是用更简短的方式保留重要信息。

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// 创建 OpenAI 聊天补全服务
var chatService = new OpenAIChatCompletionService(
    modelId: "<model-id>",  // 模型 ID
    apiKey: "<api-key>");    // API 密钥

// 创建聊天历史截断优化器，仅保留系统消息和最近两条用户消息
var reducer = new ChatHistoryTruncationReducer(targetCount: 2); 

// 初始化聊天历史，并设定系统消息
var chatHistory = new ChatHistory("你是一位图书管理员，精通关于城市的书籍");

string[] userMessages = [
    "推荐一些关于西雅图的书籍", 
    "推荐一些关于都柏林的书籍", 
    "推荐一些关于阿姆斯特丹的书籍", 
    "推荐一些关于巴黎的书籍", 
    "推荐一些关于伦敦的书籍"
];

int totalTokenCount = 0;

foreach (var userMessage in userMessages)
{
    // 添加用户消息
    chatHistory.AddUserMessage(userMessage);

    Console.WriteLine($"\n>>> 用户:\n{userMessage}");

    // 执行聊天历史优化，减少历史记录
    var reducedMessages = await reducer.ReduceAsync(chatHistory);

    if (reducedMessages is not null)
    {
        chatHistory = new ChatHistory(reducedMessages);
    }

    // 发送优化后的聊天历史给 AI 并获取回复
    var response = await chatService.GetChatMessageContentAsync(chatHistory);

    // 将 AI 的回复添加到聊天历史
    chatHistory.AddAssistantMessage(response.Content!);

    Console.WriteLine($"\n>>> 助手:\n{response.Content!}");

    // 计算总 Token 使用量
    if (response.InnerContent is OpenAI.Chat.ChatCompletion chatCompletion)
    {
        totalTokenCount += chatCompletion.Usage?.TotalTokenCount ?? 0;
    }
}

Console.WriteLine($"总 Token 计数: {totalTokenCount}");

```

### 在聊天补全中使用图片

许多 AI 服务支持同时使用图像、文本等多种输入，这使得开发者可以融合不同的输入方式。
这样可以实现诸如传递一张图片，并向 AI 模型提问等应用场景。

Semantic Kernel 的聊天补全连接器支持同时向聊天补全 AI 模型传递图像和文本。
但需要注意，并非所有 AI 模型或 AI 服务都支持此功能。在按照聊天补全文章中的步骤构建聊天补全服务后，
你可以使用以下方式提供图像和文本。

```csharp
// Load an image from disk.
byte[] bytes = File.ReadAllBytes("path/to/image.jpg");
// Create a chat history with a system message instructing
// the LLM on its required role.
var chatHistory = new ChatHistory("Your job is describing images.");
// Add a user message with both the image and a question
// about the image.
chatHistory.AddUserMessage(
[
    new TextContent("What’s in this image?"),
    new ImageContent(bytes, "image/jpeg"),
]);
// Invoke the chat completion model.
var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
Console.WriteLine(reply.Content);
```


## 总结

这篇内容详细展示了如何在 Semantic Kernel 中集成多种 AI 服务并创建 Chat Completion 功能，包括安装必要的包、添加服务到 Kernel、使用依赖注入、以及管理聊天历史和优化性能等重要技巧。通过灵活运用上述方法，可以更好地利用 Semantic Kernel 的强大特性，为各类应用提供更全面、高效的智能对话支持。下一节我们讨论SK中的Function Calling