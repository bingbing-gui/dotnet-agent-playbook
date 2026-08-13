#pragma warning disable MEAI001
//改造成带审批功能的AI

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.Diagnostics.Metrics;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var apikey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
var modleId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME") ?? "gpt-4o-mini";


// ============================================================
// 方式一：通过 AzureOpenAIClient / ChatClient 创建 Agent
// ============================================================
OpenAIClient client = new OpenAIClient(
    new ApiKeyCredential(apikey),
    new OpenAIClientOptions
    {
        Endpoint=new Uri( endpoint)    
    }
    );

// 1. 创建原始工具函数
AIFunction getNewsFunction = AIFunctionFactory.Create(GetNews);
// 2. 用 ApprovalRequiredAIFunction 包装，标记为需要审批
AIFunction approvalRequiredNewsFunction = new ApprovalRequiredAIFunction(getNewsFunction);

AIAgent openAIAgent = client.GetChatClient(modleId).AsAIAgent(
    name: "toolsAgent",
    instructions: "你是一个乐于助人的助手",

    tools: [approvalRequiredNewsFunction]
    );

AgentSession openAISession = await openAIAgent.CreateSessionAsync();
var openAIResponse = await openAIAgent.RunAsync("美国的最新新闻是什么？", openAISession);
List<ToolApprovalRequestContent> openAIApprovalRequests = openAIResponse.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();

while (openAIApprovalRequests.Count > 0)
{
   
    var userInputResponses = new List<Microsoft.Extensions.AI.ChatMessage>();
    bool userRejected = false;  // 标记用户是否拒绝
    foreach (var request in openAIApprovalRequests)
    {
        // 获取要调用的函数名
        var functionCall = (FunctionCallContent)request.ToolCall;
        string functionName = functionCall.Name;

        // 提示用户是否批准
        Console.WriteLine($"代理想调用函数：{functionName}");
        Console.Write("是否批准？(输入 Y 表示同意，其他表示拒绝): ");

        string userInput = Console.ReadLine() ?? "";
        bool isApproved = userInput.Equals("Y", StringComparison.OrdinalIgnoreCase);
        // 如果用户拒绝，显示拒绝信息并停止处理后续请求
        if (!isApproved)
        {
            Console.WriteLine($"已拒绝调用函数：{functionName}，操作终止。");
            userRejected = true;
            continue;
        }
        // 创建审批结果（同意或拒绝）
        var approvalResult = request.CreateResponse(isApproved);
        
        // 包装成用户消息
        var userResponse = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, [approvalResult]);
        userInputResponses.Add(userResponse);
    }
    // 用户拒绝了，退出整个审批循环

    // 把用户的审批结果发回给 Agent，继续执行
    if (userInputResponses == null || userInputResponses.Count == 0)
        break;
    else
    {
        openAIResponse = await openAIAgent.RunAsync(userInputResponses, openAISession);

        // 检查是否还有新的审批请求
        openAIApprovalRequests = openAIResponse.Messages
            .SelectMany(m => m.Contents)
            .OfType<ToolApprovalRequestContent>()
            .ToList();
    }

    
}
Console.WriteLine($"\nAgent: {openAIResponse}");

[Description("获取指定国家的最新新闻标题。")]
static string GetNews([Description("国家名称。")] string country)
    => $"来自 {country} 的头条新闻：AI 正在革新软件开发领域。";
