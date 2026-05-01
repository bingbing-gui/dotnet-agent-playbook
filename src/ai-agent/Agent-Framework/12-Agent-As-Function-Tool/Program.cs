// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a Azure OpenAI AI agent as a function tool.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ComponentModel;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

[Description("获取指定地点的天气信息。")]
static string GetWeather([Description("要获取天气的地点。")] string location)
    => $"{location} 多云，最高气温 15°C。";


AIAgent weatherAgent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        name: "WeatherAgent",
        instructions: "你专注于回答天气相关的问题，必要时调用工具获取信息后再回答。",
        description: "一个提供天气信息的智能体。",
        tools: [AIFunctionFactory.Create(GetWeather)]
    );

AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: """
你是一个助手，必须只用日语回答。
工具返回的内容可能是中文，请将其翻译成自然的日语后再输出。
不要输出中文或英文。
""",
        tools: [weatherAgent.AsAIFunction()]
    );

Console.WriteLine(await agent.RunAsync("东京的天气如何？"));
