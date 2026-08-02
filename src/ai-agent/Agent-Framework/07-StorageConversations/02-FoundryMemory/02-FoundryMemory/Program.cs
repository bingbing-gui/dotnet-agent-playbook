// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to use the FoundryMemoryProvider to persist and recall memories for an agent.
// The sample stores conversation messages in a Microsoft Foundry memory store and retrieves relevant
// memories for subsequent invocations, even across new sessions.
//
// Note: Memory extraction in Microsoft Foundry is asynchronous and takes time. This sample demonstrates
// a simple polling approach to wait for memory updates to complete before querying.

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var foundryEndpoint = "https://maf.services.ai.azure.com/api/projects/maf";
var memoryStoreName = "memory-store-0001";
var deploymentName = "gpt-4o"; //Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";
var embeddingModelName = "text-embedding-3-large";// Environment.GetEnvironmentVariable("AZURE_AI_EMBEDDING_DEPLOYMENT_NAME") ?? "text-embedding-ada-002";

using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Debug)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});

DefaultAzureCredential credential = new();

AIProjectClient projectClient = new(new Uri(foundryEndpoint), credential);

#pragma warning disable MAAI001 
FoundryMemoryProvider memoryProvider = new(
    projectClient,
    memoryStoreName,
    stateInitializer: _ => new FoundryMemoryProvider.State(new FoundryMemoryProviderScope("sample-user-0001")),
    options: new FoundryMemoryProviderOptions
    {
        EnableSensitiveTelemetryData = true
    },
    loggerFactory: loggerFactory);

ChatClientAgent agent = projectClient.AsAIAgent(
    new ChatClientAgentOptions()
    {
        Name = "TravelAssistantWithFoundryMemory",
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "你是一个友好的旅行助手。在回答时使用已知的用户记忆，不要编造细节。"
        },
        AIContextProviders = [memoryProvider]
    });

AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("\n>> 设置 Foundry Memory 存储\n");

// 确保 Foundry Memory Store 已创建（如果需要，使用指定的模型创建它）。
await memoryProvider.EnsureMemoryStoreCreatedAsync(deploymentName, embeddingModelName, "简单Memory 存储针对旅游助手");

// 清除此范围内的任何现有记忆，以演示新的行为。
await memoryProvider.EnsureStoredMemoriesDeletedAsync(session);

Console.WriteLine(await agent.RunAsync("你好，我的名字是桂兵兵，我计划去北海道旅游。", session));
Console.WriteLine(await agent.RunAsync("我和朋友去旅游，帮我找风景优美的景点。", session));


Console.WriteLine("\n等待 Foundry Memory 处理更新...");

await memoryProvider.WhenUpdatesCompletedAsync();

Console.WriteLine("更新完成。\n");

Console.WriteLine(await agent.RunAsync("你已经知道我即将进行的旅行的哪些信息？", session));

Console.WriteLine("\n>> 序列化和反序列化会话以演示持久化状态\n");
JsonElement serializedSession = await agent.SerializeSessionAsync(session);
AgentSession restoredSession = await agent.DeserializeSessionAsync(serializedSession);
Console.WriteLine(await agent.RunAsync("你能回顾一下你记得的个人信息吗？", restoredSession));

Console.WriteLine("\n>> 开始一个共享相同 Foundry Memory 范围的新会话\n");

Console.WriteLine("\n等待 Foundry Memory 处理更新...");
await memoryProvider.WhenUpdatesCompletedAsync();

AgentSession newSession = await agent.CreateSessionAsync();
Console.WriteLine(await agent.RunAsync("总结一下你已经知道的关于我的信息。", newSession));