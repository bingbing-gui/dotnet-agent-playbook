// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a AI agents with Azure Foundry Agents as the backend.

using Azure.AI.Projects;
using Azure.AI.Projects.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = "https://maf.services.ai.azure.com/api/projects/maf"; //Environment.GetEnvironmentVariable("AZURE_AI_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_AI_PROJECT_ENDPOINT is not set.");
var deploymentName = "gpt-4o";//Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

const string JokerName = "JokerAgent";

//获取一个客户端，用于通过 Azure Foundry Agents 创建 / 获取 / 删除服务器端的 Agent。
// 警告：DefaultAzureCredential 在开发阶段非常方便，但在生产环境中需要谨慎使用。
// 在生产环境中，建议使用明确指定的凭据（例如 ManagedIdentityCredential），以避免
// 由于凭据探测机制带来的延迟问题、非预期的凭据尝试，以及潜在的安全风险。
var aiProjectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

// 定义要创建的 Agent（本例中是 Prompt Agent）。
var agentVersionCreationOptions = new AgentVersionCreationOptions(new PromptAgentDefinition(model: deploymentName) { Instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。" });

// Azure.AI.Agents SDK 通过名称和版本来创建和管理 Agent。
// 你可以使用下面的 Azure.AI.Agents SDK 客户端在服务器端创建一个 Agent 版本。
var createdAgentVersion = aiProjectClient.Agents.CreateAgentVersion(agentName: JokerName, options: agentVersionCreationOptions);

// 注意：
//      agentVersion.Id = "<agentName>:<versionNumber>",
//      agentVersion.Version = <versionNumber>,
//      agentVersion.Name = <agentName>
// 你可以使用一个 AIAgent 来调用已经在服务器端创建好的 Agent 版本。
AIAgent existingJokerAgent = aiProjectClient.AsAIAgent(createdAgentVersion);

// 你也可以通过提供相同的 Agent 名称，但使用不同的定义，来创建另一个 AIAgent 版本。
AIAgent newJokerAgent = await aiProjectClient.CreateAIAgentAsync(name: JokerName, model: deploymentName, instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。");

// 你也可以只提供 Agent 的名称来获取该 AIAgent 的最新版本。
AIAgent jokerAgentLatest = await aiProjectClient.GetAIAgentAsync(name: JokerName);
var latestAgentVersion = jokerAgentLatest.GetService<AgentVersion>()!;

// 可以通过调用 GetService 方法来获取对应版本的 AIAgent。
Console.WriteLine($"最新Agent版本号Id: {latestAgentVersion.Id}");

// 一旦你获取到了 AIAgent，就可以像调用其他 AIAgent 一样调用它。
AgentSession session = await jokerAgentLatest.CreateSessionAsync();
Console.WriteLine(await jokerAgentLatest.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。", session));

// 这将使用同一个会话（session）来继续对话。
Console.WriteLine(await jokerAgentLatest.RunAsync("现在把这个段子重新输出一次输出，加上一些表情符号。", session));

// 按 Agent 名称进行清理时，会删除该名称下创建的所有 Agent 版本。
//aiProjectClient.Agents.DeleteAgent(existingJokerAgent.Name);