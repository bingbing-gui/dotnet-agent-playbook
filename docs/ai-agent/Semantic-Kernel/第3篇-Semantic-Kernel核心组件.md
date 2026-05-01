
Semantic Kernel 提供了丰富的核心组件，这些组件既可独立使用，也可组合协作。本文将对各组件进行概述，并解释它们之间的关系。

---

## 1. Kernel（核心调度器）

Kernel 是 Semantic Kernel 的“大脑”，主要负责：

- 管理和协调 AI 服务连接器、插件函数、向量存储、内存等资源。
- 执行提示模板的渲染和调用。
- 处理过滤器（Filters）的调用。

可以将 Kernel 理解为运行时调度中心，所有 AI 任务最终都通过 Kernel 协调。

---

## 2. AI 服务连接器（AI Service Connectors）

AI 服务连接器为不同 AI 服务提供统一的抽象接口，支持：

- 聊天补全
- 文本生成
- 嵌入生成
- 文本转图像
- 图像转文本
- 文本转音频
- 音频转文本

> 默认情况下，Kernel 只会使用 Chat Completion 或 Text Generation。若需调用嵌入、向量搜索、图像生成等服务，需要手动注册并调用。

---

## 3. 向量存储连接器（Vector Store Connectors）

向量存储连接器通过通用接口暴露不同提供商的向量存储能力。Kernel 不会自动使用已注册的向量存储，但可通过插件将**向量搜索（Vector Search）**暴露给 Kernel，供 Prompt Templates 和 Chat Completion AI Model 使用。

---

## 4. Memory（记忆）

Memory 是 Semantic Kernel 的“记忆系统”，用于在任务和对话中保存信息，分为：

- **Volatile Memory（易失性记忆）**：会话或任务期间保存的上下文信息，程序结束后不持久化。
- **Semantic Memory（语义记忆）**：基于向量存储的长期记忆，常用于对话历史或 RAG 场景下知识库检索。

Memory 通常依赖 Vector Store Connectors 实现语义搜索，与向量存储紧密结合。

---

## 5. 函数和插件（Functions and Plugins）

插件（Plugins）是一组命名的功能模块，每个插件包含一个或多个可调用函数。注册到 Kernel 后，插件可被：

1. **AI 直接调用**：AI 在对话中自主选择并执行插件函数，如调用 API、查询数据库等。
2. **模板渲染时调用**：在提示模板中直接调用插件函数，增强 AI 生成能力。

插件常见形式：

- 本地代码（如 .NET、Python、Java）
- OpenAPI 规范服务
- ITextSearch 实现（RAG 场景）
- 提示模板（Prompt Templates）

---

## 6. Planner（规划器）

Planner 是“任务规划器”，可将自然语言请求分解为步骤，并自动调用函数或插件完成任务。常见类型：

- **ActionPlanner**：将请求映射为单个函数调用。
- **StepwisePlanner**：将复杂请求拆解为多步骤，逐步执行并生成后续计划。

Planner 让 AI 能自主选择和组合函数，简化开发流程，是实现多 Agent 协作和自动化任务的核心机制。

---

## 7. 提示模板（Prompt Templates）

提示模板由开发者或提示工程师创建，结合上下文、指令、用户输入和函数输出，帮助 AI 生成更精准回答。模板可包含：

- AI 的思考或回答方式
- 用户输入占位符（如 `{用户问题}`）
- 插件或函数调用

**使用方式：**

1. 作为 Chat Completion 的起点，先渲染模板，再调用 AI。
2. 注册为插件函数，像普通函数一样被调用。

**执行流程示意：**

- 模板 A 注册为插件，模板 B 由用户输入并依赖 A。
- 渲染 B 时先渲染 A，A 的结果传递给 AI，B 得到 A 的数据后继续渲染，最终返回结果。

AI 也可在后台自动调用插件，用户无需感知。

**优势：**

- 让 AI 通过自然语言调用功能，无需编程。
- AI 可单独思考每个功能，提高成功率。

---

## 8. 过滤器（Filters）

过滤器可在聊天补全流程中，在特定事件前后执行自定义操作，如：

- 函数调用前后
- 提示渲染前后

过滤器需注册到 Kernel。提示模板执行前会被转换为 KernelFunctions，因此函数过滤器和提示过滤器都会被调用。多个过滤器嵌套执行，函数过滤器为外层，提示过滤器为内层。

---

## 总结

Semantic Kernel 的核心组件协同工作：

- **Kernel**：中枢大脑，负责调度和协调
- **AI 服务连接器 & 向量存储**：提供底层能力
- **Memory**：赋予系统记忆
- **Functions & Plugins、Prompt Templates**：赋予 AI 技能和表达方式
- **Planner**：实现自主任务规划
- **Filters**：提供可插拔扩展点

通过这些组件协作，Semantic Kernel 支持从简单对话到复杂 AI 应用和多步骤工作流的构建。
