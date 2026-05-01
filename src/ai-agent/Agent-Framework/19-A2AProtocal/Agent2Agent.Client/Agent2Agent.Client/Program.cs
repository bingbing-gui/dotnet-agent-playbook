// See https://aka.ms/new-console-template for more information
using A2A;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ComponentModel;
using System.Text;
try
{
    Console.InputEncoding = Encoding.UTF8;
    Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
    var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";
    // Initialize an A2ACardResolver to get an A2A agent card.

    A2ACardResolver agentCardResolver = new A2ACardResolver(new Uri("http://localhost:5000"));

    // Get the agent card
    AgentCard agentCard = await agentCardResolver.GetAgentCardAsync();


    AIAgent a2aAgent = agentCard.AsAIAgent();


    //Console.WriteLine(await a2aAgent.RunAsync(" 解释什么是 A2A 协议"));

    var callA2AAgent = AIFunctionFactory.Create(
        async (string input, CancellationToken ct) =>
        {
            Console.WriteLine("[Client Agent] 调用 Server A2A Agent...");
            Console.WriteLine($"📨 input: {input}");

            var response = await a2aAgent.RunAsync(input, cancellationToken: ct);

            Console.WriteLine("[ClientAgent] Server Agent 输出");
            Console.WriteLine($"📩 output: {response.Text}");
        },
        new AIFunctionFactoryOptions
        {
            Name = "call_remote_agent",
            Description = """
        调用远程 A2A Agent，用于通用问答与推理。
        """
        }
    );
    // Create the main agent, and provide the a2a agent skills as a function tools.
    AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
        .GetChatClient(deploymentName)
        .AsIChatClient()
        .AsAIAgent(
            name: "ClientAgent",
            instructions: "你是一个乐于助人的助手。",
            tools: [callA2AAgent]);




    Console.WriteLine(await agent.RunAsync("请调用远程 agent 解释什么是 A2A 协议"));

}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
