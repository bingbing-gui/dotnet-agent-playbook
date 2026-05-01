## 使用 Microsoft Foundry  实现持久化 Agents

当我们构建一个 AI Agent 时，如果希望 Agent 具备“记忆能力”，通常需要维护对话上下文。然而，在传统模式下，应用需要自行管理会话状态和历史记录。

Microsoft Foundry Agents 提供了服务端持久化能力，可以在平台侧管理 Agent 的生命周期、会话状态以及上下文信息，从而简化应用层的实现。

## 场景简介

我们创建一个Agent， 如果让Agent产生记忆，必须维持上下文回话，那么有没有别的方式来维持上下文回话，我们可以使用Azure Foundry Agents提供的持久化能力，来管理这个Agent的生命周期和交互状态。

- 创建或获取持久化 Agent
- 配置模型与系统指令（instructions）
- 创建会话并执行对话
- 清理示例资源

## 核心流程说明

### 环境变量&配置

首先读取 Microsoft Foundry 项目的 endpoint 以及模型部署名称（deployment name）。

```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
```

### 客户端初始化

通过 Azure.AI.Agents.Persistent SDK 提供的 `PersistentAgentsClient`，并结合 Azure 标准身份认证机制（如 `DefaultAzureCredential`），初始化用于与 Azure Foundry Agents 服务端交互的客户端对象。

```csharp
var persistentAgentsClient = new PersistentAgentsClient(endpoint, new DefaultAzureCredential());
```

### 创建或获取持久代理

#### 创建代理元信息

可以通过 `persistentAgentsClient.Administration.CreateAgentAsync` 创建一个具备特定模型和指令（instructions）的代理。

#### 获取具体的 AIAgent 实例

支持两种方式：

- 先创建，再通过 id 获取为 AIAgent
- 直接创建并作为 AIAgent 返回

### 与代理交互

- 使用 `AIAgent.CreateSessionAsync()` 表示一次独立的对话会话，用于隔离不同用户或不同任务的上下文。
- 使用 `AIAgent.RunAsync()` 发送用户请求并获取回复：

```csharp
AgentSession session = await agent1.CreateSessionAsync();
Console.WriteLine(await agent1.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", session));
```

### 代理清理

为了避免资源浪费，在示例代码中通过 DeleteAgentAsync 及时清理测试用 Agent。
## 注意事项

- `DefaultAzureCredential` 适合开发测试，生产环境推荐更精细化的身份认证（如 Managed Identity）以避免安全隐患。
- 持久代理适合在需要长期保存交互能力或分布式多实例场景（如多用户Bot、企业级Agent Pool）下使用。

## 场景扩展

- 可以扩展 instructions，"定制"化生成特定角色代理，如客服、小助手等。
- 利用代理元数据，集中管理企业级大量 AI 实例。
- 持久化 Agent 会在 Azure 服务端创建资源，建议合理管理 Agent 生命周期。

## 总结

Microsoft Foundry 提供的持久化 Agent 能力，使开发者可以在云端统一管理 Agent 的生命周期和会话状态。
相比传统的模型调用方式，这种模式可以更好地支持多用户会话、Agent 复用以及分布式系统中的统一管理。
对于需要长期运行、具备上下文记忆能力的 AI 应用来说，这种架构可以显著降低开发复杂度。