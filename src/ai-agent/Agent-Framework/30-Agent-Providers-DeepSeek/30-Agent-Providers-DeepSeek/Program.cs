using Anthropic;
using Anthropic.Core;
using Anthropic.Models.Beta.Sessions;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using System.ClientModel;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


Console.WriteLine("## 基于 OpenAI 协议接入 DeepSeek V4 Pro");
#pragma warning disable OPENAI001

var client = new OpenAIClient(
    new ApiKeyCredential("sk-"),
    new OpenAIClientOptions
    {
        Endpoint = new Uri("https://api.deepseek.com")
    });

var chatClient = client.GetChatClient("deepseek-v4-pro");

var result = await chatClient.AsAIAgent().RunAsync("你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。");

Console.WriteLine(result);

Console.WriteLine(new string('-', 60));

#region 不支持Response方式
//AIAgent agent = client
//    .GetResponsesClient()
//    .AsAIAgent(
//        model: "deepseek-v4-pro", // ⭐DeepSeek模型名
//        instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。",
//        name: "Joker"
//    );

// 调用
//var result = await agent.RunAsync("你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。");
#endregion


Console.WriteLine("## 基于 Anthropic 协议接入 DeepSeek V4 Pro");

#pragma warning restore OPENAI001

var client2 = new AnthropicClient(new ClientOptions
{
    ApiKey = "sk-",
    BaseUrl = "https://api.deepseek.com/anthropic"
});

var agent = client2.AsIChatClient("deepseek-v4-pro");

result = await agent.AsAIAgent().RunAsync("你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。");

Console.WriteLine(result);
