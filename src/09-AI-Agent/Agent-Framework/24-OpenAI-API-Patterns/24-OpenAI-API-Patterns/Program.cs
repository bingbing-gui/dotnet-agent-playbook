// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with Azure OpenAI Chat Completion as the backend.

using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = "https://maf.services.ai.azure.com/api/projects/maf";// Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = "GPT-54-PRO";//Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";


var options = new AzureOpenAIClientOptions();
options.EnableDistributedTracing = true;


AIAgent chatCompletionAegntPattern = new AzureOpenAIClient(
    new Uri("https://maf.cognitiveservices.azure.com/"),
    new DefaultAzureCredential(),options)
     .GetChatClient(deploymentName)
     .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");



Console.WriteLine(await chatCompletionAegntPattern.RunAsync("给我讲一个发生在茶馆里的段子。"));



AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

// Define the agent you want to create. (Prompt Agent in this case)
AgentVersionCreationOptions agentVersionCreationOptions = new AgentVersionCreationOptions(new PromptAgentDefinition(model: deploymentName) { Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。" });

// Azure.AI.Agents SDK creates and manages agent by name and versions.
// You can create a server side agent version with the Azure.AI.Agents SDK client below.

var createdAgentVersion = aiProjectClient.Agents.CreateAgentVersion(agentName: "Joker", options: agentVersionCreationOptions);


AIAgent jokerAgent = aiProjectClient.AsAIAgent(createdAgentVersion);


Console.WriteLine(await jokerAgent.RunAsync("给我讲一个发生在茶馆里的段子。"));

