
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI.Evaluation;
using System.Text;
using FoundryEvals = Microsoft.Agents.AI.Foundry.FoundryEvals;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-4o-mini";

AIProjectClient projectClient = new(new Uri(endpoint), new DefaultAzureCredential());

AIAgent agent = projectClient.AsAIAgent(
    model: deploymentName,
    instructions: "你是一个乐于助人的助手。提供清晰、准确的答案。",
    name: "Joker");

// 配置 Foundry 质量评估器 — 通过 Foundry Evals API 在服务器端运行评估。
FoundryEvals evaluator = new(projectClient, deploymentName, FoundryEvals.Relevance, FoundryEvals.Coherence);

// 运行代理以测试查询并在一次调用中进行评估。
string[] queries = ["什么是光合作用?", "疫苗是如何工作的?"];
AgentEvaluationResults results = await agent.EvaluateAsync(queries, evaluator);

// 输出评估结果。
Console.WriteLine($"通过: {results.Passed}/{results.Total}");
if (results.ReportUrl is not null)
{
    Console.WriteLine($"报告: {results.ReportUrl}");
}

Console.WriteLine();

for (int i = 0; i < results.Items.Count; i++)
{
    Console.WriteLine($"查询: {queries[i]}");
    Console.WriteLine($"回答: {(results.InputItems?[i].Response is { } resp ? resp.Substring(0, Math.Min(50, resp.Length)) : "N/A")}...");
    foreach (var metric in results.Items[i].Metrics)
    {
        string score = metric.Value is NumericMetric nm && nm.Value.HasValue
            ? nm.Value.Value.ToString("F1")
            : "N/A";
        Console.WriteLine($"  {metric.Key}: {score}");
    }

    Console.WriteLine();
}