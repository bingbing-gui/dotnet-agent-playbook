// Copyright (c) Microsoft. All rights reserved.
//
// 改造说明：将 Azure OpenAI Responses API 替换为 OpenAI Chat Completions API。
// 关键变更：
//   1. AzureOpenAIClient  →  OpenAIClient
//   2. AzureCliCredential  →  ApiKeyCredential
//   3. GetResponsesClient()  →  GetChatClient(model)   ★ 核心改动
//      原因：Responses API (/v1/responses) 仅 OpenAI 官方支持，
//            大多数兼容性 API 只支持 Chat Completions (/v1/chat/completions)
//   4. 环境变量：AZURE_OPENAI_ENDPOINT / AZURE_OPENAI_DEPLOYMENT_NAME
//      →  OPENAI_API_KEY / OPENAI_MODEL_NAME / OPENAI_ENDPOINT (可选)
//
// 兼容性 API 示例：
//   set OPENAI_API_KEY=sk-xxx
//   set OPENAI_MODEL_NAME=gpt-4o-mini
//   set OPENAI_ENDPOINT=https://your-proxy.com/v1   (如使用中转/代理服务)

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;          // IChatClient, ChatOptions, AsAIAgent() 扩展方法

using OpenAI;
using System.ClientModel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- 配置 ---
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT");
var apikey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("OPENAI_API_KEY 未设置。");
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME")
    ?? "gpt-4o-mini";

// --- 诊断输出 ---
Console.WriteLine($"[配置] Endpoint = {(string.IsNullOrEmpty(endpoint) ? "https://api.openai.com/v1 (默认)" : endpoint)}");
Console.WriteLine($"[配置] Model   = {modelId}");
Console.WriteLine($"[配置] API Key = {(apikey.Length > 8 ? apikey[..8] + "..." : "???")}");
Console.WriteLine();

// --- 基于类的技能 ---
var unitConverter = new UnitConverterSkill();

// --- 技能提供程序 ---
#pragma warning disable MAAI001
var skillsProvider = new AgentSkillsProvider(unitConverter);
#pragma warning restore MAAI001

// --- Agent 设置 ---
//  ★ 关键改动：用 OpenAIClient 替代 AzureOpenAIClient
//
//  原始（Azure OpenAI）：
//    new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
//
//  改造后（标准 OpenAI / 兼容性 API）：
//    new OpenAIClient(new ApiKeyCredential(apiKey))
//
//  如果需要自定义端点（OpenAI 兼容服务）：
//    new OpenAIClient(
//        new ApiKeyCredential(apiKey),
//        new OpenAIClientOptions { Endpoint = new Uri(customEndpoint) })

OpenAIClientOptions? clientOptions = null;
if (!string.IsNullOrEmpty(endpoint))
{
    clientOptions = new OpenAIClientOptions { Endpoint = new Uri(endpoint) };
}

OpenAIClient openAIClient = clientOptions != null
    ? new OpenAIClient(new ApiKeyCredential(apikey), clientOptions)
    : new OpenAIClient(new ApiKeyCredential(apikey));

//  ★ 核心改动：GetResponsesClient() → GetChatClient(modelId).AsIChatClient()
//
//  Responses API (之前):
//    openAIClient.GetResponsesClient().AsAIAgent(options, model: modelId)
//    → 请求 POST /v1/responses  (仅 OpenAI 官方支持，兼容服务返回 404)
//
//  Chat Completions API (现在):
//    openAIClient.GetChatClient(modelId).AsIChatClient().AsAIAgent(options)
//    → 请求 POST /v1/chat/completions  (所有 OpenAI 兼容服务均支持)
//
//  关键：AsAIAgent() 是 IChatClient 接口上的扩展方法（来自 Microsoft.Agents.AI），
//        而 GetChatClient() 返回的是 OpenAI.Chat.ChatClient 类型，
//        必须先调用 AsIChatClient()（来自 Microsoft.Extensions.AI.OpenAI）转换为 IChatClient。

#pragma warning disable OPENAI001
AIAgent agent = openAIClient
    .GetChatClient(modelId)
    .AsIChatClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个可以进行单位换算的助手。",
        },
        AIContextProviders = [skillsProvider],
    });
#pragma warning restore OPENAI001

// === 步骤 1：基础对话测试（无技能、无工具） ===
// 目的：验证与 DeepSeek 的基本连通性和模型可用性
Console.WriteLine("=== 步骤 1：基础对话测试（无技能、无工具） ===");
Console.WriteLine(new string('-', 60));

IChatClient testClient = openAIClient
    .GetChatClient(modelId)
    .AsIChatClient();

List<ChatMessage> testMessages = [
    new(ChatRole.System, "你是一个有帮助的助手。"),
    new(ChatRole.User, "1+1等于几？请直接回答。")
];

try
{
    ChatResponse testResponse = await testClient.GetResponseAsync(testMessages);
    Console.WriteLine($"基础对话响应: {testResponse.Text}");
    Console.WriteLine($"  Token: Input={testResponse.Usage.InputTokenCount}, Output={testResponse.Usage.OutputTokenCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"基础对话失败: {ex.Message}");
}
Console.WriteLine();

// === 步骤 2：手动工具调用测试 ===
// 目的：验证 DeepSeek 是否支持 function calling
Console.WriteLine("=== 步骤 2：手动工具调用测试 ===");
Console.WriteLine(new string('-', 60));

var addFunc = AIFunctionFactory.Create(
    (double a, double b) => (a + b).ToString(),
    name: "add",
    description: "将两个数字相加并返回结果");

IChatClient funcClient = new ChatClientBuilder(
    openAIClient.GetChatClient(modelId).AsIChatClient())
    .UseFunctionInvocation()
    .Build();

List<ChatMessage> funcMessages = [
    new(ChatRole.System, "你是一个助手。需要计算时必须使用 add 工具。"),
    new(ChatRole.User, "帮我计算 25 + 17")
];

try
{
    ChatResponse funcResponse = await funcClient.GetResponseAsync(funcMessages, new ChatOptions
    {
        Tools = [addFunc]
    });
    Console.WriteLine($"工具调用响应: {funcResponse.Text}");
    Console.WriteLine($"  Token: Input={funcResponse.Usage.InputTokenCount}, Output={funcResponse.Usage.OutputTokenCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"工具调用失败: {ex.Message}");
}
Console.WriteLine();

// === 步骤 3：基于类的技能进行单位换算 ===
// 目的：测试 AgentSkillsProvider + AgentClassSkill 是否正常工作
Console.WriteLine("=== 步骤 3：基于类的技能进行单位换算 ===");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync("马拉松（26.2 英里）是多少公里？75 千克又是多少磅？");

Console.WriteLine($"助手：{response.Text}");
Console.WriteLine($"  Text is null:  {response.Text is null}");
Console.WriteLine($"  Text length:   {response.Text?.Length ?? -1}");
Console.WriteLine($"  Messages count: {response.Messages.Count}");
foreach (var msg in response.Messages)
{
    Console.WriteLine($"  --- Message [{msg.Role}] ---");
    Console.WriteLine($"    Text: {(msg.Text?.Length > 200 ? msg.Text[..200] + "..." : msg.Text ?? "(null)")}");
    // 遍历所有 Contents 项，包括 function call / function result
    foreach (var content in msg.Contents)
    {
        Console.WriteLine($"    Content: {content.GetType().Name}");
        if (content is TextContent tc)
            Console.WriteLine($"      → Text: {(tc.Text?.Length > 200 ? tc.Text[..200] + "..." : tc.Text)}");
        else if (content is FunctionCallContent fcc)
            Console.WriteLine($"      → CallId: {fcc.CallId}, Name: {fcc.Name}, Args: {JsonSerializer.Serialize(fcc.Arguments)}");
        else if (content is FunctionResultContent frc)
            Console.WriteLine($"      → CallId: {frc.CallId}, Name: {frc.CallId}, Result: {frc.Result}");
        else
            Console.WriteLine($"      → ToString: {content}");
    }
}

Console.WriteLine();

// === 步骤 3b：手动注册同样的工具（绕过 AgentSkillsProvider）===
// 目的：如果步骤 3 失败但 3b 成功，说明问题在 AgentSkillsProvider 的工具注册方式
Console.WriteLine("=== 步骤 3b：手动注册 convert 工具（绕过 AgentSkillsProvider）===");
Console.WriteLine(new string('-', 60));

var convertFunc = AIFunctionFactory.Create(
    (double value, double factor) =>
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    },
    name: "convert",
    description: "将数值与换算因子相乘，并以 JSON 返回结果。参数: value (要换算的数值), factor (换算因子)");

IChatClient manualClient = new ChatClientBuilder(
    openAIClient.GetChatClient(modelId).AsIChatClient())
    .UseFunctionInvocation()
    .Build();

string conversionTableText = """
    # 换算表
    公式：result = value × factor
    | 从 | 到 | 因子 |
    |英里|公里|1.60934|
    |公里|英里|0.621371|
    |磅|千克|0.453592|
    |千克|磅|2.20462|
    """;

List<ChatMessage> manualMessages = [
    new(ChatRole.System, $"你是一个可以进行单位换算的助手。\n\n参考换算表:\n{conversionTableText}\n\n当用户请求换算时，使用 convert 工具计算。"),
    new(ChatRole.User, "马拉松（26.2 英里）是多少公里？75 千克又是多少磅？")
];

try
{
    ChatResponse manualResponse = await manualClient.GetResponseAsync(manualMessages, new ChatOptions
    {
        Tools = [convertFunc]
    });
    Console.WriteLine($"手动工具响应: {manualResponse.Text}");
    Console.WriteLine($"  Token: Input={manualResponse.Usage.InputTokenCount}, Output={manualResponse.Usage.OutputTokenCount}");
}
catch (Exception ex)
{
    Console.WriteLine($"手动工具失败: {ex.Message}");
}

Console.ReadLine();

#pragma warning disable MAAI001
public class UnitConverterSkill : AgentClassSkill<UnitConverterSkill>
#pragma warning restore MAAI001
{
    /// <inheritdoc/>
#pragma warning disable MAAI001
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
#pragma warning restore MAAI001
        "unit-converter",
        "使用乘法因子在常见单位之间进行转换。当被要求在英里、公里、磅或千克之间换算时使用。");

    /// <inheritdoc/>
    protected override string Instructions => """
        当用户请求进行单位换算时，使用此技能。

        1. 查看 conversion-table 资源，找到目标换算所需的因子。
        2. 使用 convert 脚本，并传入数值和表中的因子。
        3. 清晰地给出结果，并同时标明两种单位。
        """;

    /// <summary>
    /// 获取用于脚本和资源参数及返回值编组的 <see cref="JsonSerializerOptions"/>。
    /// </summary>
    protected override JsonSerializerOptions? SerializerOptions => null;

    /// <summary>
    /// 一个提供乘法因子的换算表资源。
    /// </summary>
#pragma warning disable MAAI001
    [AgentSkillResource("conversion-table")]
#pragma warning restore MAAI001
    [Description("常见单位换算的乘法因子查询表。")]
    public string ConversionTable => """
        # 换算表

        公式：**result = value × factor**

        | 从          | 到          | 因子     |
        |-------------|-------------|----------|
        | 英里        | 公里        | 1.60934  |
        | 公里        | 英里        | 0.621371 |
        | 磅          | 千克        | 0.453592 |
        | 千克        | 磅          | 2.20462  |
        """;

    /// <summary>
    /// 按给定因子换算数值。
    /// </summary>
#pragma warning disable MAAI001
    [AgentSkillScript("convert")]
#pragma warning restore MAAI001
    [Description("将数值与换算因子相乘，并以 JSON 返回结果。")]
    private static string ConvertUnits(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}
