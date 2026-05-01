// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with Azure Foundry Agents as the backend.

using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = "https://maf.services.ai.azure.com/api/projects/maf";
var deploymentName = "gpt-4o";

const string persistentAgent = "persistent-agent";
const string instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。";
// 获取一个客户端，用于创建/获取服务端代理。
// 警告：DefaultAzureCredential 在开发环境中很方便，但在生产环境中需要谨慎使用。
// 在生产环境中，建议使用更明确的凭据类型（例如 ManagedIdentityCredential），以避免
// 延迟问题、非预期的凭据探测，以及由回退机制带来的潜在安全风险。
var persistentAgentsClient = new PersistentAgentsClient(endpoint, new DefaultAzureCredential());

// 你可以使用 Azure.AI.Agents.Persistent SDK 创建一个服务端持久化 Agent。

var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: deploymentName,
    name: persistentAgent,
    instructions: instructions);

// 你可以将一个已经创建好的服务端持久化 Agent 检索出来，并作为 AIAgent 使用。
AIAgent agent1 = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);

// 你也可以在创建服务端持久化 Agent 的同时，直接将其作为 AIAgent 返回。
AIAgent agent2 = await persistentAgentsClient.CreateAIAgentAsync(
    model: deploymentName,
    name: persistentAgent,
    instructions: instructions);

// 然后你就可以像调用其他 AIAgent 一样调用这个 Agent。
AgentSession session = await agent1.CreateSessionAsync();
Console.WriteLine(await agent1.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", session));


Console.WriteLine($"agent1 id: {agent1.Id}");
Console.WriteLine($"agent2 id: {agent2.Id}");


if (session is ChatClientAgentSession typedSession)
{
    Console.WriteLine($"conversationId: {typedSession.ConversationId}");
}


// 执行清理操作。
await persistentAgentsClient.Administration.DeleteAgentAsync(agent1.Id);
await persistentAgentsClient.Administration.DeleteAgentAsync(agent2.Id);