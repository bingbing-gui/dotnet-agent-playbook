// Copyright (c) Microsoft。保留所有权利。

// 压缩管道——渐进式上下文管理策略
//
// 此示例演示如何将带有压缩管道的 CompactionProvider
// 用作 AIContextProvider，以便在运行期间管理上下文。该管道
// 按照从温和到激进的顺序串联多种压缩策略：
//   1. ToolResultCompactionStrategy——将旧的工具调用组折叠为摘要
//   2. SummarizationCompactionStrategy——使用 LLM 压缩较早的对话片段
//   3. SlidingWindowCompactionStrategy——仅保留最近 N 个用户轮次
//   4. TruncationCompactionStrategy——紧急情况下的令牌预算保障措施

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Compaction;
using Microsoft.Extensions.AI;

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

#pragma warning disable OPENAI001
IChatClient agentChatClient = aiProjectClient
    .GetProjectOpenAIClient()
    .GetResponsesClient()
    .AsIChatClient(deploymentName);
IChatClient summarizerChatClient = aiProjectClient
    .GetProjectOpenAIClient()
    .GetResponsesClient()
    .AsIChatClient(deploymentName);
#pragma warning restore OPENAI001

// 定义一个可供代理使用的工具，以便观察工具结果压缩的实际效果。
[Description("根据产品名称查找当前价格。")]
static string LookupPrice([Description("要查找的产品名称。")] string productName) =>
    productName.ToUpperInvariant() switch
    {
        "笔记本" => "笔记本 $999.99.",
        "键盘" => "键盘 $79.99.",
        "鼠标" => "鼠标 $29.99.",
        _ => $"抱歉，我没有 '{productName}' 的定价信息。"
    };

#pragma warning disable MAAI001
// 为各个策略配置压缩管道
PipelineCompactionStrategy compactionPipeline =
    new(
        // 1. 温和：将旧的工具调用组折叠为简短摘要
        new ToolResultCompactionStrategy(CompactionTriggers.MessagesExceed(7)),
        // 2. 适中：使用 LLM 将较早的对话片段总结为简洁消息
        new SummarizationCompactionStrategy(summarizerChatClient, CompactionTriggers.TokensExceed(0x500)),
        // 3. 激进：仅保留最后 N 个用户轮次及其响应
        new SlidingWindowCompactionStrategy(CompactionTriggers.TurnsExceed(4)),
        // 4. 紧急：丢弃最早的消息组，直至低于令牌预算
        new TruncationCompactionStrategy(CompactionTriggers.TokensExceed(0x8000)));
// 使用采用压缩管道的 CompactionProvider 创建代理。
AIAgent agent =
    agentChatClient
        .AsBuilder()
        // 注意：在生成器级别添加 CompactionProvider，意味着它将应用于
        // 此生成器构建的所有代理，并同时管理代理消息和工具调用的上下文。
        .UseAIContextProviders(new CompactionProvider(compactionPipeline))
        .BuildAIAgent(
            new ChatClientAgentOptions
            {
                Name = "ShoppingAssistant",
                ChatOptions = new()
                {
                    Instructions =
                        """
                        你是一名乐于助人但较为健谈的购物助手。
                                               请帮助用户查询价格并比较产品。
                                               对于每个价格问题，你都必须使用 LookupPrice 工具，
                                               绝不能依靠记忆回答价格问题。
                                               回复时请务必提供充分详细的说明，并在不过分夸张的前提下
                                               尽可能完整地进行描述。
                        """,
                    Tools = [AIFunctionFactory.Create(LookupPrice)]
                },
                // 注意：也可以在此处指定 AIContextProviders，而不是使用
                // ChatClientBuilder.UseAIContextProviders。
                // 在代理级别指定压缩将跳过函数调用循环中的压缩。
                //AIContextProviders = [new CompactionProvider(compactionPipeline)]
            });

AgentSession session = await agent.CreateSessionAsync();

// 用于输出聊天历史记录大小的辅助方法
void PrintChatHistory()
{
    if (session.TryGetInMemoryChatHistory(out var history))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"\n[Messages: #{history.Count}]\n");
        Console.ResetColor();
    }
}

// 运行包含工具调用的多轮对话，以触发并演示压缩管道。
string[] prompts =
[
    "笔记本的价格是多少？",
    "那么键盘呢？",
    "鼠标呢？",
    "哪种产品最便宜？",
    "可以帮我比较一下笔记本和键盘吗？",
    "我最先询问的产品是什么？",
    "谢谢！",
];

foreach (string prompt in prompts)
{
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("\n[User] ");
    Console.ResetColor();
    Console.WriteLine(prompt);
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write("\n[Agent] ");
    Console.ResetColor();
    Console.WriteLine(await agent.RunAsync(prompt, session));

    PrintChatHistory();
}
