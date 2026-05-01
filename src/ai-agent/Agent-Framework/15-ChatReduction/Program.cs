// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to use a chat history reducer to keep the context within model size limits.
// Any implementation of Microsoft.Extensions.AI.IChatReducer can be used to customize how the chat history is reduced.
// NOTE: this feature is only supported where the chat history is stored locally, such as with OpenAI Chat Completion.
// Where the chat history is stored server side, such as with Azure Foundry Agents, the service must manage the chat history size.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);



var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";


#pragma warning disable MEAI001
AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new() { Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。" },
        Name = "Joker",
        ChatMessageStoreFactory = ctx => new InMemoryChatMessageStore(new MessageCountingChatReducer(2), ctx.SerializedState, ctx.JsonSerializerOptions)
    });
#pragma warning restore MEAI001

AgentThread thread = agent.GetNewThread();


Console.WriteLine(await agent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", thread));

IList<ChatMessage>? chatHistory = thread.GetService<IList<ChatMessage>>();
Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");

// Invoke the agent a few more times.
Console.WriteLine(await agent.RunAsync("现在把这个段子加上一些表情符号，并用说书人的语气再讲一遍。", thread));
Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");
Console.WriteLine(await agent.RunAsync("保持刚才的语气，讲一个关于健忘冒险者的轻松小故事，像是在讲笑话一样。", thread));
Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");

// At this point, the chat history has exceeded the limit and the original message will not exist anymore,
// so asking a follow up question about it will not work as expected.
Console.WriteLine(await agent.RunAsync("接着刚才的氛围，讲一个发生在日常生活里的小乌龙事件，轻松随意一点。", thread));

Console.WriteLine($"\n 聊天有 {chatHistory?.Count} 消息.\n");