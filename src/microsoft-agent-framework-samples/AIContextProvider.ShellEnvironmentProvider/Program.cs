// 版权所有 (c) Microsoft。保留所有权利。

// 使用可感知环境的系统提示词的 Shell 工具
//
// 警告：此示例使用 LocalShellExecutor，它会在本机 Shell 中执行真实命令。
// 此处禁用了审批门控，以便演示能够在无人干预的情况下运行；在任何实际应用中，
// 请保持启用审批（默认设置），或使用 DockerShellExecutor 实现容器隔离。
// 下面由模型生成的命令是只读命令或作用域受限的命令（echo、进入临时文件夹、
// 设置进程局部环境变量），但其他模型或提示词可能会选择执行破坏性操作。
// 请仅在你允许代理向终端输入命令的环境中运行此示例。
//
// 演示如何在两种模式下使用 LocalShellExecutor，并搭配
// ShellEnvironmentProvider。ShellEnvironmentProvider 是一个 AIContextProvider，
// 它会探测实时 Shell 环境（操作系统、系统系列、版本、当前工作目录和常用 CLI），
// 并注入权威的系统提示词指令，使代理能够使用正确的命令风格
//（PowerShell 或 POSIX）。
//
// 两次运行：
//   1) 无状态模式：每次工具调用都会启动一个全新的 Shell。当命令彼此独立
//      （只读脚本、版本检查、文件列表），并且希望各次调用之间具有强隔离时，
//      此模式非常有用。一次调用产生的副作用（更改目录、导出变量）不会延续到
//      下一次调用。
//   2) 持久模式：所有调用复用同一个长期运行的 Shell，因此会保留工作目录和
//      导出的环境变量。此模式适合需要逐步构建状态的多步骤工作流
//      （进入某个文件夹并在其中执行一系列命令；在一个步骤中设置令牌，
//      然后在下一步读取它）。

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Tools.Shell;
using Microsoft.Extensions.AI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("未设置 FOUNDRY_PROJECT_ENDPOINT。");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

// 警告：DefaultAzureCredential 便于开发，但在生产环境中需要谨慎评估。
// 在生产环境中，请考虑使用特定凭据（例如 ManagedIdentityCredential），
// 以避免回退机制导致延迟、意外的凭据探测和潜在安全风险。
var aiProjectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

const string Instructions = """
    你是一个仅拥有 run_shell 工具的代理。请使用该工具满足用户的请求。
    不要描述你将要执行的操作，而是实际运行命令。
    请根据真实的命令输出回复最终答案。
    """;

// --------------------------------------------------------------------
// 1. 无状态模式——每次调用都会使用一个全新的 Shell。
// --------------------------------------------------------------------
Console.WriteLine("### 无状态模式\n");
await using (var statelessShell = new LocalShellExecutor(new() { Mode = ShellMode.Stateless, AcknowledgeUnsafe = true }))
{
    var envProvider = new ShellEnvironmentProvider(statelessShell);
    var statelessAgent = aiProjectClient.AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = Instructions,
            Tools = [statelessShell.AsAIFunction(requireApproval: false)],
        },
        AIContextProviders = [envProvider],
    });

    var statelessSession = await statelessAgent.CreateSessionAsync();
    Console.WriteLine(await statelessAgent.RunAsync("输出当前工作目录。", statelessSession));
    Console.WriteLine();

    // 展示无状态调用之间不会保留副作用：先让代理在一次调用中进入系统临时目录，
    // 再通过第二次调用查询当前工作目录。由于使用无状态模式，目录变更不会保留。
    Console.WriteLine(await statelessAgent.RunAsync("进入系统临时文件夹，然后输出当前工作目录。", statelessSession));
    Console.WriteLine();
    Console.WriteLine(await statelessAgent.RunAsync("在一次新的 Shell 调用中，再次输出当前工作目录。告诉我它是否与上一次调用中的临时文件夹一致。", statelessSession));
    Console.WriteLine();

    PrintSnapshot(envProvider.CurrentSnapshot!);
}

// --------------------------------------------------------------------
// 2. 持久模式——复用同一个 Shell，并在调用之间保留状态。
// --------------------------------------------------------------------
Console.WriteLine("\n### 持久模式\n");
await using (var persistentShell = new LocalShellExecutor(new() { Mode = ShellMode.Persistent, AcknowledgeUnsafe = true }))
{
    var envProvider = new ShellEnvironmentProvider(persistentShell);
    var persistentAgent = aiProjectClient.AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = Instructions,
            Tools = [persistentShell.AsAIFunction(requireApproval: false)],
        },
        AIContextProviders = [envProvider],
    });

    var persistentSession = await persistentAgent.CreateSessionAsync();

    // 持久模式会在调用之间保留状态：进入临时目录，然后验证下一次调用是否仍能看到
    // 新的当前工作目录。
    Console.WriteLine(await persistentAgent.RunAsync("进入系统临时文件夹，然后输出当前工作目录。", persistentSession));
    Console.WriteLine();
    Console.WriteLine(await persistentAgent.RunAsync("在一次新的 Shell 调用中，再次输出当前工作目录。告诉我它是否仍与临时文件夹一致。", persistentSession));
    Console.WriteLine();

    // 导出的变量也是如此：在一次调用中设置，然后在下一次调用中读取。
    Console.WriteLine(await persistentAgent.RunAsync("将环境变量 DEMO_TOKEN 设置为值 'hello-world'。", persistentSession));
    Console.WriteLine();
    Console.WriteLine(await persistentAgent.RunAsync("输出 DEMO_TOKEN 的当前值。准确告诉我 Shell 返回的值。", persistentSession));
    Console.WriteLine();

    PrintSnapshot(envProvider.CurrentSnapshot!);
}

static void PrintSnapshot(ShellEnvironmentSnapshot snap)
{
    Console.WriteLine("--- 捕获的环境快照 ---");
    Console.WriteLine($"  系列：    {snap.Family}");
    Console.WriteLine($"  操作系统：{snap.OSDescription}");
    Console.WriteLine($"  Shell：   {snap.ShellVersion ?? "（未知）"}");
    Console.WriteLine($"  当前目录：{snap.WorkingDirectory}");
    foreach (var (tool, version) in snap.ToolVersions)
    {
        Console.WriteLine($"  {tool,-8} {version ?? "（未安装）"}");
    }
}