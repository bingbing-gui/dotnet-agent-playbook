// Copyright (c) Microsoft. All rights reserved.

using System.Text;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
if (string.IsNullOrWhiteSpace(endpoint))
{
    throw new InvalidOperationException("请设置 FOUNDRY_PROJECT_ENDPOINT 为 Foundry 项目终结点。");
}

var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL");
if (string.IsNullOrWhiteSpace(deploymentName))
{
    throw new InvalidOperationException("请设置 FOUNDRY_MODEL 为 Foundry 模型部署名称。");
}
var project = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
#pragma warning disable OPENAI001
IChatClient chatClient = project.GetProjectOpenAIClient()
    .GetResponsesClient()
    .AsIChatClient(deploymentName);
#pragma warning restore OPENAI001

#pragma warning disable MAAI001, OPENAI001
var strategy = new SummarizationCompactionStrategy(
    new SummaryReportingChatClient(chatClient),
    CompactionTriggers.MessagesExceed(6),
    minimumPreservedGroups: 2,
    summarizationPrompt: "请用中文简洁总结对话，保留用户的需求和偏好。",
    target: index => index.IncludedMessageCount <= 4);

AIAgent agent = chatClient.AsBuilder()
    .UseAIContextProviders(new CompactionProvider(strategy))
    .BuildAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new ChatOptions
        {
            Instructions = "你是购物助手，请用中文简洁回答。",
            RawRepresentationFactory = _ => new CreateResponseOptions { StoredOutputEnabled = false },
        },
    });
#pragma warning restore MAAI001, OPENAI001

AgentSession session = await agent.CreateSessionAsync();
string[] prompts =
[
    "我想买一台笔记本电脑，预算是 5000 元。",
    "我经常出差，希望电脑轻便一些。",
    "我主要用它写代码，不玩游戏。",
    "我希望电池续航时间长一些。",
    "请根据我之前说的需求，帮我总结一下选购建议。",
];

foreach (string prompt in prompts)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine($"\n用户：{prompt}");
    Console.ResetColor();

    var response = await agent.RunAsync(prompt, session);

    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine($"助手：{response}");
    Console.ResetColor();
}

sealed class SummaryReportingChatClient(IChatClient innerClient) : DelegatingChatClient(innerClient)
{
    public override async Task<ChatResponse> GetResponseAsync(
        IEnumerable<ChatMessage> messages,
        ChatOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine("\n[压缩] 正在总结较早的对话...");
        try
        {
            ChatResponse response = await base.GetResponseAsync(messages, options, cancellationToken);
            Console.WriteLine($"[压缩摘要] {response.Text}\n");
            return response;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine($"[压缩失败] {ex.Message}");
            throw;
        }
    }
}
