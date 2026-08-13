using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(false);

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
var apikey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME");

#region Deepseek认证
var openAiClient = new OpenAIClient(
    new ApiKeyCredential(apikey),
    new OpenAIClientOptions
    {
        // 重要：Endpoint 设置为 DeepSeek API 的地址，末尾不加 /v1
        Endpoint = new Uri(endpoint)
    }
);
var chatClient = openAiClient.GetChatClient(modelId);
AIAgent agent = chatClient.AsAIAgent(
        instructions: "你是一位热情、知识渊博的旅行规划助手。当用户询问旅行建议时，你应当使用 'GetRandomDestination' 工具来随机选择一个目的地，然后为其规划一个简短的一日游行程。",
        tools: [AIFunctionFactory.Create(GetRandomDestination)] // 将方法注册为工具
    );

// --- 4. 运行 Agent ---
Console.WriteLine("正在与 DeepSeek Agent 对话...");
Console.WriteLine(string.Concat(Enumerable.Repeat('-', 120)));
Console.WriteLine("询问: " + "帮我规划一个一日游");

// 使用 RunStreamingAsync 可以流式输出，提升体验 [citation:3]
await foreach (var update in agent.RunStreamingAsync("帮我规划一个一日游"))
{
    Console.Write(update);
}
Console.WriteLine();
#endregion



// --- 2. 定义 Agent 可用的工具 (Tool) ---
// 这里定义一个返回随机旅行目的地的工具
// [Description] 属性帮助 AI 理解这个工具的用途，非常重要 [citation:3]
[Description("提供一个随机的度假目的地。")]
static string GetRandomDestination()
{
    var destinations = new[]
    {
        "巴黎, 法国", "东京, 日本", "纽约, 美国",
        "悉尼, 澳大利亚", "罗马, 意大利", "巴塞罗那, 西班牙"
    };
    var random = new Random();
    return destinations[random.Next(destinations.Length)];
}
