// Copyright (c) Microsoft. All rights reserved.

// This sample shows multiple middleware layers working together with Azure OpenAI:
// chat client (global/per-request), agent run (PII filtering and guardrails),
// function invocation (logging and result overrides), and human-in-the-loop
// approval workflows for sensitive function calls.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// 从环境变量中获取Azure Foundry 端点和部署名称
var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o";



[Description("获取指定位置的天气.")]
static string GetWeather([Description("用于查询天气的地点.")] string location)
    => $"{location} 的天气是多云，最高气温为 15°C。";

[Description("当前的日期时间偏移量")]
static string GetDateTime() => DateTimeOffset.Now.ToString();

// 创建Azure OpenAI客户端并获取ChatCliet对象
var azureOpenAIClient = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetChatClient(deploymentName);

// 构建 Agent 时注入底层中间件
var originalAgent = azureOpenAIClient.AsIChatClient()
    .AsBuilder()
    .Use(getResponseFunc: ChatClientMiddleware, getStreamingResponseFunc: null)
    .BuildAIAgent(instructions: "你是一个帮助人们查找信息的 AI 助手。", tools: [AIFunctionFactory.Create(GetDateTime, name: nameof(GetDateTime))]);

// 添加中间件在agent级别，并在其上构建一个新的代理
var middlewareEnabledAgent = originalAgent
    .AsBuilder()
    .Use(FunctionCallMiddleware)
    .Use(FunctionCallOverrideWeather)
    .Use(PIIMiddleware, null)
    .Use(GuardrailMiddleware, null)
    .Build();

var thread = middlewareEnabledAgent.GetNewThread();

Console.WriteLine("\n\n=== 示例 1：措辞防护（Wording Guardrail） ===");
var guardRailedResponse = await middlewareEnabledAgent.RunAsync("告诉我一些有害的内容。");
Console.WriteLine($"防护后的响应：{guardRailedResponse}");


Console.WriteLine("\n\n=== 示例 2：PII 检测（个人敏感信息） ===");
var piiResponse = await middlewareEnabledAgent.RunAsync("我的名字是 John Doe，电话是 123-456-7890，邮箱是 john@something.com");
Console.WriteLine($"PII 过滤后的响应：{piiResponse}");


Console.WriteLine("\n\n=== 示例 3：Agent 函数中间件 ===");

var options = new ChatClientAgentRunOptions(new()
{
    Tools = [AIFunctionFactory.Create(GetWeather, name: nameof(GetWeather))]
});

var functionCallResponse = await middlewareEnabledAgent.RunAsync("西雅图现在几点了？天气怎么样？", thread, options);
Console.WriteLine($"函数调用响应: {functionCallResponse}");


async Task<ChatResponse> ChatClientMiddleware(IEnumerable<ChatMessage> message, ChatOptions? options, IChatClient innerChatClient, CancellationToken cancellationToken)
{
    Console.WriteLine("Chat Client 中间件 - 运行前聊天");
    var response = await innerChatClient.GetResponseAsync(message, options, cancellationToken);
    Console.WriteLine("Chat Client 中间件 - 运行后聊天");
    return response;
}

async ValueTask<object?> FunctionCallMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    Console.WriteLine($"函数: {context!.Function.Name} - 中间件 1 执行前");
    var result = await next(context, cancellationToken);
    Console.WriteLine($"函数: {context!.Function.Name} - 中间件 1 执行后");
    return result;
}

async ValueTask<object?> FunctionCallOverrideWeather(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
{
    Console.WriteLine($"函数: {context!.Function.Name} - 中间件 2 执行前");

    var result = await next(context, cancellationToken);

    if (context.Function.Name == nameof(GetWeather))
    {
        // Override the result of the GetWeather function
        result = "天气晴朗，最高气温25°C。";
    }
    Console.WriteLine($"函数: {context!.Function.Name} - 中间件 2 执行后");
    return result;
}


async Task<AgentRunResponse> PIIMiddleware(IEnumerable<ChatMessage> messages, AgentThread? thread, AgentRunOptions? options, AIAgent innerAgent, CancellationToken cancellationToken)
{
    var filteredMessages = FilterMessages(messages);
    Console.WriteLine("Pii 中间件 - 运行前过滤消息");

    var response = await innerAgent.RunAsync(filteredMessages, thread, options, cancellationToken).ConfigureAwait(false);

    response.Messages = FilterMessages(response.Messages);

    Console.WriteLine("Pii 中间件 - 运行后过滤消息");

    return response;

    static IList<ChatMessage> FilterMessages(IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => new ChatMessage(m.Role, FilterPii(m.Text))).ToList();
    }

    static string FilterPii(string content)
    {
        Regex[] piiPatterns =
        [
            new(@"\b\d{3}-\d{3}-\d{4}\b", RegexOptions.Compiled), // 电话号码( 123-456-7890)
            new(@"\b[\w\.-]+@[\w\.-]+\.\w+\b", RegexOptions.Compiled), // 邮件
            new(@"\b[A-Z][a-z]+\s[A-Z][a-z]+\b", RegexOptions.Compiled) // 全名
        ];

        foreach (var pattern in piiPatterns)
        {
            content = pattern.Replace(content, "[已屏蔽: PII]");
        }
        return content;
    }
}

// This middleware enforces guardrails by redacting certain keywords from input and output messages.
async Task<AgentRunResponse> GuardrailMiddleware(IEnumerable<ChatMessage> messages, AgentThread? thread, AgentRunOptions? options, AIAgent innerAgent, CancellationToken cancellationToken)
{
    var filteredMessages = FilterMessages(messages);

    Console.WriteLine("Guardrail 中间件 - 运行前过滤消息");

    var response = await innerAgent.RunAsync(filteredMessages, thread, options, cancellationToken);

    response.Messages = FilterMessages(response.Messages);

    Console.WriteLine("Guardrail 中间件 - 运行后过滤消息");

    return response;

    List<ChatMessage> FilterMessages(IEnumerable<ChatMessage> messages)
    {
        return messages.Select(m => new ChatMessage(m.Role, FilterContent(m.Text))).ToList();
    }

    static string FilterContent(string content)
    {
        foreach (var keyword in new[] { "有害", "非法", "暴力" })
        {
            if (content.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return "[已屏蔽：包含禁止内容]";
            }
        }

        return content;
    }
}


//Console.WriteLine("\n\n=== 示例 4：按请求的中间件（Per-request middleware），带“人工审批”的函数调用授权（Human-in-the-loop approval）===");

//#pragma warning disable MEAI001 
//var optionsWithApproval = new ChatClientAgentRunOptions(new()
//{
//    // Adding a function with approval required
//    Tools = [new ApprovalRequiredAIFunction(AIFunctionFactory.Create(GetWeather, name: nameof(GetWeather)))],
//})
//{
//    ChatClientFactory = (chatClient) => chatClient
//        .AsBuilder()
//        .Use(PerRequestChatClientMiddleware, null) // Using the non-streaming for handling streaming as well
//        .Build()
//};
//#pragma warning restore MEAI001

//var response = await originalAgent
//    .AsBuilder()
//    .Use(PerRequestFunctionCallingMiddleware)
//    .Use(ConsolePromptingApprovalMiddleware, null)
//    .Build()
//    .RunAsync("西雅图现在是什么时间，天气怎样？", thread, optionsWithApproval);


//async ValueTask<object?> PerRequestFunctionCallingMiddleware(AIAgent agent, FunctionInvocationContext context, Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
//{
//    Console.WriteLine($"Agent Id: {agent.Id}");
//    Console.WriteLine($"函数: {context!.Function.Name} - 按请求级别执行前");
//    var result = await next(context, cancellationToken);
//    Console.WriteLine($"函数: {context!.Function.Name} - 按请求级别执行后");
//    return result;
//}

//async Task<AgentRunResponse> ConsolePromptingApprovalMiddleware(IEnumerable<ChatMessage> messages, AgentThread? thread, AgentRunOptions? options, AIAgent innerAgent, CancellationToken cancellationToken)
//{
//    var response = await innerAgent.RunAsync(messages, thread, options, cancellationToken);

//    var userInputRequests = response.UserInputRequests.ToList();

//    while (userInputRequests.Count > 0)
//    {
//#pragma warning disable MEAI001
//        response.Messages = userInputRequests
//            .OfType<FunctionApprovalRequestContent>()
//            .Select(functionApprovalRequest =>
//            {
//                Console.WriteLine($"代理想调用以下函数，请回复 Y 以批准：名称 {functionApprovalRequest.FunctionCall.Name}");
//                return new ChatMessage(ChatRole.User, [functionApprovalRequest.CreateResponse(Console.ReadLine()?.Equals("Y", StringComparison.OrdinalIgnoreCase) ?? false)]);
//            })
//            .ToList();
//#pragma warning restore MEAI001
//        response = await innerAgent.RunAsync(response.Messages, thread, options, cancellationToken);
//        userInputRequests = response.UserInputRequests.ToList();
//    }

//    return response;
//}


//async Task<ChatResponse> PerRequestChatClientMiddleware(IEnumerable<ChatMessage> message, ChatOptions? options, IChatClient innerChatClient, CancellationToken cancellationToken)
//{
//    Console.WriteLine("Per-Request Chat Client 中间件 - 运行前聊天");
//    var response = await innerChatClient.GetResponseAsync(message, options, cancellationToken);
//    Console.WriteLine("Per-Request Chat Client 中间件 - 运行后聊天");

//    return response;
//}

//Console.WriteLine($"按请求级别的中间件响应： {response}");



