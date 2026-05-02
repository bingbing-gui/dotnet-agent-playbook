// Copyright (c) Microsoft. All rights reserved.

using GitHub.Copilot.SDK;
using GitHub.Copilot.SDK.Rpc;
using Microsoft.Agents.AI;
using System.Text;

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

//await using CopilotClient copilotClient = new();
//await copilotClient.StartAsync();

//// 创建会话配置并指定模型
//SessionConfig sessionConfig = new()
//{
//    Model = "claude-opus-4.5",
//    Streaming = false,
//    OnPermissionRequest = PermissionHandler.ApproveAll
//    // or a custom handler
//};

//// 使用扩展方法创建带自定义配置的 Agent
//AIAgent agent = copilotClient.AsAIAgent(
//    sessionConfig,
//    ownsClient: true,
//    id: "my-copilot-agent",
//    name: "My Copilot Assistant",
//    description: "一个由 GitHub Copilot 驱动的智能 AI 助手"
//);

//// 使用 Agent —— 让它帮我们写代码
//AgentResponse response = await agent.RunAsync("编写一个 .NET 10 的 C# 单文件 Hello World 程序");
//Console.WriteLine(response);

