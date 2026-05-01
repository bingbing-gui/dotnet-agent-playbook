# 第19章-创建并使用Azure AI 服务

## Azure AI 服务介绍

人工智能（AI）正在改变我们的世界，几乎所有行业都受到了它的影响。从改善医疗保健到提升网络安全，AI 正在帮助我们应对一些社会上最重大的挑战。

Azure AI 服务是一组 AI 能力的组合，可实现语言、视觉、智能搜索、内容生成等工作负载的自动化。这些服务易于实施，不需要专业的 AI 知识。

各类组织正在以创新方式使用 Azure AI 服务。例如，在机器人应用中，AI 可通过表达快乐、关心甚至笑声，为老年人提供拟人化的陪伴。在其他场景中，科学家们利用 AI 来保护濒危物种，例如通过识别图像中难以发现的动物。过去，这是一项耗时且容易出错的工作，而 Azure AI 视觉服务能够快速高效地完成这一任务，从而让科学家们可以专注于其他研究工作。

在本模块中，你将学习 Azure AI 服务的基本概念，以及如何在自己的应用程序中使用它们。


Azure AI 服务是可以集成到 Web 或移动应用程序中的 AI 功能，实现方式简单直接。这些 AI 服务包括生成式 AI、图像识别、自然语言处理、语音、AI 驱动的搜索等。Azure 提供十多种不同的 AI 服务，这些服务可以单独使用，也可以组合使用，为应用程序提供 AI 能力。

---

## 创建Azure AI 服务

![](/learning-notes/materials/azure-cloud-services.png)

Azure AI 服务是基于云的，与所有 Azure 服务一样，您需要创建资源才能使用它们。Azure AI 服务资源分为两种类型：多服务资源 和 单服务资源。您的开发需求以及成本计费方式决定了您需要的资源类型。

多服务资源：在 Azure 门户 中创建的资源，可通过 单一密钥和端点 访问多个 Azure AI 服务。当您需要多个 AI 服务或正在探索 AI 功能时，可以使用 Azure AI Services 资源。使用该资源时，所有 AI 服务的费用将统一计费。

单服务资源：在 Azure 门户 中创建的资源，仅提供对 单个 Azure AI 服务（如 语音（Speech）、视觉（Vision）、语言（Language） 等）的访问。每个 Azure AI 服务都有 独立的密钥和端点。如果您只需要 一个 AI 服务 或希望单独查看 成本信息，可以选择此类资源。

您可以通过多种方式创建资源，例如在 Azure 门户 中创建。

要创建 Azure AI 服务资源，请登录 Azure 门户

![](/learning-notes/materials/1741351883302.jpg)

要创建单服务资源，请搜索特定的 Azure AI 服务，例如 人脸识别（Face）、语言（Language） 或 内容安全（Content Safety） 等。大多数 AI 服务都提供 免费定价层，以便您探索其功能。

在选择所需资源并点击 “创建” 后，系统会提示您填写相关信息，包括：

1. 订阅（Subscription）
2. 资源组（Resource Group，用于容纳该资源）
3. 区域（Region）
4. 唯一名称（Unique Name）
5. 定价层（Price Tier）

完成这些详细信息后，即可创建该资源

## 使用Azure AI 服务

创建 Azure AI 服务资源 后，您可以使用 REST API、软件开发工具包（SDKs） 或 Visual Studio 接口 来构建应用程序。

![](/learning-notes/materials/azure-studio-examples.png)

## Azure AI Studio

Azure AI Studio提供了 友好的用户界面，可用于探索 Azure AI服务。不同的 Azure AI 服务提供不同的Studio，例如：

1. Vision Studio（视觉工作室）
2. Language Studio（语言工作室）
3. Speech Studio（语音工作室）
4. Content Safety Studio（内容安全工作室）
您可以使用提供的示例测试 Azure AI 服务，或使用自己的内容进行实验。基于Studio方式 使您能够探索、演示和评估 Azure AI 服务，无论您是否具备 AI 或编程经验。

注意
除了针对单个 Azure AI 服务的Studio，Microsoft Azure 还提供了 Azure AI Foundry 门户，该门户将多个 Azure AI 服务 和 生成式 AI 模型 集成到 一个用户界面 中，以提供统一的访问和管理体验。

## 关联AI服务资源

在使用 AI服务资源 之前，您必须在 设置Settings页面中将其与您要使用的Studio关联。

选择您的 AI 服务资源。
点击 使用资源 Use Resource）。
完成后，您即可在 Studio界面 中探索 Azure AI 服务。

![](/learning-notes/materials/content-safety-resource-example.png)

以 Azure AI 内容安全（Content Safety）服务 为例，该服务用于识别有害文本或图像。

要探索 内容安全服务 的功能，我们可以使用 Content Safety Studio，具体步骤如下：
创建 Azure AI 资源：

1. 创建 多服务 Azure AI 资源，或
2. 创建 单服务 Content Safety 资源。

在 Content Safety Studio 关联资源：

1. 进入 Content Safety Studio 的 设置（Settings） 页面。
2. 选择刚刚创建的 AI 服务资源。
3. 点击 “使用资源”（Use Resource） 进行关联。

完成后，您创建的 AI 服务 就会与 Content Safety Studio 关联，并可以开始使用。

## 理解Azure AI 服务的认证

您现在已经学会了如何创建 AI 服务资源。但是，如何确保只有授权用户才能访问您的 AI 服务呢？这可以通过**身份验证（Authentication）**来实现，即验证用户或服务的身份，并确保其被授权使用该服务。

大多数 Azure AI 服务是通过 RESTful API 访问的，尽管还有其他访问方式。API 定义了 Azure AI 服务与使用它的软件组件之间传递的信息。清晰的 API 接口定义至关重要，因为如果 AI 服务更新，您的应用仍然需要能够正常运行。

API 的一部分功能是处理身份验证。每当请求使用 AI 服务资源时，该请求都必须经过身份验证。例如，Azure 会验证您的订阅和AI 服务资源，以确保您具有访问权限。此身份验证过程使用端点（Endpoint）和资源密钥（Resource Key）。

端点 指定了如何访问 AI 服务资源，类似于 URL 标识网站。当您查看资源的端点时，它可能如下所示：

```
https://myaiservices29.cognitiveservices.azure.com/
```
资源密钥（Resource Key） 保护您的资源隐私。为了确保安全性，密钥可以定期更换。

您可以在 Azure 门户（Azure Portal） 中的 资源管理（Resource Management） 下的 密钥和端点（Keys and Endpoint） 查看 端点（Endpoint） 和 密钥（Key）。

![](/learning-notes/materials/1741355607717.jpg)

当您编写代码访问 AI 服务时，必须在 身份验证请求头（Authentication Header） 中包含 密钥（Keys） 和 端点（Endpoint）。身份验证请求头会将授权密钥发送到服务，以确认应用程序可以使用该资源。您可以在此处了解更多关于 Azure AI 服务的不同身份验证请求。

当您在 Azure AI Studio 界面 中使用 Azure AI 服务时，您的凭据在登录时即已完成身份验证，后台会执行类似的身份验证过程。

📢 **欢迎 Star ⭐ 本仓库，获取更多 AI 资源！**
