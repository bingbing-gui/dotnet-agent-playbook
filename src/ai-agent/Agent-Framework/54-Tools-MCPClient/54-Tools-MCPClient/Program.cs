// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates how to wrap MCP tools with a DelegatingAIFunction to add custom behavior (e.g., logging).
// Compare with Step09 which shows basic MCP tool usage without wrapping.
// The LoggingMcpTool pattern is useful for diagnostics, metering, or adding approval logic around tool calls.

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using SampleApp;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var agentInstructions = "你是一个乐于助人的助手，可以帮助解答 Microsoft 文档相关问题。请使用 Microsoft Learn MCP 工具搜索文档。";
var agentName = "DocsAgent-RAPI";


Console.WriteLine("正在连接 MCP 服务器：https://learn.microsoft.com/api/mcp ...");

await using McpClient mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new()
{
    Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
    Name = "Microsoft Learn MCP",
}));

IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();
Console.WriteLine($"可用的 MCP 工具：{string.Join(", ", mcpTools.Select(t => t.Name))}");

List<AITool> wrappedTools = mcpTools.Select(tool => (AITool)new LoggingMcpTool(tool)).ToList();

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("未设置 FOUNDRY_PROJECT_ENDPOINT。");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: agentInstructions,
    name: agentName,
    tools: wrappedTools);

Console.WriteLine($"智能体“{agent.Name}”已成功创建。");

// First query
var prompt1 = "如何使用 Azure CLI 创建 Azure 存储帐户？";
Console.WriteLine($"\n用户：{prompt1}\n");
AgentResponse response1 = await agent.RunAsync(prompt1);
Console.WriteLine($"智能体：{response1}");

Console.WriteLine("\n=======================================\n");

// Second query
var prompt2 = "什么是 Microsoft Agent Framework？";
Console.WriteLine($"用户：{prompt2}\n");
AgentResponse response2 = await agent.RunAsync(prompt2);
Console.WriteLine($"智能体：{response2}");

namespace SampleApp
{
    internal sealed class LoggingMcpTool(AIFunction innerFunction) : DelegatingAIFunction(innerFunction)
    {
        protected override ValueTask<object?> InvokeCoreAsync(AIFunctionArguments arguments, CancellationToken cancellationToken)
        {
            Console.WriteLine($"  >> [本地 MCP] 正在本地调用工具“{this.Name}”...");
            return base.InvokeCoreAsync(arguments, cancellationToken);
        }
    }
}