// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with tools from an MCP Server.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI.Chat;
using System.Text;



Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";


await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
{
    Name = "MCPServer",
    Command = "npx",
    Arguments = ["-y", "--verbose", "@modelcontextprotocol/server-github"],
}));


var mcpTools = await mcpClient.ListToolsAsync().ConfigureAwait(false);

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
     .GetChatClient(deploymentName)
     .AsAIAgent(instructions: "你是一个只回答与GitHub仓库相关问题的AI助手。", tools: [.. mcpTools.Cast<AITool>()]);

Console.WriteLine(await agent.RunAsync("总结一下 microsoft/semantic-kernel 仓库的最近四次提交？"));