## 1. 什么是 AI 智能体（AI Agent）？

AI 智能体是一种软件实体，旨在通过接收输入、处理信息并采取行动，来自主或半自主地完成特定目标。

- 智能体可以发送和接收消息，并通过模型、工具、人类输入或其他可定制组件来生成响应。
- 智能体被设计为协作工作，通过交互实现复杂工作流。
- Agent Framework 支持构建简单或复杂的智能体，提升系统的模块化和可维护性。

---

## 2. AI 智能体能解决什么问题？

**主要价值：**

- **模块化组件**：为特定任务定义不同类型的智能体（如数据抓取、API 交互、自然语言处理），应用可随需求变化快速扩展。
- **协作能力**：多个智能体协作完成复杂任务（如数据收集 → 分析 → 决策），形成分布式智能。
- **人机协作**：支持“人类在环”，智能体提供建议，人类审核与调整，提高生产力。
- **流程编排**：跨系统、工具、API 协调任务，实现端到端自动化。应用场景如云编排、应用部署、内容创作等。

---

## 3. 何时应该使用 AI 智能体？

智能体具备更强的自主性、灵活性和交互性，适用场景包括：

- **自主决策与适应性**：机器人系统、自动驾驶、智能家居
- **多智能体协作**：供应链管理、分布式计算、群体机器人
- **目标驱动与交互性**：虚拟助手、游戏 AI、任务规划器

---

## 4. Agent Framework 的设计目标

- **核心基础**：实现智能体功能的核心基石
- **多智能体协作**：不同类型智能体可在一个对话中协作，并结合人类输入
- **多会话支持**：一个智能体可同时参与并管理多个并发对话

---

## 5. Agent 核心概念与类型

### (1) Agent 抽象类

- 所有智能体的基础抽象，提供可扩展结构
- 可直接调用执行任务，或通过编排模式组织

### (2) Agent 类型

- `ChatCompletionAgent`
- `OpenAIAssistantAgent`
- `AzureAIAgent`
- `OpenAIResponsesAgent`
- `CopilotStudioAgent`

---

## 6. Agent Thread（智能体线程）

- AgentThread 是对话或会话状态的抽象
- 有状态智能体会在服务端存储会话（如 AzureAIAgent）
- 无状态智能体需每次调用时传入完整历史
- 若线程类型不匹配，系统会快速失败并报错

### AgentThread 协作示意图
![](https://files.mdnice.com/user/42214/cadc0a38-3cd9-4d81-8a6f-7f99b6d1b9d5.png)

---

## 7. Agent Orchestration（智能体编排）

> ⚠️ 实验性功能，正在积极开发中。

### 编排模式（Orchestration Patterns）

| 模式 | 特点 |
|------|------|
| **Concurrent（并发）** | 多个智能体并行处理任务 |
| **Sequential（顺序）** | 智能体依次处理，前一个输出作为下一个输入 |
| **Handoff（交接）** | 将任务交接给更合适的智能体 |
| **Group Chat（群聊）** | 多智能体群组协作（替代旧的 AgentGroupChat） |
| **Magentic（磁性）** | 基于上下文动态吸引合适的智能体参与任务 |

**其他特性：**

- 数据转换逻辑：输入/输出在智能体与外部系统间自动适配。
- 人类在环：部分模式允许人工加入判断与干预。

---

## 8. 与 Semantic Kernel 特性的对齐

- Agent Framework 并不是独立存在的，它是 Semantic Kernel 的扩展层，所有能力（消息、插件、函数调用等）都依赖于 Semantic Kernel 的Kernel。
- 复用 Kernel 结构与能力，扩展更高级的自主智能体行为
- 保持一致性，便于开发者迁移已有知识与技能

---

## 9. 插件与函数调用

- 插件（Plugins）扩展 AI 应用功能，引入业务逻辑或特定特性
- 结合函数调用（Function Calling），智能体可动态交互外部服务、执行复杂任务

---

## 10. Agent 消息机制

- 智能体输入与响应基于 Semantic Kernel 内容类型（Content Types）：
    - `ChatHistory`
    - `ChatMessageContent`
    - `KernelContent`
    - `StreamingKernelContent`
    - `FileReferenceContent`
    - `AnnotationContent`
- 便于从传统聊天模式平滑过渡到智能体模式

---

## 11. 模板化（Templating）

- 智能体行为由指令（Instructions）定义，可包含参数模板（值或函数）
- 支持动态替换，生成上下文感知响应
- 可通过 Prompt Template Configuration 配置，确保行为一致性和复用性

---

## 12. 声明式规范（Declarative Spec）

- 声明式规范文档即将发布，未来将提供更标准化的智能体行为定义方式

---

## 13. 安装与依赖

> ⚠️ 除智能体相关包外，还必须引用 `Microsoft.SemanticKernel` 核心库

| 包名                                         | 描述                               |
| -------------------------------------------- | ---------------------------------- |
| Microsoft.SemanticKernel                     | 核心库，必须显式引用               |
| Microsoft.SemanticKernel.Agents.Abstractions | 智能体抽象层（通常由其他包引入）   |
| Microsoft.SemanticKernel.Agents.Core         | 包含 ChatCompletionAgent           |
| Microsoft.SemanticKernel.Agents.OpenAI       | 提供 OpenAIAssistantAgent          |
| Microsoft.SemanticKernel.Agents.Orchestration| 提供智能体编排功能                 |


![](https://files.mdnice.com/user/42214/7f0a01d9-1b05-4291-be16-39f4042670b5.png)
