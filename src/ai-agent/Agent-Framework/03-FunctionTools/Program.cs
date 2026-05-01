// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with a multi-turn conversation.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ComponentModel;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

[Description("获取指定国家的最新新闻标题。")]
static string GetNews([Description("国家名称。")] string country)
    => $"来自 {country} 的头条新闻：AI 正在革新软件开发领域。";

// 创建 agent 并集成工具方法
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一个乐于助人的助手", tools: [AIFunctionFactory.Create(GetNews)]);

// 非流式调用
Console.WriteLine(await agent.RunAsync("美国的最新新闻头条有哪些？"));


// 流式调用
//await foreach (var update in agent.RunStreamingAsync("美国的最新新闻头条有哪些？"))
//{
//    Console.WriteLine(update);
//}