// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to use Image Multi-Modality with an AI agent.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = System.Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o";

var agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "VisionAgent",
        instructions: "你是一个分析图片内容的智能代理，请根据图片内容回答用户的问题。");


using HttpClient httpClient = new();
httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
byte[] imageBytes = await httpClient.GetByteArrayAsync("https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg");

// DataContent 表示二进制输入内容（如图片），具体类型名可能随 SDK 版本调整
ChatMessage message = new(ChatRole.User, [
    new TextContent("你在这张图片中看到了什么？"),
    new DataContent(imageBytes, "image/jpeg")
]);


var thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync(message,thread));

//await foreach (var update in agent.RunStreamingAsync(message, thread))
//{
//    Console.WriteLine(update);
//}