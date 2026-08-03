// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates using Valkey for persistent chat history with the Agent Framework.
// ValkeyChatHistoryProvider persists conversation history across sessions using Valkey lists.
//
// Prerequisites:
//   - A running Valkey server (any version):
//       docker run -d --name valkey -p 6379:6379 valkey/valkey:latest
//   - Azure OpenAI endpoint and deployment configured via environment variables

using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Valkey;
using Microsoft.Extensions.AI;
using System.Text;
using Valkey.Glide;

//var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
//var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = "https://maf.services.ai.azure.com/api/projects/maf";
var deploymentName = "gpt-5.2";

var valkeyConnection = Environment.GetEnvironmentVariable("VALKEY_CONNECTION") ?? "localhost:6379";
var connection = await ConnectionMultiplexer.ConnectAsync(valkeyConnection);

Console.WriteLine("=== ValkeyChatHistoryProvider — 持久化会话历史 ===\n");

var historyProvider = new ValkeyChatHistoryProvider(
    connection,
    _ => new ValkeyChatHistoryProvider.State($"sample-{Guid.NewGuid():N}"),
    new ValkeyChatHistoryProviderOptions
    {
        KeyPrefix = "sample_chat",
        MaxMessages = 20
    });

#pragma warning disable MAAI001,MEAI001
AIAgent historyAgent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetProjectOpenAIClient()
    .GetProjectResponsesClient()
    .AsIChatClientWithStoredOutputDisabled(deploymentName)
    .AsAIAgent(new ChatClientAgentOptions()
    {
        ChatOptions = new() { ModelId = deploymentName, Instructions = "你是一个乐于助人的助手，会记住我们的对话。" },
        ChatHistoryProvider = historyProvider
    });
#pragma warning restore MAAI001,MEAI001

AgentSession session1 = await historyAgent.CreateSessionAsync();
Console.WriteLine(await historyAgent.RunAsync("你好，我叫桂兵兵，我是一名软件工程师。", session1));
Console.WriteLine(await historyAgent.RunAsync("我正在使用Valkey进行缓存的项目。", session1));
Console.WriteLine(await historyAgent.RunAsync("你记得我什么吗？", session1));

var messageCount = await historyProvider.GetMessageCountAsync(session1);
Console.WriteLine($"\n  已在Valkey中存储 {messageCount} 条消息。\n");

// Clean up
connection.Dispose();

Console.WriteLine("完成！");
