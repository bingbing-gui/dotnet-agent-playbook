// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create an agent from a YAML based declarative representation.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// Create the chat client
IChatClient chatClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetChatClient(deploymentName)
     .AsIChatClient();

var text =
    """
    kind: Prompt
    name: Assistant
    description: 乐于帮忙的小助手
    instructions: 你是一个乐于帮忙的小助手。你会使用用户指定的语言来回答问题。你需要以 JSON 格式返回你的回答。
    model:
        options:
            temperature: 0.9
            topP: 0.95
    outputSchema:
        properties:
            language:
                type: string
                required: true
                description: 回答所使用的语言。
            answer:
                type: string
                required: true
                description: 回答的文本内容。
    """;

var agentFactory = new ChatClientPromptAgentFactory(chatClient);
var agent = await agentFactory.CreateFromYamlAsync(text);

Console.WriteLine(await agent!.RunAsync("用英语讲一个发生在茶馆里面的故事。"));

await foreach (var update in agent!.RunStreamingAsync("用日语讲一个发生在茶馆里面的故事"))
{
    Console.WriteLine(update);
}

Console.ReadLine();