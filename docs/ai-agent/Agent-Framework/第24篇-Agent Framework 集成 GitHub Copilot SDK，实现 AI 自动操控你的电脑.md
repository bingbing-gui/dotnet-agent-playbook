# 先决条件

⚠️ **警告：容器环境建议**

GitHub Copilot 可以执行工具和命令，这些操作可能会与你的系统产生交互。为了安全起见，强烈建议在容器化环境中运行此示例（例如 Docker、Dev Container），以避免对你的本地机器造成意外影响。

在开始之前，请确保你具备以下先决条件：

- 已安装 .NET 10 SDK 或更高版本
- 已安装 GitHub Copilot CLI，并且可以在你的 PATH 中访问（或者你也可以提供自定义路径）

## 配置 GitHub Copilot CLI

要使用此示例，你需要安装 GitHub Copilot CLI。你可以按照以下地址的说明进行安装：

👉 https://github.com/github/copilot-sdk

安装完成后，请确保 copilot 命令已经添加到你的 PATH 中，或者通过 CopilotClientOptions 配置一个自定义路径。

## 运行示例

如果使用默认配置，则无需额外的环境变量。该示例将会：

- 使用默认配置创建一个 GitHub Copilot 客户端
- 使用 Copilot SDK 创建一个 AI Agent
- 向 Agent 发送一条消息
- 显示返回结果

```csharp
Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

static Task<PermissionRequestResult> PromptPermission(PermissionRequest request, PermissionInvocation invocation)
{
    Console.WriteLine($"\n[请求权限: {request.Kind}]");
    Console.Write("同意? (y/n): ");

    string? input = Console.ReadLine()?.Trim().ToUpperInvariant();

    PermissionRequestResultKind kind = input is "Y" or "YES" ? PermissionRequestResultKind.Approved : PermissionRequestResultKind.DeniedInteractivelyByUser;

    return Task.FromResult(new PermissionRequestResult { Kind = kind });
}

await using CopilotClient copilotClient = new();
await copilotClient.StartAsync();

SessionConfig sessionConfig = new()
{
    OnPermissionRequest = PromptPermission,
};

AIAgent agent = copilotClient.AsAIAgent(sessionConfig, ownsClient: true);

bool useStreaming = true;

string prompt = "帮我打开Chrome浏览器，查询微软的股票";
Console.WriteLine($"User: {prompt}\n");

if (useStreaming)
{
    await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(prompt))
    {
        Console.Write(update);
    }

    Console.WriteLine();
}
else
{
    AgentResponse response = await agent.RunAsync(prompt);
    Console.WriteLine(response);
}
```

运行命令：



## 高级用法

你可以通过提供额外配置来自定义 Agent：

```csharp
await using CopilotClient copilotClient = new();
await copilotClient.StartAsync();

// 创建会话配置并指定模型
SessionConfig sessionConfig = new()
{
    Model = "claude-opus-4.5",
    Streaming = false,
    OnPermissionRequest = PermissionHandler.ApproveAll
    // or a custom handler
};

// 使用扩展方法创建带自定义配置的 Agent
AIAgent agent = copilotClient.AsAIAgent(
    sessionConfig,
    ownsClient: true,
    id: "my-copilot-agent",
    name: "My Copilot Assistant",
    description: "一个由 GitHub Copilot 驱动的智能 AI 助手"
);

// 使用 Agent —— 让它帮我们写代码
AgentResponse response = await agent.RunAsync("编写一个 .NET 10 的 C# 单文件 Hello World 程序");
Console.WriteLine(response);
```

## 流式响应（Streaming Responses）

如果你希望获取流式输出（边生成边返回）：

```csharp
await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("编写一个计算斐波那契数列的 C# 函数"))
{
    Console.Write(update.Text);
}
```

