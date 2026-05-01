// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to configure ChatClientAgent to produce structured output.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using System.ComponentModel;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";


ChatClient chatClient = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
        .GetChatClient(deploymentName);


ChatClientAgent agent = chatClient.CreateAIAgent(new ChatClientAgentOptions(name: "HelpfulAssistant", instructions: "你是一个乐于助人的助手."));


AgentRunResponse<PersonInfo> response = await agent.RunAsync<PersonInfo>(
        "请提供关于桂兵兵的信息，他是一名 39 岁的软件工程师。"
);

Console.WriteLine("助理输出:");
Console.WriteLine($"姓名: {response.Result.Name}");
Console.WriteLine($"年龄: {response.Result.Age}");
Console.WriteLine($"职业: {response.Result.Occupation}");


ChatClientAgent agentWithPersonInfo = chatClient.CreateAIAgent(new ChatClientAgentOptions(name: "HelpfulAssistant", instructions: "你是一个乐于助人的助手。")
{
    ChatOptions = new()
    {
        ResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat.ForJsonSchema<PersonInfo>()
    }
});

var updates = agentWithPersonInfo.RunStreamingAsync("请提供关于桂兵兵的信息，他是一名 39 岁的软件工程师。");
PersonInfo personInfo = (await updates.ToAgentRunResponseAsync()).Deserialize<PersonInfo>(JsonSerializerOptions.Web);


Console.WriteLine("助理输出:");
Console.WriteLine($"姓名: {personInfo.Name}");
Console.WriteLine($"年龄: {personInfo.Age}");
Console.WriteLine($"职业: {personInfo.Occupation}");


[Description("个人信息，包括他们的姓名、年龄和职业。")]
public class PersonInfo
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("age")]
    public int? Age { get; set; }

    [JsonPropertyName("occupation")]
    public string? Occupation { get; set; }
}