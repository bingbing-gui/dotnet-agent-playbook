// Copyright (c) Microsoft. All rights reserved.

// 本示例演示如何使用 AgentClassSkill 将 Agent 技能定义为 C# 类，
// 并通过特性自动发现脚本和资源。

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Responses;
using System.ComponentModel;
using System.Text;
using System.Text.Json;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- 配置 ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT 未设置。");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5.4-mini";

// --- 基于类的技能 ---
// 实例化技能类。
var unitConverter = new UnitConverterSkill();

// --- 技能提供程序 ---
#pragma warning disable MAAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。
var skillsProvider = new AgentSkillsProvider(unitConverter);
#pragma warning restore MAAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。

// --- Agent 设置 ---
#pragma warning disable OPENAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个可以进行单位换算的助手。",
        },
        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);
#pragma warning restore OPENAI001 // 此类型仅用于评估，未来更新中可能更改或移除。抑制此诊断以继续。

// --- 示例：单位换算 ---
Console.WriteLine("使用基于类的技能进行单位换算");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync(
    "马拉松（26.2 英里）是多少公里？75 千克又是多少磅？");

Console.WriteLine($"助手：{response.Text}");

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
    /// <remarks>
    /// 在本示例中不一定需要重写该属性，但可用于提供自定义序列化选项，
    /// 例如为 Native AOT 兼容性提供源生成的 <c>JsonTypeInfoResolver</c>。
    /// </remarks>
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