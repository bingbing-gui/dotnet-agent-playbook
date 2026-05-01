微软在 **Microsoft Foundry** 中提供了一套完整的 **Agent Runtime 服务**，并结合 **Microsoft Agent Framework (MAF)**，让开发者可以更方便地构建、部署和运行 AI Agent。

本文通过一个简单的示例，使用 **C# + Microsoft Foundry Agents** 创建一个“江湖说书人”Agent，并逐步演示以下内容：

- 创建 Agent
- 管理 Agent 版本
- 获取最新版本 Agent
- 创建会话（Session）
- 运行多轮对话

整个示例代码非常精简，但已经涵盖了一个 Agent 的基本生命周期。

---

## 一、什么是 Microsoft Foundry Agents

Microsoft Foundry 提供了一种 **Agent Runtime 服务**，用于在云端运行 AI Agent。

从概念上看，一个 Agent 可以理解为：

> **模型 + 指令 + 工具 + 会话**

开发者可以通过 SDK 创建 Agent，并在服务器端进行管理和版本控制。

在 Foundry 中，一个 Agent 通常包含以下几个核心概念：

| 概念 | 说明 |
|---|---|
| Agent | AI 智能体 |
| Agent Version | Agent 的版本 |
| Session | 对话会话 |
| Run | 执行一次任务 |

Agent 的版本格式通常为：

```
<agentName>:<version>
```

例如：

```
JokerAgent:1
JokerAgent:2
```

---

## 二、创建 Project Client

首先我们在Microsoft中创建一个Foundry创建一个项目，我这里引用之前创建的项目。

图片


有了Project之后，我们拿到这个地址来初始化一个 **AIProjectClient**，用于与 Microsoft Foundry 服务进行通信。

```csharp
var endpoint = "https://maf.services.ai.azure.com/api/projects/maf";
var deploymentName = "gpt-4o";

var aiProjectClient = new AIProjectClient(
    new Uri(endpoint),
    new DefaultAzureCredential()
);
```

这里使用的是：

```
DefaultAzureCredential
```
关于更多的认证方式请查看 ----地址
这种认证方式在开发阶段非常方便，可以自动使用当前环境中的凭据，例如：

- Azure CLI 登录凭据
- Visual Studio 登录凭据
- Managed Identity

在生产环境中，通常建议改为使用 **ManagedIdentityCredential**，以减少不必要的凭据探测过程。

---

## 三、定义 Agent

接下来我们定义一个 **Prompt Agent**：

```csharp
var agentVersionCreationOptions =
    new AgentVersionCreationOptions(
        new PromptAgentDefinition(model: deploymentName)
        {
            Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。"
        });
```

这里的 `Instructions` 相当于 Agent 的 **系统提示词（System Prompt）**，用于定义 Agent 的角色和行为方式。

在本例中，我们为 Agent 设定的角色是：

> 一位擅长讲段子的江湖说书人。

---

## 四、创建 Agent Version

接下来在 Foundry 中创建一个 Agent：

```csharp
const string JokerName = "JokerAgent";
var createdAgentVersion =
    aiProjectClient.Agents.CreateAgentVersion(
        agentName: JokerName,
        options: agentVersionCreationOptions
    );
```
创建成功后，Foundry 会生成一个版本，例如：
```
JokerAgent:1
```

返回对象为：

```
AgentVersion
```

该对象包含以下关键字段：

| 属性 | 含义 |
|---|---|
| Id | AgentName:Version |
| Name | AgentName |
| Version | 版本号 |

---

## 五、将 AgentVersion 转换为 AIAgent

`AgentVersion` 是服务器端对象，如果要调用 Agent，需要将其转换为 **AIAgent**：

```csharp
AIAgent existingJokerAgent =
    aiProjectClient.AsAIAgent(createdAgentVersion);
```

`AIAgent` 是 Microsoft Agent Framework 中的核心对象，用于执行 Agent 的运行逻辑。

---

## 六、创建新的 Agent 版本

如果再次创建同名 Agent，并提供新的定义：

```csharp
AIAgent newJokerAgent =
    await aiProjectClient.CreateAIAgentAsync(
        name: JokerName,
        model: deploymentName,
        instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。"
    );
```

系统会自动创建新的版本，例如：

```
JokerAgent:2
```

Microsoft Foundry 会自动维护版本号，不需要开发者手动管理。

---

## 七、获取最新版本 Agent

如果只提供 Agent 名称：

```csharp
AIAgent jokerAgentLatest =
    await aiProjectClient.GetAIAgentAsync(name: JokerName);
```

SDK 会返回该 Agent 的 **最新版本**。

例如：

```
JokerAgent:2
```

我们也可以通过 `GetService` 获取当前版本信息：

```csharp
var latestAgentVersion =
    jokerAgentLatest.GetService<AgentVersion>();

Console.WriteLine($"最新Agent版本号Id: {latestAgentVersion.Id}");
```
输出示例：
```
Latest agent version id: JokerAgent:2
```
---
## 八、创建对话 Session

接下来创建一个 **Session**：

```csharp
AgentSession session =
    await jokerAgentLatest.CreateSessionAsync();
```

Session 的作用是：

> 保存对话上下文

这意味着 Agent 可以记住之前的对话内容，从而实现 **多轮对话**。

---

## 九、运行 Agent

现在可以调用 Agent：

```csharp
Console.WriteLine(
    await jokerAgentLatest.RunAsync(
        "给我讲一个发生在茶馆里的段子，轻松一点的那种。",
        session
    ));
```

Agent 会根据之前定义的角色生成回复。

例如：




---

## 十、多轮对话

如果继续使用同一个 Session：

```csharp
Console.WriteLine(
    await jokerAgentLatest.RunAsync(
        "现在把这个段子重新输出一次，加上一些表情符号。",
        session
    ));
```

Agent 会基于上一轮对话继续回答。

这就是 **多轮对话能力**。

---

## 十一、删除 Agent

如果需要清理 Agent，可以按名称删除：

```csharp
aiProjectClient.Agents.DeleteAgent(existingJokerAgent.Name);
```

需要注意的是：

> 删除 Agent 会同时删除该名称下的所有版本。

例如：

```
JokerAgent:1
JokerAgent:2
```

都会被删除。

---

## 总结

通过 Microsoft Foundry 和 Microsoft Agent Framework，我们可以快速构建一个可运行的 AI Agent。

整体流程如下：

```
创建 Client
      ↓
创建 Agent Version
      ↓
获取 Agent
      ↓
创建 Session
      ↓
运行 Agent
      ↓
多轮对话
```

相比直接调用 LLM API，Agent 架构提供了更多能力，例如：

- 版本管理
- 会话管理
- Agent 编排
- 工具调用

这使得 AI 应用的开发更加结构化，也更适合构建复杂的 AI 系统。

---

# 附：Persistent VS Foundry Agent 模式对比

| 对比项 | Persistent Agent | Foundry Agent |
|---|---|---|
| 创建方式 | `PersistentAgentsClient` | `AIProjectClient.Agents` |
| ID格式 | `asst_xxxxx` | `agentName:version` |
| Portal显示 | ✔ 可见 | ✖ 不显示 |
| 版本管理 | 无版本 | ✔ Agent Version |
| 调试方式 | Portal UI | SDK |
| 会话 | Thread / Run | Session |
| 定位 | Chat Agent | Workflow Agent |
| 工具 | Tools | Tools |
| 文件 | Files | Tools / connectors |
| 多Agent | ❌ | ✔ |
| 编排 | ❌ | ✔ |
| 适合场景 | 聊天机器人 | AI系统 |
