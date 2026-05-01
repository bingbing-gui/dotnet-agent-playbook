## 前置准备——部署模型

在构建智能体之前，我们需要先准备好依赖的大模型。这里我们可以选择两种方式：
- 云部署方式（如 Azure AI Foundry 提供的 GPT-4、Phi-3、Mistral 等）
- 本地私有模型（例如自己部署的开源的模型）

这是构建智能体的前提条件：
- ✅ 必须先有一个可供调用的模型。

在本系列文章中，我们选择 Azure AI Foundry 作为核心平台。

Azure AI Foundry 是一个 PaaS 级的 AI 开发与运营平台，能够让你在云端轻松部署上千种模型。平台提供：
- 模型目录（Model Catalog）
- 智能体构建与测试环境
- 完整的 API 访问能力

通过它，你可以快速部署模型并通过 API 接入到你的应用程序中。

---

## 访问 API 的 Azure 授权方式

当你调用 Azure AI Foundry 中部署的模型 API 时，必须进行 Azure 授权，否则无法访问。

调用 Azure 服务（例如模型推理、数据存储、监控日志等）都需要携带身份凭证（Credential）。这些凭证由 Azure.Identity 包提供，常见的两种方式如下：

1️⃣ DefaultAzureCredential
- 智能检测多种身份来源（环境变量、托管身份、CLI 登录等）
- 适合从本地开发到云端部署的全生命周期
- 官方推荐的默认方式

2️⃣ AzureCliCredential
- 直接复用你通过 az login 命令登录 Azure CLI 后的身份
- 适合本地开发与测试阶段使用
- 不适用于生产环境

本示例中我们将使用 AzureCliCredential 来访问部署在 Azure AI Foundry 的 GPT-4o 模型。

---

## 编写智能体代码（C# 示例）

完成模型部署和授权配置后，我们就可以开始编写代码。下面的示例展示了如何使用 Microsoft Agent Framework 调用 Azure AI Foundry 模型并构建一个简单的对话型 Agent。

### 🔧 环境准备
创建一个 Console 应用项目，并添加以下 NuGet 包：
```bash
dotnet add package Azure.AI.OpenAI
dotnet add package Microsoft.Agents.AI.OpenAI
dotnet add package Microsoft.Extensions.AI.OpenAI
dotnet add package Azure.Identity
```

### ⚙️ 环境变量
为了方便维护，我们把上面模型部署名和 api 地址配置到环境变量中：
```bash
AZURE_OPENAI_ENDPOINT=https://your-endpoint-name.openai.azure.com/
AZURE_OPENAI_DEPLOYMENT_NAME=gpt-4o-mini
```

### 智能体示例代码
```csharp
// Copyright (c) Microsoft. All rights reserved.
// 示例：使用 Azure OpenAI 模型创建一个简单的 AI 智能体

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-4o-mini";

// 使用 Azure CLI 登录凭证授权访问
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一个诗人", name: "Joker");

// 调用智能体并输出结果
Console.WriteLine(await agent.RunAsync("请帮我写一首诗。"));

Console.ReadLine();

// 流式调用（可选）
// await foreach (var update in agent.RunStreamingAsync("请帮我写一首诗。"))
// {
//     Console.WriteLine(update);
// }
```

---

运行结果

运行程序后，智能体将通过 Azure AI Foundry 调用 GPT-4o 模型，生成一首诗并输出到控制台。

你刚刚完成了：
- ✅ 使用 Microsoft Agent Framework 搭建了第一个对话型 Agent。

---

## 补充

### 1. 部署模型

你可以参考这篇文章来部署模型：基于 Azure AI Foundry 的企业级 AI 应用开发

在该文章中，你可以完成以下操作：
- 创建 Azure AI Foundry 项目 / Hub
- 选择并部署 GPT-4o 等模型
- 获取对应的 推理 Endpoint 与 部署名（deployment name）


### 2. 认证方式

在调用 Azure AI Foundry / Azure OpenAI 的模型 API 时，必须先完成 Azure 的身份认证。常见三种方式：
- 使用本地 Azure CLI 登录（适合开发调试）
- 使用服务主体 (Service Principal) 认证（适合 CI/CD、生产环境）
- 使用 API 密钥认证（简单直接）

#### 2.1 使用本地 Azure CLI 登录

安装 Azure CLI（Windows MSI）：
- https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?view=azure-cli-latest&pivots=msi

安装完成后验证版本：
```bash
az version
```

使用浏览器登录：
```bash
az login
```
登录完成后，可在代码中通过 AzureCliCredential 复用该登录状态。

#### 2.2 使用服务主体 (Service Principal) 认证

适用场景：
- CI/CD 管道（GitHub Actions、Azure DevOps）
- 生产环境应用（容器、Web API、后台服务）
- 不方便人工 az login 的环境

2.2.1 在 Entra ID 中创建应用注册：
- Azure Portal → Microsoft Entra ID → App registrations → New registration
- Name：my-app-for-ai-access
- 支持的账户类型：默认（单租户）
- Redirect URI：可留空
- 记录：Application (client) ID、Directory (tenant) ID、Object ID

2.2.2 生成客户端密钥（Client Secret）：
- Certificates & secrets → New client secret
- 保存并复制生成的 Value（clientSecret）

2.2.3 为服务主体赋予 Azure OpenAI 访问权限：
- 打开你的 Azure OpenAI 资源（例如 https://maf.openai.azure.com/）
- Access control (IAM) → Add → Add role assignment
- 角色：Cognitive Services OpenAI User（推荐）
- 成员：选择 my-app-for-ai-access
- 保存并等待权限生效

#### 2.3 使用 API 密钥认证

如果希望最快速调用模型（不使用 Entra ID / RBAC），可以直接使用 API Key 认证。
- 注意：API Key 权限较高，务必妥善保管，避免泄露。

2.3.1 获取 API Key 和 Endpoint：
- 在 Azure OpenAI 资源中：Keys and Endpoint
- 获取 KEY 1 或 KEY 2
- 获取 Endpoint（如：https://maf.openai.azure.com/）
