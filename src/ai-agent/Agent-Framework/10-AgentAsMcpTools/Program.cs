// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to expose an AI agent as an MCP tool.

using Azure.AI.Agents.Persistent;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using System.Text;


//var endpoint = Environment.GetEnvironmentVariable("AZURE_FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("AZURE_FOUNDRY_PROJECT_ENDPOINT is not set.");
//var deploymentName = Environment.GetEnvironmentVariable("AZURE_FOUNDRY_PROJECT_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var persistentAgentsClient = new PersistentAgentsClient("https://maf.services.ai.azure.com/api/projects/maf", new AzureCliCredential());

// 创建一个服务器端持久化代理。
var agentMetadata = await persistentAgentsClient.Administration.CreateAgentAsync(
    model: "gpt-4o",
    instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。",
    name: "Joker",
    description: "我是一个擅长讲故事的江湖说书人。");

// Retrieve the server side persistent agent as an AIAgent.
// 获取服务器端持久化代理
AIAgent agent = await persistentAgentsClient.GetAIAgentAsync(agentMetadata.Value.Id);

// 将代理转换为AIFunction，然后转换为MCP工具
// 代理名称和描述将用作mcp工具名称和描述
McpServerTool tool = McpServerTool.Create(agent.AsAIFunction());

try
{
    // 使用Stdio传输注册MCP服务器，并通过服务器公开工具
    HostApplicationBuilder builder = Host.CreateEmptyApplicationBuilder(settings: null);
    builder.Services
        .AddMcpServer()
        .WithStdioServerTransport()
        .WithTools([tool]);

    await builder.Build().RunAsync();
}
catch(Exception ex) when (ex.InnerException != null)
{
    Console.Error.WriteLine("initialize called",ex.Message);
}
