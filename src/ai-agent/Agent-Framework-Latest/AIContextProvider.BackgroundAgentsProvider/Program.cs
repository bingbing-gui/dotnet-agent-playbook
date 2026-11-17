#pragma warning disable MAAI001, OPENAI001
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Diagnostics;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";


AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());


IChatClient chatClient = aiProjectClient
    .GetProjectOpenAIClient()
    .GetResponsesClient()
    .AsIChatClient(deploymentName);


AIAgent productAgent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "ProductResearchAgent",
        Description = "负责分析产品价值、适用场景和用户收益。",
        ChatOptions = new ChatOptions
        {
            Instructions =
                """
                你是产品研究专家。
                根据收到的研究任务，分析产品解决的问题、
                适用场景、用户收益和不适用的场景。
                返回简洁、有结构的中文结论。
                不要分析安全风险。
                """
        }
    });

AIAgent riskAgent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "RiskAnalysisAgent",
        Description = "负责分析技术、安全、成本和运行风险。",
        ChatOptions = new ChatOptions
        {
            Instructions =
                """
                你是技术风险分析专家。
                根据收到的研究任务，分析技术风险、安全风险、
                成本风险、运行维护风险以及对应的缓解措施。
                返回简洁、有结构的中文结论。
                不要重复产品价值分析。
                """
        }
    });

var backgroundAgentsProvider =
    new BackgroundAgentsProvider(
        [
            productAgent,
            riskAgent
        ],
        new BackgroundAgentsProviderOptions
        {
            WaitTimeout = TimeSpan.FromMinutes(2),

            Instructions =
                """
                ## 后台 Agent

                你可以通过 background_agents_* 工具将任务
                委托给后台 Agent。

                将相互独立的任务分别交给合适的后台 Agent。
                必须先启动全部后台任务，再等待结果，不要启动
                一个任务后立即等待。

                后台任务启动后会返回 taskId。使用等待工具检查
                任务是否完成，再使用结果工具读取任务结果。

                如果结果不完整，可以继续原任务。读取最终结果后，
                清理已经完成的任务。

                {background_agents}
                """
        });

AIAgent coordinatorAgent = chatClient.AsAIAgent(
    new ChatClientAgentOptions
    {
        Name = "ResearchCoordinator",

        Description =
            "负责拆解、并行委托和汇总研究任务。",

        ChatOptions = new ChatOptions
        {
            Instructions =
                """
                你是研究协调者，不要独自完成所有分析。

                收到研究问题后
                把产品价值分析交给 ProductResearchAgent
                把风险分析交给 RiskAnalysisAgent。

                必须先启动两个后台任务，使它们并发运行。
                等待任务完成后，读取两个任务的结果，并对结果
                进行综合判断。

                最终报告需要说明产品价值、适用场景、主要风险、
                缓解措施以及是否建议采用。

                生成最终报告后，清理已经完成的后台任务。
                """
        },

        AIContextProviders =
        [
            backgroundAgentsProvider
        ]
    });

AgentSession parentSession = await coordinatorAgent.CreateSessionAsync();

var stopwatch = Stopwatch.StartNew();

try
{
    Task<AgentResponse> runTask =
            coordinatorAgent.RunAsync(
            """
            请评估在企业内部知识库项目中采用向量数据库的
            产品价值和风险。
            产品价值与风险分析应由不同的后台 Agent
            并行完成，最后给出是否建议采用的结论。
            """,
            parentSession);

    while (!runTask.IsCompleted)
    {
        IReadOnlyList<BackgroundTaskInfo> runningTasks =
            backgroundAgentsProvider.GetIncompleteTasks(parentSession);

        Console.Clear();
        Console.WriteLine("父 Agent 正在运行……");
        Console.WriteLine();

        if (runningTasks.Count == 0)
        {
            Console.WriteLine("正在拆解任务、等待模型响应或汇总结果……");
        }
        else
        {
            foreach (BackgroundTaskInfo task in runningTasks)
            {
                Console.WriteLine(
                    $"任务 {task.Id} · {task.AgentName}");
                Console.WriteLine(
                    $"状态：{task.Status}");
                Console.WriteLine(
                    $"内容：{task.Description}");
                Console.WriteLine();
            }
        }

        await Task.Delay(TimeSpan.FromSeconds(1));
    }

    AgentResponse response = await runTask;

    Console.WriteLine("========== 最终结果 ==========");
    Console.WriteLine(response.Text);
}
finally
{
    await backgroundAgentsProvider.ReleaseSessionAsync(
        parentSession,
        cancelRunning: true,
        timeout: TimeSpan.FromSeconds(30));
}

