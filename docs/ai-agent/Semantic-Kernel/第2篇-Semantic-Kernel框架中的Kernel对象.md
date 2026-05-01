# 第3章-Semantic-Kernel核心组件

## Kernel介绍

Kernel是Semantic Kernel整个框架的核心组件，简单来说，Kernel是一个依赖注入DI容器，它负责管理运行 AI 应用所需的所有服务和插件。只要你将所有的服务和插件提供给Kernel，AI 就能在需要时无缝使用它们。，由于 Kernel 拥有运行本地代码和AI服务所需的所有服务和插件，因此它几乎被SK SDK 中的所有组件使用，以支持你的AI-Agents。这意味着，无论你在SK中运行提示-prompt还是code，Kernel 都会始终可用，并自动检索和调用所需的服务和插件。
![Kernel](/docs/SemanticKernel/Materials/the-kernel-is-at-the-center-of-everything.png)  
这非常强大，因为作为开发者，你可以在一个地方完成 AI 代理的配置和监控。

举个例子，当你在 Kernel 里调用一个 Prompt 时，内核会自动执行以下步骤：

### 第一步：选择AI服务

**图示**：在Kernel里，AI服务的选择发生在 "Select AI Service"这一阶段。  
**描述**：Semantic Kernel先决定使用哪一个AI模型，如OpenAI或Azure AI。  
**特性**：支持 "Model Selection"，确保调用最合适的AI。

### 第二步：渲染Prompt

**图示**：Render Prompt阶段是 Prompt 模板化(Templatization)。  
**描述**：Semantic Kernel负责基于预定义的Prompt模板，动态填充变量，使Prompt适配当前任务。  
**特性**：在这个阶段支持Prompt参数化(Parameterization)，便于在不同场景中复用和自定义Prompt。

### 第三步：调用AI服务

**图示**：Invoke AI Service直接调用 OpenAI、Azure AI 等服务，并处理其响应。  
**描述**：当Prompt准备就绪，Semantic Kernel将其发送到AI模型进行推理。  
**特性**：在这个阶段提供可靠性保障(Reliability)，以确保调用稳定。

### 第四步：解析LLM响应

**图示**：Parse LLM Response负责将AI生成的原始数据解析成人类可读格式。  
**描述**：Semantic Kernel负责解析AI生成的结果，并准备返回到应用程序。  
**特性**：这个阶段支持遥测和监控(Telemetry and Monitoring)，方便开发者跟踪AI响应。

### 第五步：创建函数结果

**图示**：Create Function Result代表最终返回的结果，供应用程序使用。  
**描述**：Semantic Kernel将解析后的数据整理成合适的格式并返回给你的应用。  
**特性**：在这里，开发者可以加入Responsible AI机制，比如过滤不合适的内容，确保AI负责任地运行。

在整个过程中，你可以创建事件(events)和中间件(middleware)，它们会在每个步骤被触发。这意味着你可以执行诸如日志记(logging)向用户提供状态更新，以及最重要的——确保负责任的AI(Responsible AI)等操作，而且所有这些都可以在一个统一的地方完成。

## 创建Kernel

| **组件**   | **描述**  |
|-----------|---------|
| **服务(Services)** | 包括 **AI 服务**（如聊天补全）和 **其他必要服务**(如日志记录、HTTP 客户端)，用于运行应用程序。它遵循 .NET 的 **Service Provider 模式**，以支持跨语言的 **依赖注入（Dependency Injection）**。 |
| **插件(Plugins)** | 这些是 **AI 服务和提示模板** 用于执行任务的组件。例如，AI 服务可以使用插件从数据库检索数据或调用外部 API 以执行操作。 |

在你的项目中导入如下包并创建Kernel：

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Plugins.Core;
```

接下来，你可以添加 服务（Services） 和 插件（Plugins）。下面是一个示例，展示如何添加 Azure OpenAI 聊天补全（chat completion）、日志记录（logger） 和 时间插件（time plugin）

```csharp
// 创建一个kernel使用logger和Azure OpenAI聊天服务
var builder = Kernel.CreateBuilder();
builder.AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
builder.Services.AddLogging(c => c.AddDebug().SetMinimumLevel(LogLevel.Trace));
builder.Plugins.AddFromType<TimePlugin>();
Kernel kernel = builder.Build();
```

## 依赖注入

在 C# 中，你可以使用依赖注入(DI)来创建Semantic Kernel。其核心步骤是 创建 ServiceCollection，然后向其中添加所需的服务和插件。
> **建议**：官方建议将 Kernel 作为 瞬态（Transient）服务 创建，使其在每次使用后自动释放，因为 插件集合（Plugin Collection）是可变的。Kernel 本身非常轻量（本质上只是一个 服务和插件的容器），因此 每次使用时创建一个新的 Kernel 并不会对性能造成影响。

```csharp

using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

// 添加 OpenAI chat completion service 作为单例模式
builder.Services.AddOpenAIChatCompletion(
    modelId: "gpt-4",
    apiKey: "YOUR_API_KEY",
    orgId: "YOUR_ORG_ID", // Optional; for OpenAI deployment
    serviceId: "YOUR_SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

// 针对插件创建单例模式
builder.Services.AddSingleton(() => new LightsPlugin());
builder.Services.AddSingleton(() => new SpeakerPlugin());

/*
serviceProvider.GetRequiredService<LightsPlugin>() 从DI容器中获取已经创建的LightsPlugin实例。
KernelPluginFactory.CreateFromObject(...)将插件转换成KernelPlugin，使其能在 KernelPluginCollection 中使用。
KernelPluginCollection 只是在管理 DI 容器中已经存在的插件，而没有创建新的插件对象
*/
builder.Services.AddSingleton<KernelPluginCollection>((serviceProvider) => 
    [
        KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<LightsPlugin>()),
        KernelPluginFactory.CreateFromObject(serviceProvider.GetRequiredService<SpeakerPlugin>())
    ]
);

// 最后，创建一个Kernel服务使用ServiceProvider和插件集合

builder.Services.AddTransient((serviceProvider)=> {
    KernelPluginCollection pluginCollection = serviceProvider.GetRequiredService<KernelPluginCollection>();

    return new Kernel(serviceProvider, pluginCollection);
});

```
## 总结 

Kernel是Semantic Kernel框架的核心组件，作为依赖注入容器，管理AI应用所需的所有服务和插件。它支持AI服务选择、Prompt渲染、调用AI服务、解析响应和创建结果。通过依赖注入，可以轻松添加服务和插件，确保AI应用的高效运行和管理。
文章地址：https://github.com/bingbing-gui/AspNetCore-Skill/blob/master/semantic-kernel/002-Semantic%20Kernel%E6%A1%86%E6%9E%B6%E7%9A%84%E6%A0%B8%E5%BF%83Kernel.md


