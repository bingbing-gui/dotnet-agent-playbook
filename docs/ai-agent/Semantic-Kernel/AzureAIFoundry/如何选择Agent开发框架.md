# 智能代理开发的方案

开发者可以通过多种方式构建 AI 代理，例如使用不同的框架和 SDK。

## Azure AI Agent 服务

Azure AI Agent 服务是 Azure 中的一项托管服务，旨在为在 Azure AI Foundry 中创建、管理和使用 AI 代理提供框架。该服务基于 OpenAI Assistants API 构建，但提供了更多的模型选择、更强的数据集成能力以及企业级安全保障；使你能够同时使用 OpenAI SDK 和 Azure Foundry SDK 来开发智能代理解决方案。

[Azure AI Agent 相关资料](https://learn.microsoft.com/en-us/azure/ai-services/agents/)

## OpenAI Assistants API

OpenAI Assistants API 提供了 Azure AI Agent 服务的一部分功能，且只能与 OpenAI 模型一起使用。在 Azure 中，你可以通过 Azure OpenAI 服务使用 Assistants API，但实际上，Azure AI Agent 服务在代理开发方面提供了更高的灵活性和更强的功能。

[OpenAI Assistants API 相关资料](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/assistant)

## Semantic Kernel

Semantic Kernel 是一个轻量级的开源开发工具包，可用于构建 AI 代理并编排多智能体解决方案。核心的 Semantic Kernel SDK 适用于各种生成式 AI 开发，而 Semantic Kernel Agent Framework 则是一个专为创建代理和实现代理解决方案模式而优化的平台。

[Semantic Kernel相关资料](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/?pivots=programming-language-csharp)

## AutoGen

AutoGen 是一个开源框架，用于快速开发代理（Agents）。在进行代理相关的研究与创意探索时，它是一个非常有用的工具。

[AutoGen相关资料](https://microsoft.github.io/autogen/stable/index.html)

## Microsoft 365 Agents SDK

开发者可以使用 Microsoft 365 Agents SDK 创建自托管的代理，并通过多种渠道进行交付。尽管名称中带有 Microsoft 365，但使用该 SDK 构建的代理并不限于 Microsoft 365，也可以通过 Slack、Messenger 等渠道进行交付。

[Microsoft 365 Agents SDK相关资料](https://learn.microsoft.com/en-us/microsoft-365/agents-sdk/)

## Microsoft Copilot Studio

Microsoft Copilot Studio 提供了一个低代码开发环境，非专业开发人员可以使用它快速构建和部署与 Microsoft 365 生态系统或常用渠道（如 Slack 和 Messenger）集成的代理。Copilot Studio 的可视化设计界面使其成为在缺乏专业开发经验的情况下构建代理的理想选择。

[Microsoft Copilot Studio相关资料](https://learn.microsoft.com/en-us/microsoft-copilot-studio/)

## Microsoft 365 Copilot 中的 Copilot Studio 代理构建器

企业用户可以使用 Microsoft 365 Copilot 中的声明式 Copilot Studio 代理构建工具，来创建用于常见任务的基础代理。该工具的声明式特性使用户能够通过描述所需功能来创建代理，也可以使用直观的可视化界面为其代理指定各类选项。

[Microsoft 365 Copilot 相关资料](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/copilot-studio-agent-builder-build)

## 如何选择适合自己的解决方案

随着可用的工具和框架日益丰富，选择合适的代理开发路径可能变得复杂。以下是根据不同用户背景和使用场景整理的建议，涵盖当前主流的代理开发框架：

---

### 🧑‍💼 业务用户（无开发经验）

- 使用工具：**Copilot Studio（在 Microsoft 365 Copilot Chat 中）**
- 功能特性：
  - 提供声明式代理构建器
  - 通过可视化界面自动化常见业务任务（如填表、查询、通知）
- 优势：
  - 无需编程经验，快速上手
  - 对 IT 部门影响较小，便于在组织内推广

---

### 🛠️ 技术型业务用户（具备低代码能力）

- 使用工具：**Copilot Studio + Microsoft Power Platform**
- 功能特性：
  - 结合低代码技能与业务知识，创建更复杂的代理流程
  - 可扩展 Microsoft 365 Copilot 能力
  - 支持部署到 **Microsoft Teams、Slack、Messenger** 等主流渠道
- 适合：希望自己动手构建个性化代理功能的业务人员

---

### 👨‍💻 专业开发者（扩展 Microsoft 365 Copilot 能力）

- 使用工具：**Microsoft 365 Agents SDK**
- 功能特性：
  - 支持自托管代理
  - 可部署至多个渠道（包括 Slack、Messenger 等）
- 适合：需要更多控制权和扩展性的开发团队

---

### ☁️ 专业开发者（使用 Azure 后端 + 自定义服务）

- 使用工具：**Azure AI Foundry 中的 Azure AI Agent Service**
- 功能特性：
  - 企业级托管服务，支持多模型选择、数据集成、安全控制
  - 同时支持 **OpenAI SDK** 与 **Azure Foundry SDK**
- 开发建议：
  - 适合构建可落地、可部署的**企业级智能代理系统**

---

### 🔗 多智能体系统构建与编排

- 使用工具：**Semantic Kernel**
- 功能特性：
  - 提供函数注册、调用计划、上下文管理、插件系统
  - 可与 Azure AI Agent Service 结合，构建复杂的多代理解决方案
- 适合：需要任务分解、智能协作、工具链整合的多代理应用

---

### 🧪 AI 研究与原型设计

- 使用工具：**AutoGen**
- 功能特性：
  - 面向研究和实验的开源框架
  - 轻松模拟多代理交互、推理、反思、协作等复杂行为
  - 使用 Python 快速搭建原型系统
- 适合：高校、研究机构、原型验证、灵感测试等场景

---

💡 **注意事项**：

- 不同代理开发工具之间功能存在一定重叠。
- 实际选择中，还应考虑以下因素：
  - 团队已有技术栈（C# / Python / 低代码）
  - 系统对部署、安全、集成的要求
  - 是否计划集成 Azure 服务或其他企业资源
