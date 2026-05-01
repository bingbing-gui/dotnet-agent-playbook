// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates how to use Agent Skills with a ChatClientAgent.
// Agent Skills are modular packages of instructions and resources that extend an agent's capabilities.
// Skills follow the progressive disclosure pattern: advertise -> load -> read resources.
//
// This sample includes the expense-report skill:
//   - Policy-based expense filing with references and assets

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Responses;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- Configuration ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

// --- Skills Provider ---

#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
var skillsProvider = new FileAgentSkillsProvider(skillPath: Path.Combine(AppContext.BaseDirectory, "skills"));
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

#pragma warning disable OPENAI001
// --- Agent Setup ---
// Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetResponsesClient(deploymentName)
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "SkillsAgent",
        ChatOptions = new()
        {
            Instructions = "你是一名乐于助人的助手。",
        },
        AIContextProviders = [skillsProvider],
    });
#pragma warning restore OPENAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

// --- Example 1: Expense policy question (loads FAQ resource) ---
Console.WriteLine("例子 1: 查看费用报销政策常见问题");
Console.WriteLine("小费可以报销吗？我在一次打车时给了 25% 的小费，想知道这部分是否可以报销。");
Console.WriteLine("---------------------------------------");
AgentResponse response1 = await agent.RunAsync("小费可以报销吗？我在一次打车时给了 25% 的小费，想知道这部分是否可以报销。");
Console.WriteLine($"Agent: {response1.Text}\n");

// --- Example 2: Filing an expense report (multi-turn with template asset) ---
Console.WriteLine("例子 2: 填写费用报销单");
Console.WriteLine("我上周有 3 次客户晚餐和一张 1,200 美元的机票。请生成一份费用报销单，并询问是否有缺失的细节。");
Console.WriteLine("---------------------------------------");
AgentSession session = await agent.CreateSessionAsync();
AgentResponse response2 = await agent.RunAsync("我上周有 3 次客户晚餐和一张 1,200 美元的机票。请生成一份费用报销单，并询问是否有缺失的细节。",
    session);

Console.WriteLine($"Agent: {response2.Text}\n");