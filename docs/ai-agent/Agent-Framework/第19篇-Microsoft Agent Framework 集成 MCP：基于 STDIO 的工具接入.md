

围绕 Agent 的讨论经常停留在“Prompt 写法”或“模型选择”，但在工程实现层面，Agent 的关键并不在对话能力，而在**工具接入、执行编排、状态管理与可治理性**。

本文从知识体系解释两个关键组件：

- **Model Context Protocol（MCP）**：工具层协议（Tool / Context Protocol Layer）
- **Microsoft Agent Framework（Microsoft.Agents.AI / Microsoft.Extensions.AI）**：Agent 编排层（Orchestration Layer）

---

## MCP 是什么：工具层标准协议

MCP 是一个开放协议，用于标准化模型（LLM）与外部工具/上下文的连接方式。它解决的是工具接入的一致性问题：工具如何被发现、被调用、被隔离。

> 除 tools 外，MCP 生态中还常见 resources / prompts 等上下文能力（具体取决于 MCP Server 实现）。


在没有 MCP 的情况下，工具接入通常会退化为胶水工程：
- 每个工具都要单独封装 function calling schema
- 每个工具都要单独处理鉴权、参数校验、错误语义
- 工具返回值格式不一致，模型侧需要大量适配

MCP 用协议消解这些重复劳动，让工具以统一方式被发现和调用。

## MCP 的核心能力边界

### 工具发现（Tool Discovery）
系统可以通过协议接口获取工具列表，工具成为“可被发现”的资源，而不是写死在 prompt 或代码里。直接收益：
- 工具接入从“代码级集成”变成“协议级对接”
- 工具能力变动可通过 server 侧演进，client 侧按协议消费

### 工具调用（Tool Invocation）
工具参数与返回结果具备结构化 schema。模型调用工具不依赖自然语言猜测，而依赖协议描述，更稳定。

### 工具隔离（Tool Isolation）

MCP 鼓励工具在 Server 侧实现，从而实现运行时隔离与权限治理。常见部署形态包括：

- **stdio**：本地 transport  
- **HTTP + SSE**：远程 transport  

隔离是 MCP 推动的工程实践特性，不是协议强制，但几乎是默认形态。

## 从协议到实践：用 GitHub MCP Server 落地 MCP

上面我们从协议层定义 MCP：它统一工具发现、工具调用与运行隔离。接下来我们用一个真实可运行的示例把这些概念落到工程实现上。

选择 GitHub MCP Server 的原因是：GitHub 场景具备典型的“外部系统工具调用特征”，包括鉴权、分页、资源读取、PR/Commit 查询等，非常适合作为 MCP tool calling 的参考实现。

本文将通过 GitHub MCP Server（`@modelcontextprotocol/server-github`）提供的工具集，让 Agent 具备“读取仓库提交记录并总结”的能力。

**示例目标：**  
让 Agent 基于 GitHub MCP tools，查询 `microsoft/semantic-kernel` 仓库最近提交并总结输出。

该示例将完整覆盖 MCP 的三个关键步骤：

1. 启动 MCP Server（stdio transport）
2. **Tool Discovery**：运行时拉取 tools 列表
3. **Tool Calling**：模型自动选择工具并调用 GitHub API

## 使用 Microsoft Agent Framework 调用 MCP 工具

### 环境变量的配置
```csharp
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
```
- AZURE_OPENAI_ENDPOINT：调用 Azure OpenAI 服务的地址，未配置时抛异常提醒。
- AZURE_OPENAI_DEPLOYMENT_NAME：指定默认的模型部署，未配置时默认使用 gpt-4o-mini。



---

### 开启 MCP 客户端

对于 Agent 来说，工具（tools）不是写死在代码里，而是由 MCP Server 以统一标准暴露出来，Agent 运行时再进行工具发现（Tool Discovery）并绑定。

```csharp
await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-github"],
}));
```
说明：
- `Command = "npx"`：通过 npx 启动 MCP Server
- `@modelcontextprotocol/server-github`：GitHub MCP Server
- `-y`：自动确认执行
- `--verbose`：输出详细日志，便于排查

关键点：程序并不是连接远程 URL 的 MCP Server，而是在本机启动一个 Node 进程作为 MCP Server，并通过 stdio 通信。

---

### 获取工具集
```csharp
var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);
```
MCP Server 会暴露一组 tools，运行时通过工具发现（Tool Discovery）获取列表，后续通过 tool calling 调用。

ListToolsAsync() 会向 MCP Server 发送 tools/list 请求
MCP Server 返回 tools 的 schema（名称、描述、参数结构等）
这些 schema 会被映射为 AI Agent 可用的 AITool

---

### 创建 AI Agent
```csharp
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetChatClient(deploymentName)
     .AsAIAgent(instructions: "你是一个只回答与GitHub仓库相关问题的AI助手。", tools: [.. mcpTools.Cast<AITool>()]);
```
- 创建 Azure OpenAI Client，并使用 AzureCliCredential 认证
- 获取 ChatClient（模型对话客户端）
- 将 ChatClient 包装为 AIAgent，并注入 MCP tools

关键参数：
- `instructions`：角色边界（回答范围与行为约束）
- `tools`：绑定工具列表，赋予 Agent 工具调用能力

---

### 运行 Agent（自然语言触发任务）
```csharp
Console.WriteLine(await agent.RunAsync("总结一下 microsoft/semantic-kernel 仓库的最近四次提交？"));
```
调用 `RunAsync` 以自然语言触发任务，Agent 结合 MCP 工具与模型完成指令并输出结果。




## 补充说明：MCP GitHub Server 启动失败排查

如果你遇到下面那问题，说明  MCP GitHub Server 进程没启动成功（npx @modelcontextprotocol/server-github 启动失败），所以
.CreateAsync(...)  
直接抛异常。  

图片说明了这个错误。

## 排查步骤

1）先确认你电脑 Node/NPM 能正常用  
打开 PowerShell / CMD：
```
node -v
npm -v
npx -v
```
图片

必须都有版本输出。

2）先在命令行单独启动 MCP Server（确认它能跑）
```
npx -y --verbose @modelcontextprotocol/server-github
```
如果这一步都报错，那就是 npm 环境问题

图片

## 解释
npm 默认把全局可执行文件放在：
```
C:\Users\<用户名>\AppData\Roaming\npm
```
但你的机器上这个目录  
不存在  
所以 npx 在执行时去访问这个路径 →  
lstat  
失败 → 直接退出

## 解决方案（最简单：创建目录即可）
请你在 PowerShell 执行：
```
mkdir $env:APPDATA\npm -Force
mkdir $env:APPDATA\npm-cache -Force
```
图片

然后检查 npm 的 prefix 是否正确
```
npm config get prefix
npm config get cache
```

重新设置 prefix/cache（推荐强制设置一次）
```
npm config set prefix "$env:APPDATA\npm"
npm config set cache "$env:LOCALAPPDATA\npm-cache"
```

再跑一次 npx 测试
```
npx -y --verbose @modelcontextprotocol/server-github
```

---

## 总结

本文从工程视角澄清了两个关键事实：

1. **MCP 并不是 Agent 框架**，而是一种用于标准化工具与上下文接入的开放协议。它把工具接入从“每个工具单独集成”提升为“按协议统一消费”，核心能力体现在 Tool Discovery、Tool Invocation 以及运行时隔离与权限治理的工程实践上。

2. **Microsoft Agent Framework（MAF）承担的是编排层职责**：负责把模型推理、工具调用、对话线程与执行流程串成一个可运行、可观测、可扩展的 Agent Runtime。

通过 GitHub MCP Server 的示例可以看到：一个工程级 Agent 的关键不在 Prompt 写法，而在于“工具以协议方式被发现并注入”，并在运行时由模型自动选择调用，最终形成可落地的工具型智能体。

