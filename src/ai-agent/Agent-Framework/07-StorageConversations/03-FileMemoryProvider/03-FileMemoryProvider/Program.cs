
#pragma warning disable MAAI001 

using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = "https://maf.services.ai.azure.com/api/projects/maf"; //Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = "gpt-4o";//Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

const string UserId = "UID1";

var memoryRoot = Path.Combine(AppContext.BaseDirectory, "agent-memory");
var fileStore = new FileSystemAgentFileStore(memoryRoot);

var workingFolder = $"users/{UserId}";

Console.WriteLine($"记忆文件被写入到: {Path.Combine(memoryRoot, workingFolder)}");
Console.WriteLine();


using var fileMemoryProvider = new FileMemoryProvider(
    fileStore,
    _ => new FileMemoryState { WorkingFolder = workingFolder });

AIAgent agent = new AIProjectClient(
        new Uri(endpoint),
        new DefaultAzureCredential())
    .AsAIAgent(new ChatClientAgentOptions
    {
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "你是一个乐于助人的旅行助手。记住用户告诉你的关于他们自己的信息，以便以后提供更好的建议。"
        },
        Name = "TravelAssistant",
        AIContextProviders = [fileMemoryProvider],
    });


AgentSession firstSession = await agent.CreateSessionAsync();
Console.WriteLine("=== 第一次对话 ===");
Console.WriteLine(await agent.RunAsync(
    "你好，我的名字是桂兵兵，我计划去北海道旅游，我和朋友去旅游，帮我找风景优美的景点。",
    firstSession));
Console.WriteLine();

// 显示代理在磁盘上创建的记忆文件。
Console.WriteLine("=== 磁盘文件===");
foreach (var file in Directory.EnumerateFiles(Path.Combine(memoryRoot, workingFolder)))
{
    Console.WriteLine(Path.GetFileName(file));
}

Console.WriteLine();

AgentSession secondSession = await agent.CreateSessionAsync();
Console.WriteLine("=== 第二次对话 (新会话) ===");
Console.WriteLine(await agent.RunAsync("你能回顾一下你记得的个人信息吗？", secondSession));