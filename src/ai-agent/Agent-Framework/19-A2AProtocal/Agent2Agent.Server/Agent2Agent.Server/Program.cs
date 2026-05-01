// See https://aka.ms/new-console-template for more information

/*
1. 添加 Microsoft.Agents.AI.Hosting.A2A.AspNetCore


 */

// See https://aka.ms/new-console-template for more information

using A2A;
using A2A.AspNetCore;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint =
    Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");

var deploymentName =
    Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? "gpt-4o-mini";

// ================================
// 1.创建本地 AI Agent
// ================================
AIAgent agent =
    new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
        .GetChatClient(deploymentName)
        .AsAIAgent(
            name: "Assistant",
            instructions: """
                你是一个通过 A2A 协议对外提供服务的 AI Agent。
                你需要清晰、简洁地回答问题。
                """
        );

// ================================
// 2️.创建 ASP.NET Core Host
// ================================
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// ================================
// 3️ 定义 Agent Card（A2A 对外说明书）
// ================================
AgentCard card = new AgentCard
{
    Name = "Assistant",
    Description = "一个通过 A2A 协议提供问答能力的 AI 助手",
    Version = "1.0.0",
    Url = "http://localhost:5000",

    DefaultInputModes = ["text"],
    DefaultOutputModes = ["text"],

    Capabilities = new AgentCapabilities
    {
        Streaming = false,
        PushNotifications = false
    },

    Skills =
    [
        new AgentSkill
        {
            Id = "general_chat",
            Name = "通用问答",
            Description = "回答用户提出的自然语言问题",
            Tags = ["chat", "qa", "general"],
            Examples =
            [
                "什么是 A2A 协议？",
                "请用一句话解释 Azure OpenAI",
                "帮我写一个 C# Hello World"
            ]
        }
    ]
};


// ================================
// 4️. 挂载 A2A 路由
// ================================
app.MapA2A(
    agent,
    path: "/",                        // Agent 执行入口
    agentCard: card,
    taskManager =>
    {
        // .well-known/agent-card.json
        app.MapWellKnownAgentCard(taskManager, "/");
    }
);

app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/"))
    {
        Console.WriteLine("[Server] 收到 A2A HTTP 请求");
    }

    await next();
});


// ================================
// 5️.启动
// ================================
await app.RunAsync();
