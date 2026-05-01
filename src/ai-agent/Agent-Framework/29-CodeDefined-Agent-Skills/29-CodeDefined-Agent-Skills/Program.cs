// Copyright (c) Microsoft. All rights reserved.

// 此示例演示如何使用 AgentInlineSkill 完全通过代码定义 Agent Skills。
// 无需 SKILL.md 文件——技能、资源和脚本都以编程方式定义。
//
// 这里使用单位转换技能展示三种方式：
// 1. 静态资源——通过 AddResource 提供的内联内容
// 2. 动态资源——通过工厂委托在运行时计算
// 3. 代码脚本——可由代理直接调用的可执行委托

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Responses;
using System.Text;
using System.Text.Json;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- 配置 ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")
    ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT not set.");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME")
    ?? throw new InvalidOperationException("AZURE_OPENAI_DEPLOYMENT_NAME not set.");

// --- 构建代码定义的技能 ---
#pragma warning disable MAAI001 
var unitConverterSkill = new AgentInlineSkill(
    name: "unit-converter",
    description: "使用乘法系数在常见单位之间进行转换。当被要求在英里、公里、磅或千克之间转换时使用此技能。",
    instructions: """
        当用户要求进行单位转换时，使用此技能。

        1. 查看 conversion-table 资源，找到所请求转换对应的系数。
        2. 查看 conversion-policy 资源，确认舍入和格式化规则。
        3. 使用 convert 脚本，并传入表格中的数值和系数。
        """)
    // 1. 静态资源：转换表
    .AddResource(
        "conversion-table",
        """
        # 转换表

        公式：**result = value × factor**

        | 从          | 到          | 系数     |
        |-------------|-------------|----------|
        | 英里        | 公里        | 1.60934  |
        | 公里        | 英里        | 0.621371 |
        | 磅          | 千克        | 0.453592 |
        | 千克        | 磅          | 2.20462  |
        """)
    // 2. 动态资源：转换策略（运行时计算）
    .AddResource("conversion-policy", () =>
    {
        const int Precision = 4;
        return $"""
            # 转换策略
            **小数位数：** {Precision}
            **格式：** 始终显示带单位的原始值和转换后值
            **生成时间：** {DateTime.UtcNow:O}
            """;
    })
    // 3. 代码脚本：convert
    .AddScript("convert", (double value, double factor) =>
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    });
#pragma warning restore MAAI001
// --- 技能提供程序 ---
#pragma warning disable MAAI001 
var skillsProvider = new AgentSkillsProvider(unitConverterSkill);
#pragma warning restore MAAI001

// --- 代理设置 ---
#pragma warning disable OPENAI001 
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个乐于助人的助手，能够进行单位转换。",
        },
        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);
#pragma warning restore OPENAI001 

// --- 示例：单位转换 ---
Console.WriteLine("使用代码定义的技能进行单位转换");
Console.WriteLine(new string('-', 60));

var stringBuilder = new StringBuilder();
await foreach (var response in agent.RunStreamingAsync("请严格用脚本计算。马拉松（26.2 英里）等于多少公里？另外，75 千克等于多少磅？"))
{
    stringBuilder.Append(response.Text);
}
Console.WriteLine(stringBuilder.ToString());
Console.ReadLine();

