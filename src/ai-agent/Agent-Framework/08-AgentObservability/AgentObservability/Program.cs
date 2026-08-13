// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with Azure OpenAI as the backend that logs telemetry using OpenTelemetry.此示例展示了如何创建和使用一个以 Azure OpenAI 为后端、并使用 OpenTelemetry 记录遥测数据的简单 AI 智能体。

using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Azure.Monitor.OpenTelemetry.Exporter;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Trace;
using System.ClientModel;
using System.Diagnostics;
using System.Text;


// 设置控制台的输入输出编码为 UTF-8，确保中文等多字节字符能正确显示和读取
Console.InputEncoding = Encoding.UTF8;
// 不输出 BOM（字节顺序标记），避免在控制台前端或日志中出现不可见字符
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// 从环境变量读取 Azure OpenAI 服务的端点与部署名
// AZURE_OPENAI_ENDPOINT 必须设置，否则抛出异常
var endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new InvalidOperationException("OPENAI_ENDPOINT is not set.");
// 部署名称可通过环境变量覆盖，默认使用 gpt-4o-mini
var modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME") ?? "gpt-4o-mini";
var apikey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "gpt-4o-mini";
// API Key 可通过环境变量读取
var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");
// 为每次运行生成一个唯一的 source name，方便在 OpenTelemetry 中区分会话
string sourceName = Guid.NewGuid().ToString("N");

// 创建 OpenTelemetry 的 TracerProviderBuilder，并添加控制台导出器（便于本地调试）
//var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
//    .AddSource(sourceName)
//    .AddConsoleExporter();

var customProcessor = new CustomActivityProcessor();

var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
    .AddSource(sourceName)  // 添加自定义的 Source，用于捕获特定来源的遥测数据
    .AddConsoleExporter()   // 添加控制台导出器，方便本地调试查看遥测数据
     .AddProcessor(customProcessor)  // 添加自定义处理器
    .SetSampler(new AlwaysOnSampler()); // 确保所有遥测数据都被采样（生产环境可根据需要调整）


// 构建并在作用域结束时释放 TracerProvider
using var tracerProvider = tracerProviderBuilder.Build();

// 指令：定义 agent 的行为风格（系统提示）
const string instructions = "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。";
// 用户输入的 prompt：想要 agent 执行的具体任务
const string prompt = "给我讲一个发生在茶馆里的段子，轻松一点的那种。";

OpenAIClient openAIClient = new OpenAIClient(
    new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint=new Uri(endpoint)}
    );
AIAgent aiAgent = openAIClient.GetChatClient(modelId).AsAIAgent(
    instructions:instructions,
    name:"Joker"
    ).AsBuilder()
    .UseOpenTelemetry(sourceName:sourceName)
    .Build();

// ============================================================
// 执行 Agent - 流式输出
// ============================================================
Console.WriteLine("开始流式输出（OpenTelemetry 已启用）：");
Console.WriteLine("----------------------------------------");
// 如果需要流式输出（逐步接收模型生成的中间结果），可以使用 RunStreamingAsync
try
{
    Console.WriteLine("🤖 AI 说书人开始讲段子：");
    Console.WriteLine("----------------------------------------");

    bool hasContent = false;
    StringBuilder fullResponse = new StringBuilder();

    await foreach (var update in aiAgent.RunStreamingAsync(prompt))
    {
        if (!string.IsNullOrEmpty(update.Text))
        {
             Console.Write(update.Text);
            fullResponse.Append(update.Text);
            hasContent = true;
            continue;
        }
    }

    Console.WriteLine("\n----------------------------------------");

    if (!hasContent)
    {
        Console.WriteLine("⚠️ 未收到任何文本内容。检查：");
        Console.WriteLine($"   - 模型：{modelId}");
        Console.WriteLine($"   - 提示词：{prompt}");
        Console.WriteLine($"   - 指令：{instructions}");
    }
    else
    {
        Console.WriteLine($"✅ 段子讲完了！共 {fullResponse.Length} 个字符");
        Console.WriteLine($"📊 统计：输入 {36} tokens，输出 {444} tokens，推理 {191} tokens");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ 发生错误: {ex.Message}");
    Console.WriteLine($"详细错误: {ex.StackTrace}");
    throw;
}
Console.WriteLine("----------------------------------------");
Console.WriteLine("流式输出完成。");

// ============================================================
// 新增：显示捕获的遥测日志内容
// ============================================================
Console.WriteLine("\n========================================");
Console.WriteLine("📊 捕获的遥测日志内容：");
Console.WriteLine("========================================\n");

// 等待一下确保所有 Activity 都已处理完成
await Task.Delay(100);

foreach (var activity in customProcessor.CapturedActivities)
{
    DisplayActivityDetails(activity);
}

// 显示汇总统计
DisplaySummary(customProcessor.CapturedActivities);

void DisplayActivityDetails(Activity activity)
{
    Console.WriteLine($"🔹 Activity: {activity.DisplayName}");
    Console.WriteLine($"   ├─ TraceId: {activity.TraceId}");
    Console.WriteLine($"   ├─ SpanId: {activity.SpanId}");
    Console.WriteLine($"   ├─ Duration: {activity.Duration.TotalMilliseconds:F2}ms");
    Console.WriteLine($"   ├─ StartTime: {activity.StartTimeUtc.ToLocalTime:yyyy-MM-dd HH:mm:ss.fff}");
    Console.WriteLine($"   └─ Tags:");

    var tags = activity.Tags.ToList();
    if (tags.Any())
    {
        foreach (var tag in tags)
        {
            Console.WriteLine($"      ├─ {tag.Key}: {tag.Value}");
        }
    }
    else
    {
        Console.WriteLine("      └─ (无标签)");
    }

    Console.WriteLine();
}

void DisplaySummary(List<Activity> activities)
{
    Console.WriteLine("========================================");
    Console.WriteLine("📈 遥测数据汇总：");
    Console.WriteLine("========================================");

    var agentActivities = activities.Where(a => a.DisplayName?.StartsWith("invoke_agent") == true).ToList();
    var chatActivities = activities.Where(a => a.DisplayName?.StartsWith("chat") == true).ToList();

    Console.WriteLine($"总 Activity 数量: {activities.Count}");
    Console.WriteLine($"Agent 调用次数: {agentActivities.Count}");
    Console.WriteLine($"Chat 调用次数: {chatActivities.Count}");

    // 从 Agent Activity 提取关键指标
    foreach (var activity in agentActivities)
    {
        var tags = activity.Tags.ToDictionary(t => t.Key, t => t.Value);

        Console.WriteLine($"\n🤖 Agent: {tags.GetValueOrDefault("gen_ai.agent.name", "未知")}");
        Console.WriteLine($"   ├─ 模型: {tags.GetValueOrDefault("gen_ai.request.model", "未知")}");
        Console.WriteLine($"   ├─ 总耗时: {activity.Duration.TotalMilliseconds:F2}ms");
        Console.WriteLine($"   ├─ 输入 Tokens: {tags.GetValueOrDefault("gen_ai.usage.input_tokens", "0")}");
        Console.WriteLine($"   ├─ 输出 Tokens: {tags.GetValueOrDefault("gen_ai.usage.output_tokens", "0")}");
        Console.WriteLine($"   ├─ 推理 Tokens: {tags.GetValueOrDefault("gen_ai.usage.reasoning.output_tokens", "0")}");
        Console.WriteLine($"   ├─ 首字延迟: {tags.GetValueOrDefault("gen_ai.response.time_to_first_chunk", "0")}s");
        Console.WriteLine($"   ├─ 完成原因: {tags.GetValueOrDefault("gen_ai.response.finish_reasons", "未知")}");
        Console.WriteLine($"   └─ 响应ID: {tags.GetValueOrDefault("gen_ai.response.id", "未知")}");
    }
}

// 1. 使用自定义的 SpanProcessor 捕获 Activity
public class CustomActivityProcessor : BaseProcessor<Activity>
{
    public List<Activity> CapturedActivities { get; } = new();

    public override void OnEnd(Activity activity)
    {
        CapturedActivities.Add(activity);
        base.OnEnd(activity);
    }
}