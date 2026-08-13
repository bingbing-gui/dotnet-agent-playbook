// See https://aka.ms/new-console-template for more information
using A2A;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.Text;
try
{
    Console.InputEncoding = Encoding.UTF8;
    Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

    var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
    var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME") ?? "gpt-4o-mini";
    var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("AZURE_OPENAI_API_KEY is not set.");

    // Initialize an A2ACardResolver to get an A2A agent card.

    A2ACardResolver agentCardResolver = new A2ACardResolver(new Uri("http://localhost:5000"));

    // Get the agent card
    AgentCard agentCard = await agentCardResolver.GetAgentCardAsync();


    AIAgent a2aAgent = agentCard.AsAIAgent();

    Console.WriteLine("==================== 远程调用 A2A ====================");
    //Console.WriteLine(await a2aAgent.RunAsync(" 解释什么是 A2A 协议"));
    await foreach (var update in a2aAgent.RunStreamingAsync("解释什么是 A2A 协议"))
    {
        if (string.IsNullOrEmpty(update.Text)==false)
            Console.Write(update.Text);
    }
    Console.WriteLine(string.Concat(Enumerable.Repeat("=", 50)));
    Console.WriteLine("==================== 远程调用 A2A 结束====================");

    Console.ReadLine();
    
    
    var callA2AAgent = AIFunctionFactory.Create(
        async (string input, CancellationToken ct) =>
        {
            Console.WriteLine("[Client Agent] 调用 Server A2A Agent...");
            Console.WriteLine($"📨 input: {input}");

            //var response = await a2aAgent.RunAsync(input, cancellationToken: ct);

            //Console.WriteLine("[ClientAgent] Server Agent 输出");
            //Console.WriteLine($"📩 output: {response.Text}");


            // 移除Console.WriteLine，只返回结果
            //var response = await a2aAgent.RunAsync(input, cancellationToken: ct);
            //return response.Text; // 只返回文本内容
        },
        new AIFunctionFactoryOptions
        {
            Name = "call_remote_agent",
            Description = """
                调用远程 A2A Agent，用于通用问答与推理。
                """
        }
    );
    Console.WriteLine("==================== 本地使用工具调用====================");
    var openAiClient = new OpenAIClient(
    new ApiKeyCredential(apiKey),
    new OpenAIClientOptions
    {
        // 重要：Endpoint 设置为 DeepSeek API 的地址，末尾不加 /v1
        Endpoint = new Uri(endpoint)
    }
);
    var chatClient = openAiClient.GetChatClient(modelId);
    AIAgent agent = chatClient.AsAIAgent(
            instructions: "你是一个乐于助人的助手。",
            tools: [callA2AAgent] // 将方法注册为工具
        );
    Console.WriteLine(string.Concat(Enumerable.Repeat("=", 50)));
    Console.WriteLine(await agent.RunAsync("请调用远程 agent 解释什么是 UDP 协议"));
    Console.WriteLine();
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
