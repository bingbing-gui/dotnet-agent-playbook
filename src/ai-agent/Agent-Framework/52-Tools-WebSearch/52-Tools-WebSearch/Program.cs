// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to use the Web Search Tool with a ChatClientAgent.

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Text;

const string AgentInstructions = "你是一个乐于助人的助手，可以搜索网络以查找最新信息并准确回答问题。";
const string AgentName = "WebSearchAgent-RAPI";

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());


AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: AgentInstructions,
    name: AgentName,
    tools: [new HostedWebSearchTool()]);

AgentResponse response = await agent.RunAsync("今天东京的天气怎么样? ");

Console.WriteLine($"Response: {response.Text}");

foreach (AIAnnotation annotation in response.Messages.SelectMany(m => m.Contents).SelectMany(c => c.Annotations ?? []))
{
    Console.WriteLine($"Annotation: {annotation}");
#pragma warning disable OPENAI001 
    if (annotation.RawRepresentation is UriCitationMessageAnnotation urlCitation)
    {
        Console.WriteLine($$"""
            Title: {{urlCitation.Title}}
            URL: {{urlCitation.Uri}}
            """);
    }
#pragma warning restore OPENAI001 
}