// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with Azure OpenAI Chat Completion as the backend.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = "GPT-54-PRO";// Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

#pragma warning disable OPENAI001 // Suppress experimental API warning

AIAgent responsePattrern = new AzureOpenAIClient(new Uri(endpoint),
    new DefaultAzureCredential())
     .GetResponsesClient(deploymentName)
     .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

#pragma warning restore OPENAI001
// Invoke the agent and output the text result.
Console.WriteLine(await responsePattrern.RunAsync("给我讲一个发生在茶馆里的段子。"));


Console.WriteLine("------------------------------------------------------------------------------------");


AIAgent chatCompletionPattern = new AzureOpenAIClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
     .GetChatClient(deploymentName)
     .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

// Invoke the agent and output the text result.
Console.WriteLine(await chatCompletionPattern.RunAsync("给我讲一个发生在茶馆里的段子。"));



