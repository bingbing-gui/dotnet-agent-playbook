#pragma warning disable MEAI001
// Copyright (c) Microsoft. All rights reserved.
// This sample demonstrates how to use a ChatClientAgent with function tools that require a human in the loop for approvals.
// It shows both non-streaming and streaming agent interactions using menu-related tools.
// If the agent is hosted in a service, with a remote user, combine this sample with the Persisted Conversations sample to persist the chat history
// while the agent is waiting for user input.

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

// Create a sample function tool that the agent can use.

[Description("获取指定国家的最新新闻标题。")]
static string GetNews([Description("国家名称。")] string country)
    => $"来自 {country} 的头条新闻：AI 正在革新软件开发领域。";

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .CreateAIAgent(
        instructions: "你是一个乐于助人的助手",
        tools: [new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GetNews))]
    );

AgentThread thread = agent.GetNewThread();
var response = await agent.RunAsync("美国的最新新闻是什么？", thread);
var userInputRequests = response.UserInputRequests.ToList();

while (userInputRequests.Count > 0)
{
    var userInputResponses = userInputRequests
        .OfType<FunctionApprovalRequestContent>()
        .Select(functionApprovalRequest =>
        {
            Console.WriteLine($"代理想调用以下函数，请回复 Y 以批准：Name {functionApprovalRequest.FunctionCall.Name}");
            return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
        })
        .ToList();

    response = await agent.RunAsync(userInputResponses, thread);
    userInputRequests = response.UserInputRequests.ToList();
}

Console.WriteLine($"\nAgent: {response}");


Console.ReadLine();

#pragma warning restore MEAI001