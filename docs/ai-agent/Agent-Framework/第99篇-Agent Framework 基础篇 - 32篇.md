# Agent Framework 基础篇 - 32篇

> 说明：以下标题提取自 `docs/ai-agent/Agent-Framework`，并和 `src/ai-agent/Agent-Framework` 做当前版本映射。  
> GitHub 仓库前缀：`https://github.com/bingbing-gui/dotnet-platform`

| 篇号 | 文档标题 | 代码路径（GitHub） |
| --- | --- | --- |
| 第1篇 | 使用Agent Framework构建你的第一个Agent 应用 | [src/ai-agent/Agent-Framework/01-Agent-Running](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/01-Agent-Running) |
| 第2篇 | Agent Thread实现同一Agent的多轮回话 | [src/ai-agent/Agent-Framework/02-Multiturn-Conversation](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/02-Multiturn-Conversation) |
| 第3篇 | Agent Framework调用工具 | [src/ai-agent/Agent-Framework/03-FunctionTools](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/03-FunctionTools) |
| 第4篇 | Agent Framework的人工审批机制，确保本地函数调用安全可控 | [src/ai-agent/Agent-Framework/04-FunctionTools-WithApprovals](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/04-FunctionTools-WithApprovals) |
| 第5篇 | Agent Framework结构化数据 | [src/ai-agent/Agent-Framework/05-StructuredOutput](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/05-StructuredOutput) |
| 第6篇 | Agent-Framework实现Agent会话持久化 | [src/ai-agent/Agent-Framework/06-PersistedConversations](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/06-PersistedConversations) |
| 第7篇 | Agent Framework链接外部存储资源 | [src/ai-agent/Agent-Framework/07-StorageConversations](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/07-StorageConversations) |
| 第8篇 | 10行代码搞定Agent的全链路监控 | [src/ai-agent/Agent-Framework/08-AgentObservability](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/08-AgentObservability) |
| 第9篇 | 使用依赖注入构建Agent | [src/ai-agent/Agent-Framework/09-DI](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/09-DI) |
| 第10篇 | 将Agent暴露为mcp工具供第三方安全调用 | [src/ai-agent/Agent-Framework/10-AgentAsMcpTools](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/10-AgentAsMcpTools) |
| 第11篇 | Agent Framework构建视觉Agent | [src/ai-agent/Agent-Framework/11-Vision-Agent](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/11-Vision-Agent) |
| 第12篇 | Agent Framework构建可组合的多agent系统 | [src/ai-agent/Agent-Framework/12-Agent-As-Function-Tool](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/12-Agent-As-Function-Tool) |
| 第13篇 | 不阻塞、不等待：让Agent 像后台服务一样持续运行 | [src/ai-agent/Agent-Framework/13-Backgroud-Response-With-Tool-And-Persistence](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/13-Backgroud-Response-With-Tool-And-Persistence) |
| 第14篇 | Agent Framework 中的 Middleware 设计：从 HTTP Pipeline 到 AI Agent Pipeline | [src/ai-agent/Agent-Framework/14-Agent-Middleware](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/14-Agent-Middleware) |
| 第15篇 | Agent Framework中 IChatReducer 进行聊天记录缩减 | [src/ai-agent/Agent-Framework/15-ChatReduction](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/15-ChatReduction) |
| 第16篇 | 如何用 Plugins 和依赖注入为 AI Agent 装上外挂 | [src/ai-agent/Agent-Framework/16-Plugins](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/16-Plugins) |
| 第17篇 | Agent Framework 中构建声明式 (Declarative) AI Agent | [src/ai-agent/Agent-Framework/17-Declarative-Agent](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/17-Declarative-Agent) |
| 第18篇 | 什么样的 Agent，才算一个真正的 AI Agent | [src/ai-agent/Agent-Framework/18-Agent-MCP-Server-Stdio](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/18-Agent-MCP-Server-Stdio) |
| 第19篇 | Microsoft Agent Framework 集成 MCP：基于 STDIO 的工具接入 | [src/ai-agent/Agent-Framework/19-A2AProtocal](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/19-A2AProtocal) |
| 第20篇 | Agent-To-Agent协议 | [src/ai-agent/Agent-Framework/20-Agent-Skill](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/20-Agent-Skill) |
| 第21篇 | 使用 Microsoft Foundry  实现持久化 Agents | [src/ai-agent/Agent-Framework/22-Persistent-Agent](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/22-Persistent-Agent) |
| 第22篇 | 使用Microsoft Agent Framework与Microsoft Foundry 构建持久化 AI Agent（AIProjectClient） | [src/ai-agent/Agent-Framework/23-Persistent-Agent-AIProject](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/23-Persistent-Agent-AIProject) |
| 第23篇 | OpenAI API 调用模式对比：ChatCompletions vs Response API | [src/ai-agent/Agent-Framework/24-OpenAI-API-Patterns](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/24-OpenAI-API-Patterns) |
| 第24篇 | Agent Framework 集成 GitHub Copilot SDK，实现 AI 自动操控你的电脑 | [src/ai-agent/Agent-Framework/25-GitHubCopilotSDK](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/25-GitHubCopilotSDK) |
| 第25篇 | Agent Framework 接入 Ollama（本地模型实践记录） | [src/ai-agent/Agent-Framework/27-Agent-On-Ollama](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/27-Agent-On-Ollama) |
| 第26篇 | 从 MCP 到 Skill基于FileBased Skill与 Agent Framework 的实践探索 | [src/ai-agent/Agent-Framework/28-FileBased-Agent-Skills](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/28-FileBased-Agent-Skills) |
| 第27篇 | 基于CodeDefined Skill与 Agent Framework 的实践探索 | [src/ai-agent/Agent-Framework/29-CodeDefined-Agent-Skills](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/29-CodeDefined-Agent-Skills) |
| 第28篇 | 基于ClassBased Skill与 Agent Framework 的实践探索 | [src/ai-agent/Agent-Framework/30-ClassBased-Agent-Skills](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/30-ClassBased-Agent-Skills) |
| 第29篇 | 基于FileBased和CodeBased和ClassBased组合Skills与Agent Framework的实践探索 | [src/ai-agent/Agent-Framework/31-Mixed-Agent-Skills](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/31-Mixed-Agent-Skills) |
| 第30篇 | 在 Agent Framework 中为 Agent Skill 接入依赖注入 DI | [src/ai-agent/Agent-Framework/32-AgentSkill-Integration-DI](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/32-AgentSkill-Integration-DI) |
| 第31篇 | AgentFramework接入原生DeepSeek-v4-pro | [src/ai-agent/Agent-Framework/33-Agent-Providers-DeepSeek](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/33-Agent-Providers-DeepSeek) |

## 当前未直接映射的代码目录

- [src/ai-agent/Agent-Framework/21-Agent-Providers-Anthropic](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/21-Agent-Providers-Anthropic)
- [src/ai-agent/Agent-Framework/26-Agent-On-ONNX](https://github.com/bingbing-gui/dotnet-platform/tree/main/src/ai-agent/Agent-Framework/26-Agent-On-ONNX)
