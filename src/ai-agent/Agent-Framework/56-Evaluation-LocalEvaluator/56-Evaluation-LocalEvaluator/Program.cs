// Copyright (c) Microsoft. All rights reserved.

// This sample demonstrates evaluating agent responses against expected outputs.

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4o-mini";

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(
        model: deploymentName,
        instructions: "你是一个数学导师。简洁回答，只给出数字结果。",
        name: "MathTutor");
// 内置的检查器可以组合使用，以便在评估时应用多个条件。
LocalEvaluator localEvaluator = new LocalEvaluator(EvalChecks.ContainsExpected(), EvalChecks.NonEmpty());   // 响应必须包含预期答案
                                                                                                            // 响应不能为空
// 查询和预期输出。
string[] queries = ["2+2 是多少?", "144 的平方根是多少?"];
string[] expectedOutputs = ["4", "12"];

// 运行代理并使用预期输出进行评估。
AgentEvaluationResults results = await agent.EvaluateAsync(
    queries,
    localEvaluator,
    expectedOutput: expectedOutputs);
// 打印结果。
Console.WriteLine($"评估: {results.ProviderName}");
Console.WriteLine($"  通过: {results.Passed}/{results.Total}");
Console.WriteLine($"  全部通过: {results.AllPassed}");
Console.WriteLine();

for (int i = 0; i < results.Items.Count; i++)
{
    Console.WriteLine($"查询: {queries[i]}  |  预期: {expectedOutputs[i]}");
    Console.WriteLine($"响应: {(results.InputItems?[i].Response is { } resp ? resp.Substring(0, Math.Min(50, resp.Length)) : "N/A")}");
    foreach (var metric in results.Items[i].Metrics)
    {
        string status = metric.Value.Interpretation?.Failed == true ? "FAIL" : "PASS";
        Console.WriteLine($"  [{status}] {metric.Key}: {metric.Value.Interpretation?.Reason}");
    }

    Console.WriteLine();
}