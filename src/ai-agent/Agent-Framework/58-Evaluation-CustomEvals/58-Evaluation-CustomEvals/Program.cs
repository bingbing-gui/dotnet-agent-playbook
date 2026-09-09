// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates writing custom evaluation functions for domain-specific
// checks. Custom evaluators run locally — no cloud evaluator service needed.
// For LLM-based quality scoring (relevance, coherence), see Evaluation_SimpleEval.

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4o-mini";


AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

AIAgent agent = projectClient.AsAIAgent(
    model: deploymentName,
    instructions: "你是一个客户支持代理。帮助用户解决他们的问题 "
                + "礼貌地提供清晰、可操作的步骤。",
    name: "SupportAgent");

EvalCheck noRefusal = FunctionEvaluator.Create("no_refusal", (string response) =>
    !response.Contains("我不能帮忙", StringComparison.OrdinalIgnoreCase)
    && !response.Contains("我无法帮忙", StringComparison.OrdinalIgnoreCase)
    && !response.Contains("超出我的能力范围", StringComparison.OrdinalIgnoreCase));

EvalCheck hasActionableSteps = FunctionEvaluator.Create("has_actionable_steps", (string response) =>
    response.Contains("1.", StringComparison.Ordinal)
    || response.Contains("- ", StringComparison.Ordinal)
    || response.Contains("• ", StringComparison.Ordinal));

EvalCheck reasonableLength = FunctionEvaluator.Create("reasonable_length", (string response) =>
    response.Length >= 50 && response.Length <= 2000);

LocalEvaluator evaluator = new LocalEvaluator(noRefusal, hasActionableSteps, reasonableLength);

string[] queries =
[
    "我的订单在两周后还没有到。 我该怎么办？",
    "我被同一件商品收取了两次费用。 你能帮忙吗？",
    "我如何退回损坏的产品？",
];

AgentEvaluationResults results = await agent.EvaluateAsync(queries, evaluator);

Console.WriteLine($"通过: {results.Passed}/{results.Total}");
Console.WriteLine();

for (int i = 0; i < results.Items.Count; i++)
{
    Console.WriteLine($"查询: {queries[i]}");
    Console.WriteLine($"响应: {(results.InputItems?[i].Response is { } resp ? resp.Substring(0, Math.Min(50, resp.Length)) : "N/A")}...");
    foreach (var metric in results.Items[i].Metrics)
    {
        string status = metric.Value.Interpretation?.Failed == true ? "失败" : "通过";
        Console.WriteLine($"  [{status}] {metric.Key}");
    }

    Console.WriteLine();
}