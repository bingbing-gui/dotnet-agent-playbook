// 版权所有 (c) Microsoft。保留所有权利。

// 此示例演示如何将 HyperlightCodeActProvider 用作沙盒化的 Python
// 代码解释器：模型可以编写并执行任意 Python 代码来回答定量问题，
// 而无需调用任何其他工具。

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hyperlight;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("未设置 FOUNDRY_PROJECT_ENDPOINT。");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

var guestPath = Path.Combine(AppContext.BaseDirectory, "GUEST_PATH", "python-sandbox.aot");

using var codeAct = new HyperlightCodeActProvider(HyperlightCodeActProviderOptions.CreateForWasm(guestPath));
//using var codeAct = new HyperlightCodeActProvider(HyperlightCodeActProviderOptions.CreateForJavaScript());

// 警告：DefaultAzureCredential 便于开发，但在生产环境中使用时需要慎重考虑。
// 在生产环境中，请考虑使用特定凭据（例如 ManagedIdentityCredential），以避免
// 回退机制导致的延迟问题、意外的凭据探测以及潜在的安全风险。
AIAgent agent = new AIProjectClient(
    new Uri(endpoint),
    new DefaultAzureCredential())
    .AsAIAgent(new ChatClientAgentOptions()
    {
        ChatOptions = new() { ModelId = deploymentName, Instructions = "你是一个乐于助人的助手。当用户询问定量问题时，请编写 Python 代码并调用 `execute_code`，而不是进行猜测。" },
        AIContextProviders = [codeAct],
    });
var result = await agent.RunAsync("第 20 个斐波那契数是多少？");
Console.WriteLine(result);
Console.WriteLine(await agent.RunAsync("计算 [1, 4, 9, 16, 25, 36] 的平均值和标准差。"));



