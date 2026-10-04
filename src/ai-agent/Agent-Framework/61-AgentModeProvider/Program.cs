// Agent Mode—使用 AgentModeProvider 在运行时切换代理的操作模式
// 此示例演示如何使用 AgentModeProvider。它是一个 AIContextProvider，能够在会话状态中
// 跟踪代理当前的操作“模式”，并公开相应工具（mode_get / mode_set），使代理可以随着任务推进
// 查询和切换模式。当前模式会在每轮对话中加入发送给模型的指令，从而让不同模式驱动不同的行为。
//
// 此示例演示以下两项内容：
//   1. 提供程序内置的默认模式（“plan”和“execute”）。
//   2. 如何通过 AgentModeProviderOptions 自定义可用模式。
//
// 程序运行一个简单的交互循环。除了与代理对话，还可以使用斜杠命令自行切换代理模式：
//   /mode            — 显示当前模式
//   /mode <name>     — 切换到指定模式
//   /help            — 列出可用命令和模式
//   /exit            — 退出
//
// 使用 /mode 切换模式时，提供程序会在下一轮对话中注入一条通知，
// 让代理明确感知模式变化并相应地调整行为。
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var model = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";


bool useCustomModes = string.Equals(Environment.GetEnvironmentVariable("AGENT_MODE_USE_CUSTOM"), "true", StringComparison.OrdinalIgnoreCase);

// <create_mode_provider>
// suppress experimental-API analyzer for this block

#pragma warning disable MAAI001
AgentModeProvider modeProvider;
string[] availableModes;

if (useCustomModes)
{
    modeProvider = new AgentModeProvider(new AgentModeProviderOptions
    {
        DefaultMode = "concise",
        Modes =
        [
            new AgentModeProviderOptions.AgentMode(
                "concise",
                "回答在一个简短的句子中。除非用户明确要求更多细节，否则不要详细说明。"),
            new AgentModeProviderOptions.AgentMode(
                "detailed",
                "详细回答。解释你的推理，提供示例，并涵盖相关的边缘情况。"),
        ],
    });

    availableModes = ["concise", "detailed"];
}
else
{
    modeProvider = new AgentModeProvider();
    availableModes = ["plan", "execute"];
}
#pragma warning restore MAAI001

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "ModeAwareAssistant",
        ChatOptions = new ChatOptions
        {
            ModelId = model,
            Instructions = "你是一个乐于助人的助手。遵循当前操作模式所要求的流程和行为。",
        },
        AIContextProviders = [modeProvider],
    });

AgentSession session = await agent.CreateSessionAsync();

Console.WriteLine("Agent Model例子. 输入一个消息与代理聊天，或使用斜杠命令。");
Console.WriteLine($"可用的模式: {string.Join(", ", availableModes)}");
Console.WriteLine($"当前模式: {modeProvider.GetMode(session)}");
PrintHelp(availableModes);
Console.WriteLine();

while (true)
{
    Console.Write("> ");
    string? input = Console.ReadLine()?.Trim();

    // 将空输入或流结束（Ctrl+D / Ctrl+Z）视为退出请求。
    if (string.IsNullOrWhiteSpace(input) || input.Equals("/exit", StringComparison.OrdinalIgnoreCase))
    {
        break;
    }

    if (input.Equals("/help", StringComparison.OrdinalIgnoreCase))
    {
        PrintHelp(availableModes);
        continue;
    }

    // 处理 /mode 斜杠命令: "/mode" 显示当前模式, "/mode <name>" 切换到该模式。
    if (input.Equals("/mode", StringComparison.OrdinalIgnoreCase) || input.StartsWith("/mode ", StringComparison.OrdinalIgnoreCase))
    {
        string[] parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length < 2)
        {
            Console.WriteLine($"当前模式: {modeProvider.GetMode(session)}");
            continue;
        }

        try
        {
            modeProvider.SetMode(session, parts[1]);
            Console.WriteLine($"切换到 \"{parts[1]}\" 模式。");
        }
        catch (ArgumentException ex)
        {
            // SetModeAsync throws when the requested mode is not one of the configured modes.
            Console.WriteLine(ex.Message);
        }

        continue;
    }

    // 其他任何内容都是发送给代理的消息。模式提供程序将当前模式（以及任何待处理的模式更改通知）注入到此轮的上下文中。
    Console.WriteLine(await agent.RunAsync(input, session));
    // 在回合结束后打印模式: 代理可能已经通过 mode_set 工具自行切换了模式，因此这反映了代理在回合中所做的任何更改。
    Console.WriteLine($"当前模式: {modeProvider.GetMode(session)}");
}

static void PrintHelp(string[] availableModes)
{
    Console.WriteLine("Commands:");
    Console.WriteLine("  /mode            现实当前模式");
    Console.WriteLine($"  /mode <name>     切换模式 ({string.Join(" | ", availableModes)})");
    Console.WriteLine("  /help            显示此帮助");
    Console.WriteLine("  /exit            退出");
}