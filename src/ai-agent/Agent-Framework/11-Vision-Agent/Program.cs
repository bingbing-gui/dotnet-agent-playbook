// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to use Image Multi-Modality with an AI agent.

using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.AI;
using System.Text;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o";



// ============================================================
// 方式一：通过 AzureOpenAIClient / ChatClient 创建 Agent
// ============================================================
var openAIAgent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .AsAIAgent(
        name: "VisionAgent",
        instructions: "你是一个分析图片内容的智能代理，请根据图片内容回答用户的问题。");

// DataContent 表示二进制输入内容（如图片），具体类型名可能随 SDK 版本调整
ChatMessage message = new(ChatRole.User, [
    new TextContent("你在这张图片中看到了什么？"),
    await DataContent.LoadFromAsync("Assets/walkway.jpg"),
]);

var session = await openAIAgent.CreateSessionAsync();

Console.WriteLine(await openAIAgent.RunAsync(message, session));
//await foreach (var update in openAIAgent.RunStreamingAsync(message, session))
//{
//    Console.WriteLine(update);
//}


// ============================================================
// 方式二：通过 AIProjectClient 创建 Agent
// ============================================================
var foundryAgent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: deploymentName,
        instructions: "你是一个分析图片内容的智能代理，请根据图片内容回答用户的问题。",
        name: "VisionAgent");

ChatMessage message2 = new(ChatRole.User, [
    new TextContent("你在这张图片中看到了什么？"),
    await DataContent.LoadFromAsync("Assets/walkway.jpg"),
]);
var session2 = await foundryAgent.CreateSessionAsync();



Console.WriteLine(await foundryAgent.RunAsync(message2, session2));

//await foreach (var update in foundryAgent.RunStreamingAsync(message2, session2))
//{
//    Console.WriteLine(update);
//}