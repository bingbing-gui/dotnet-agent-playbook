随着大语言模型（LLM）的发展，其能力早已不再局限于纯文本处理。  
多模态（Multimodality）——即同时理解文本、图像，甚至音频的能力，正在逐渐成为主流模型的标配能力。

在 .NET 生态中，这种变化同样体现在开发范式上：  
我们不再只是“调用一次 API”，而是开始构建具备明确职责的 AI Agent。

本文将通过一段简洁但完整的 C# 示例，演示如何基于 Azure OpenAI 和 Microsoft.Extensions.AI 提供的 Agent 抽象，构建一个能够理解图像内容的 Vision Agent。

## 1. 核心依赖与准备工作

在开始编写业务逻辑之前，首先引入必要的命名空间：

```csharp
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
```

这里涉及两个关键点：

- Azure.AI.OpenAI：Azure OpenAI 官方 SDK
- Microsoft.Extensions.AI：为 AI 能力提供统一抽象（Chat、Embedding、Agent 等）

### 配置加载

模型与服务的配置通过环境变量注入：

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
               ?? throw new InvalidOperationException("Missing endpoint");

var deploymentName =
    Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-4o";
```

说明：为了获得最佳的图像理解效果，建议使用具备原生视觉能力的模型，例如 gpt-4o。

## 2. 创建一个“视觉”Agent

与传统直接使用 `ChatClient` 不同，这里我们将其提升为一个 Agent，使模型具备：

- 明确的身份（Name）
- 持久的行为指令（Instructions）
- 独立的对话上下文（Thread）

```csharp
var agent =
    new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
        .GetChatClient(deploymentName)
        .CreateAIAgent(
            name: "VisionAgent",
            instructions:
                "你是一个分析图片内容的智能代理，请根据图片内容回答用户的问题。");
```

关键说明：

- 身份验证：使用 `AzureCliCredential()`，直接复用本地 Azure CLI 的登录状态，避免在代码中硬编码 API Key。
- Instructions（系统指令）：用于定义 Agent 的长期行为角色，而不是每次请求都重复传入 Prompt。  
  从这一刻开始，这个 Agent 的“职责”就是：分析图像并回答问题。

## 3. 构建多模态消息（Multimodal Message）

在多模态场景中，一条用户消息不再只是一个字符串，而是由多个 Content Item 组成。

下面的示例中，我们将文本问题与图片数据组合成一条用户消息。

```csharp
using HttpClient httpClient = new();

// 为示例简洁直接使用 HttpClient；
// 生产环境中建议使用 IHttpClientFactory
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(
    "Mozilla/5.0 (compatible; VisionAgent/1.0)");

byte[] imageBytes = await httpClient.GetByteArrayAsync(
    "https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/" +
    "Gfp-wisconsin-madison-the-nature-boardwalk.jpg/" +
    "2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg");

ChatMessage message = new(ChatRole.User, [
    new TextContent("你在这张图片中看到了什么？"),
    // DataContent / BinaryContent 表示二进制输入（如图片）
    new DataContent(imageBytes, "image/jpeg")
]);
```

内容说明：

- `TextContent`：用户的自然语言提问
- `DataContent`：图片的二进制数据（指定 MIME 类型）

框架会自动将这一结构转换为底层模型（如 GPT-4o）能够理解的多模态 JSON 格式。

## 4. 运行 Agent 并使用流式输出

接下来，创建一个新的对话线程（Thread），并以流式方式运行 Agent：

```csharp
var thread = agent.GetNewThread();

await foreach (var update in agent.RunStreamingAsync(message, thread))
{
    Console.WriteLine(update);
}
```

这里发生了什么？

- Thread：表示一次完整对话的上下文容器，可用于多轮交互。
- RunStreamingAsync：以流式方式返回模型的响应内容，适合：
  - 实时 UI
  - 命令行交互
  - Agent 调试与可观测性

## 5. 总结：从“调用模型”到“构建 Agent”

这段代码虽然简洁，却清晰地体现将图像视为一种自然的输入类型，与文本并列处理。

