在多 Agent 系统中，一个常见问题是：如何将云端 AI Agent 安全、标准化地暴露给其他模型或工具调用？

本文以 Azure AI Foundry + MCP 为例，演示如下内容：
- 创建一个可复用的云端 Persistent Agent
- 将其封装为符合 MCP 标准的工具（MCP Tool）
- 通过 stdio 传输与 JSON-RPC 接口被其他 Agent 或客户端调用

## 1. 核心概念

在开始代码讲解之前，需要理解几个关键角色：

- Persistent Agent（持久化 Agent）：云端长期存在的 AI 实体，具有唯一 ID，可跨会话与客户端复用，状态由服务端管理。
- MCP Tool（MCP 工具）：将 Agent 封装为标准化可调用接口，定义名称、描述与输入模式，供 MCP 客户端通过 JSON-RPC 调用。
- stdio transport（标准输入输出传输）：本地进程级通信通道，使用 stdin/stdout 承载 JSON-RPC 请求与响应，便于开发与调试。

## 2. 环境配置与客户端初始化

首先，代码从环境变量中获取 Azure AI Foundry 的连接信息，并初始化客户端。这里可能稍微和前面系列文章有点差异，
PROJECT_ENDPOINT 是指向 Azure AI Foundry 的端点 URL。

```csharp
using Azure.AI.Agents.Persistent;
using Azure.Identity;
// 使用 Azure CLI 凭据创建持久化 Agent 客户端
var persistentAgentsClient = new PersistentAgentsClient(endpoint, new AzureCliCredential());
```

要点：这里使用了 AzureCliCredential，意味着在运行代码前需要确保本地已通过 Azure CLI（az login）登录。

## 3. 创建服务端 Agent

接下来，代码在 Azure 服务端创建一个具体的 Agent 实例。

```csharp
// 创建一个服务端持久化 Agent
var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: "gpt-4o",
    instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。",
    name: "Joker",
    description: "我是一个擅长讲故事的江湖说书人。");
```

Joker Agent：该 Agent 被设定为一个爱讲笑话的角色。

元数据：name（"Joker"）和 description（"我是一个擅长讲故事的江湖说书人。"）非常重要，它们稍后会直接成为 MCP 工具的名称和描述，供其他 AI 模型识别。

## 4. 获取并转换 Agent

这是最核心的一步：将云端的 Agent 实体转换为本地代码可用的 AIAgent 对象，进而转化为 MCP 工具。

```csharp
// 根据 ID 获取完整的 Agent 对象
AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);
// 核心转换逻辑：
// 1. agent.AsAIFunction() -> 将 Agent 包装为一个可调用的 AI 函数
// 2. McpServerTool.Create(...) -> 将函数封装为符合 MCP 标准的工具
McpServerTool tool = McpServerTool.Create(agent.AsAIFunction());
```
MCP 并不关心 Agent 内部是 Prompt、Chat 还是 RAG，只要求它“像函数一样可调用”。
AsAIFunction() 正是完成了这层“语义降维”。

## 5. 注册 MCP 服务器并运行

最后，利用 .NET 的通用主机模式（Host Builder）启动一个 MCP 服务器。

```csharp
// 注册 MCP 服务器，使用 stdio 传输协议，并挂载我们刚刚创建的工具
HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools([tool]); // 将 "Joker" 工具注册到服务器中

// 启动并运行
await builder.Build().RunAsync();
```

stdio 通信：程序运行后，会监听标准输入（stdin）的 JSON-RPC 消息，并将结果输出到标准输出（stdout）。这是 MCP 客户端（如 Claude Desktop 或其他 Agent 编排器）与该工具交互的方式。

## 6. 用 stdio 的方式调用 Agent 工具

我们使用Node.js方式来访问我们的MCP服务，为什么使用Node.js，因为 Node.js 能以最小成本、最贴近 MCP 规范的方式充当一个通用 MCP Client。
具体代码如下：

```javascript
import { spawn } from "child_process";
// Unicode 解码函数（必须有）
function decodeUnicode(str) {
    return JSON.parse(`"${str}"`);
}
// ⚠改成你自己的 MCP Server 项目路径
const MCP_PROJECT_PATH = "E:/Repos/aspnetcore-developer/aspnetcore-developer/src/09-AI-Agent/Agent-Framework/10-AgentAsMcpTools";

// 启动 dotnet MCP Server
const server = spawn(
    "dotnet",
    ["run", "--project", MCP_PROJECT_PATH],
    { stdio: ["pipe", "pipe", "pipe"] }
);

// ===== 监听 MCP STDOUT =====
server.stdout.on("data", (data) => {
    console.log("⬅ MCP STDOUT:");
    const text = data.toString().trim();
    let msg;

    try {
        msg = JSON.parse(text);
    } catch {
        console.log(text);
        return;
    }
    // 只对 tools/list 的 description 解码
    if (msg?.result?.tools) {
        msg.result.tools.forEach(tool => {
            if (typeof tool.description === "string") {
                tool.description = decodeUnicode(tool.description);
            }
        });
    }
    console.dir(msg, { depth: null });
});
// ===== STDERR =====
server.stderr.on("data", (data) => {
    console.error("⚠ MCP STDERR:");
    console.error(data.toString());
});

server.on("exit", (code) => {
    console.error(`❌ MCP Server exited with code ${code}`);
});

// ===== MCP 协议交互 =====
function send(msg) {
    server.stdin.write(JSON.stringify(msg) + "\n");
}
// tools/list
setTimeout(() => {
    console.log("➡ Sending tools/list");
    send({
        id: 1,
        jsonrpc: "2.0",
        method: "tools/list"
    });
}, 2000);

// 调用 Joker
setTimeout(() => {
    console.log("➡ Sending tools/call");
    send({
        id: 2,
        jsonrpc: "2.0",
        method: "tools/call",
        params: {
            name: "Joker",
            arguments: {
                query: "讲一个江湖笑话"
            }
        }
    });
}, 3000);
```

通过 JSON-RPC 与 MCP 工具交互，向 MCP Server 请求：列出当前可用的所有工具。

```json
{
  "id": 1,
  "jsonrpc": "2.0",
  "method": "tools/list"
}
```

字段说明：
- id: 1 —— 请求编号，用于匹配返回结果
- jsonrpc: "2.0" —— 使用 JSON-RPC 2.0 协议（MCP 必须）
- method: "tools/list" —— MCP 内置方法：查询服务器当前暴露的工具

发送该请求后，服务器将返回可用工具的列表。

```json
{
  "result": {
    "tools": [
      {
        "name": "Joker",
        "description": "我是一个擅长讲故事的江湖说书人。",
        "inputSchema": {
          "type": "object",
          "properties": {
            "query": {
              "description": "Input query to invoke the agent.",
              "type": "string"
            }
          },
          "required": ["query"]
        }
      }
    ]
  },
  "id": 1,
  "jsonrpc": "2.0"
}
```

通过 JSON-RPC 与 MCP 工具交互，向 MCP Server 请求：调用名为 Joker 的工具，并传入参数执行一次任务。

字段说明：
- id: 2 —— 请求编号，用于匹配返回结果
- jsonrpc: "2.0" —— 使用 JSON-RPC 2.0 协议（MCP 必须）
- method: "tools/call" —— MCP 内置方法：调用一个指定工具
- params.name: "Joker" —— 要调用的工具名称（来自 tools/list 返回结果）
- params.arguments —— 工具执行所需的输入参数
- query —— 具体传给 Joker Agent 的用户输入内容

```json
{
  "id": 2,
  "jsonrpc": "2.0",
  "method": "tools/call",
  "params": {
    "name": "Joker",
    "arguments": {
      "query": "讲一个江湖笑话"
    }
  }
}
```

返回结果如下：

```json
{
    "result": {
        "content": [
            {
                "type": "text",
                "text": "\"好嘞，各位大侠，小二今儿就给大伙说一个江湖笑话，听好了啊！\\n\\n话说，有一天，少林寺来了个奇葩游客，愣说自己是天下第一高手，上到山门就大喊：“谁敢和我过两招？不然就别怪我挑战少林威名了！”\\n\\n少林小和尚们站在旁边抖个不停，其实不是害怕，是憋笑，这都什么年头了，还大言不惭呢？老和尚坐着摇头叹气，心想：这人怕不是脑子空，但好歹是来找事的，也不能怠慢了江湖规矩。\\n\\n于是方丈亲自上前，双手合十，客气道：“施主可曾听闻少林武功？”\\n\\n游客掏出一根木棍，鼻孔朝天：“少林武功嘛，就是那点花拳绣腿！我今天用一根棍子，就看谁敢挡我！”\\n\\n方丈笑眯眯，提了提自己的袈裟说：“施主，那您要动手可得出家了，我劝您三思！”\\n\\n游客一愣：“为啥动手得出家？”\\n\\n老和尚手一招，只见十八铜人整齐列队，统一掏出剃头刀片，喊了一声：“剃好了他！”\\n\\n这一刻，那游客终于明白，少林棍法是一回事，剃头武功才是真绝学！直接吓得扔了棍子，光速跑下山，边跑边喊：“算我输、算我输！少林武功果然不凡！”\\n\\n客栈内围观的诸位客官笑得前仰后合。\\n\\n故事讲完，茶点该上了，您说还能再听一个不？\""
            }
        ]
    },
    "id": "2",
    "jsonrpc": "2.0"
}
```
运行效果如下：



MCP 工具调用的最小闭环
- `tools/list` —— 发现能力：列出当前可用的工具
- `tools/call` —— 执行能力：按名称调用指定工具
- `result.content` —— 返回模型输出：工具执行后的内容载体



## 7.云端持久化智能体 vs 本地临时智能体

我们在之前的文章中全部使用的AzureOpenAIClient.CreateAIAgent来创建Agent，
有些读者可能会好奇，这两种方式有什么区别？我们针对这个问题做一个对比说明。

这两种方式创建的“Agent”并非同一层级：
- PersistentAgentsClient：云端持久化的“一等 Agent 实体”，适合 MCP / 多工具 / 多会话 / 多客户端。
- AzureOpenAIClient.CreateAIAgent：本地临时的“会话级 Agent Wrapper”，本质是 Chat Completion 的语法糖。

| 对比维度         | PersistentAgentsClient  | AzureOpenAIClient.CreateAIAgent |
|------------------|-------------------------|----------------------------------|
| Agent 存在位置   | ☁️ 云端（Azure Foundry）  | 💻 本地进程内                      |
| 是否持久化       | ✅ 是（有 Agent ID）       | ❌ 否（进程结束即消失）             |
| 是否可复用       | ✅ 多次、多客户端           | ❌ 仅当前代码可用                   |
| 是否支持 MCP     | ✅ 官方设计目标             | ❌ 不适合                          |
| 是否支持工具编排 | ✅ 支持（Function / Tool） | ⚠️ 仅简单函数                      |
| 会话状态         | ✅ 云端管理                | ❌ 需自行管理                      |
| 生命周期         | 长生命周期                  | 短生命周期                          |
| 本质             | Agent 实体                  | ChatClient 封装                    |

## 总结

这节文章中，我们深入解析了如何将 Azure AI Foundry 上的持久化 Agent 转换为 MCP 工具，并通过 stdio 传输协议暴露给外部客户端使用。 这种方式不仅实现了 AI 组件的标准化调用，还为构建复杂的多 Agent 协作系统奠定了基础。通过理解 Persistent Agent 与本地 Agent Wrapper 之间的区别，开发者可以更好地选择适合自己应用场景的方案。