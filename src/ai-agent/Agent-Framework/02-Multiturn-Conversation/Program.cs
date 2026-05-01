// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with a multi-turn conversation.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");


AgentThread thread = agent.GetNewThread();
Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));
Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", thread));

// Invoke the agent with a multi-turn conversation and streaming, where the context is preserved in the thread object.
thread = agent.GetNewThread();

Console.WriteLine(await agent.RunAsync("再讲一个关于江湖侠客的小笑话，要幽默一点。", thread));
Console.WriteLine(await agent.RunAsync("给这个江湖侠客的小笑话加些表情符号，再添加点夸张的江湖腔。", thread));

//await foreach (var update in agent("再讲一个关于江湖侠客的小笑话，要幽默一点。", thread))
//{
//    Console.WriteLine(update);
//}
//await foreach (var update in agent.RunStreamingAsync("给这个江湖侠客的小笑话加些表情符号，再添加点夸张的江湖腔。", thread))
//{
//    Console.WriteLine(update);
//}