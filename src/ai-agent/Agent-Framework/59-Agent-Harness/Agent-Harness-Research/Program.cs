
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4";


var harnessAgentOptions = new HarnessAgentOptions
{
    Name = "research-agent",
    HarnessInstructions = "有明确目的时再调用工具，结论应有已核实的资料支持。用中文回答。",
    ChatOptions = new ChatOptions
    {
        Instructions = "你是一名研究助手，优先使用学术资料。用中文回答。",
    },
    MaxContextWindowTokens = 128_000,
    MaxOutputTokens = 16_384,
    AgentModeProviderOptions = new AgentModeProviderOptions
    {
        DefaultMode = "plan",
    },
    LoopEvaluators =
    [
        new TodoCompletionLoopEvaluator(
            new TodoCompletionLoopEvaluatorOptions
            {
                Modes = ["execute"],
            }),
    ],
    LoopAgentOptions = new LoopAgentOptions { MaxIterations = 10 },
};


AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .GetProjectOpenAIClient()
    .GetResponsesClient()
    .AsIChatClient(deploymentName)
    .AsHarnessAgent(harnessAgentOptions);


Console.WriteLine(await agent.RunAsync("研究恐龙灭绝的根本原因"));


Console.ReadLine();
