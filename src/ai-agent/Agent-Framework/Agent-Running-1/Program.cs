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
var modeId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME");

OpenAIClient openAIClient = new OpenAIClient(
    new ApiKeyCredential(apikey),
    new OpenAIClientOptions
    {
        Endpoint=new Uri(endpoint)
    }
    );

AIAgent aIAgent = openAIClient.GetChatClient(modeId).AsAIAgent(
    instructions: "你是一位热心且知识渊博的旅行博主，擅于帮人规划旅行路线，请使用相关工具进行规划。",
    tools: [AIFunctionFactory.Create(GetCity)]    
);

Console.WriteLine("正在与DEEPSEEK对话");
Console.WriteLine(string.Concat(Enumerable.Repeat('=', 120)));
await foreach (var update in aIAgent.RunStreamingAsync("请帮我规划一日游"))
{
    Console.Write(update.Text);
}
Console.WriteLine("\r\n");
Console.WriteLine(string.Concat(Enumerable.Repeat('=', 120)));
Console.WriteLine();


[Description("随机获取一个城市名称")]
static string GetCity()
{
    var citys = new string[]
    {
        "巴黎, 法国", "东京, 日本", "纽约, 美国",
        "悉尼, 澳大利亚", "罗马, 意大利", "巴塞罗那, 西班牙"
    };
    Random rnd = new Random();
    return citys[rnd.Next(citys.Length)];
}