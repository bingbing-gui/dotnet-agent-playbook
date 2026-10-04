
Agent Harness 是一套运行时支撑框架，能够将语言模型转变为可以执行实际任务的智能体。它负责驱动模型调用和工具调用、管理对话状态与上下文、执行审批策略，并让智能体在多步骤任务中持续推进工作。

Agent Framework 提供了一个具有明确设计约定、功能完备且开箱即用的 Harness，适用于研究、编程、数据分析以及其他长时间运行的任务。你只需提供一个聊天客户端，并根据应用需求定制所需的能力。

## 架构

Harness 通过组合现有的 Agent Framework 基础组件来实现，而不是定义一个独立的智能体运行时：

1. **Chat client**：将智能体连接到模型。
2. **Chat pipeline**：添加函数调用、消息注入、每次服务调用的历史记录持久化，以及可选的上下文压缩。
3. **Agent and context providers**：添加会话范围内的指令、工具、记忆、待办事项状态、运行模式以及可选能力。
4. **Middleware and decorators**：添加审批处理、可观测性以及可选的有界循环。
5. **Application UX**：以流式方式输出响应、显示进度，并收集工具审批等输入。

最终生成的对象仍然是一个普通的 Agent Framework 智能体：在 .NET 中，它是派生自 `AIAgent` 的 `HarnessAgent`；


## Harness 能力矩阵

| 能力 | Harness 行为 | 对应指南 |
| --- | --- | --- |
| Function invocation | 已启用，可配置每个请求的迭代次数上限。 | 函数工具 |
| Per-service-call history persistence | 在包含工具调用的运行过程中，每次模型调用后都会持久化历史记录。 | 会话 |
| Compaction | 提供 Token 限制或自定义策略时启用。 | 上下文压缩 |
| Todo tracking | 默认启用。 | 规划与待办事项 |
| Agent modes | 默认启用规划模式和执行模式。 | 规划与待办事项 |
| File memory and file access | 默认启用会话文件记忆；共享文件访问需要显式启用。 | 上下文提供程序 |
| Tool approval | 默认启用持续授权和自动审批规则。 | 工具审批 |
| OpenTelemetry | 默认启用智能体可观测性。 | 可观测性 |
| Web search | 所选聊天客户端支持时，默认添加。 | Web 搜索 |
| Agent Skills | .NET 中默认启用；Python 中需要通过提供程序或路径显式启用。 | 智能体技能 |
| Background agents | 可选地将任务并行委派给具名的子智能体。 | 后台智能体 |
| Shell execution | 通过组合 Shell 包提供；Python 工厂函数可以自动完成接入。 | Shell 工具 |
| Looping | 可选的有界重复调用，由评估器或谓词驱动。 | 智能体循环 |


## 先创建一个 Harness Agent

`Microsoft.Agents.AI.Harness` 包在 `Microsoft.Agents.AI` 命名空间中公开了 `HarnessAgent`。可以通过 `AsHarnessAgent` 从任意 `IChatClient` 创建一个 Harness 智能体，也可以直接构造 `HarnessAgent`：

```csharp
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

AIAgent agent = chatClient.AsHarnessAgent();

AgentSession session = await agent.CreateSessionAsync();

AgentResponse response = await agent.RunAsync(
    "帮我规划一次西雅图周末旅行。",
    session);

Console.WriteLine(response.Text);
```

核心就是 `AsHarnessAgent()`。它基于聊天客户端，创建带有默认运行能力的 Agent。

这里显式创建了一个 `AgentSession`。如果用户后面继续补充需求，调用 `RunAsync` 时要继续传入同一个会话，这样对话历史、待办事项和审批状态才能接续起来。


## 配置 Agent 的指令和上下文

需要进一步调整行为时，可以传入 `HarnessAgentOptions`：

```csharp
AIAgent agent = chatClient.AsHarnessAgent(new HarnessAgentOptions
{
    Name = "research-agent",
    HarnessInstructions = "有明确目的时再调用工具，结论应有已核实的资料支持。",
    ChatOptions = new ChatOptions
    {
        Instructions = "你是一名研究助手，优先使用学术资料。",
    },
    MaxContextWindowTokens = 128_000,
    MaxOutputTokens = 16_384,
});
```

这段配置把两类指令分开了：

- `HarnessInstructions`：描述执行任务时应遵循的通用工作要求，例如如何使用工具、如何处理未经确认的信息。
- `ChatOptions.Instructions`：描述当前 Agent 的职责，例如它是一名优先使用学术资料的研究助手。

框架通过 `HarnessAgent.DefaultInstructions` 提供默认的 Harness 指导。组合指令时，`HarnessInstructions` 会出现在 `ChatOptions.Instructions` 前面。

同时设置了上下文窗口和输出 Token 上限。这会让 Harness 建立默认的上下文压缩策略，并在工具调用循环中的每次模型调用前执行相应的检查。也可以通过 `CompactionStrategy` 提供自己的策略。

这两个数值需要根据实际模型和任务设置。没有同时提供这两个参数，也没有提供自定义策略时，上下文压缩默认不会启用。[上下文压缩说明](https://learn.microsoft.com/en-us/agent-framework/concepts/agents/conversations/compaction#use-compaction-with-harness-agent)

## 自定义组合方式

默认能力具有针对性的配置选项，包括：

- `DisableTodoProvider`
- `DisableAgentModeProvider`
- `DisableFileMemory`
- `DisableAgentSkillsProvider`
- `DisableWebSearch`
- `DisableToolAutoApproval`
- `DisableOpenTelemetry`
- `DisableCompaction`

## 总结

Agent Harness 通过组合 Agent Framework 的聊天客户端、管道、上下文提供程序和中间件，为智能体提供工具调用、会话管理、待办事项、审批和可观测性等运行支持。开发者可以使用 AsHarnessAgent() 创建带有默认配置的智能体，再通过 HarnessAgentOptions 调整指令、Token 限制和功能开关，按需启用上下文压缩等能力，减少运行过程中的重复配置工作。

