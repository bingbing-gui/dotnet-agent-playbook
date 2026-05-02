// Copyright (c) Microsoft. 保留所有权利。

// 此示例演示如何将依赖注入 (DI) 与 Agent Skills 结合使用。
// 它并排展示两种方法，每种方法处理不同的转换领域：
//
// 1. 代码定义的技能 (AgentInlineSkill) — 转换距离（英里 ↔ 千米）。
//    资源和脚本是内联委托，会从 IServiceProvider 解析服务。
//
// 2. 基于类的技能 (AgentClassSkill) — 转换重量（磅 ↔ 千克）。
//    资源和脚本封装在类中，同样会从 IServiceProvider 解析服务。
//
// 这两个技能共享同一个注册到 DI 容器中的 ConversionService，
// 这表明无论技能如何定义，DI 的工作方式都是一致的。
// 当提示中包含跨越两个领域的问题时，智能体会同时使用这两个技能。

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Responses;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);


// --- 配置 ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5.4-mini";
// --- DI 容器 ---
// 注册应用程序服务，以便技能资源和脚本在执行时可以解析这些服务。
ServiceCollection services = new();
services.AddSingleton<ConversionService>();

IServiceProvider serviceProvider = services.BuildServiceProvider();

// =====================================================================
// 方法 1：使用 DI 的代码定义技能 (AgentInlineSkill)
// =====================================================================
// 处理距离转换（英里 ↔ 千米）。
// 资源和脚本是内联委托。每个委托都可以声明一个 IServiceProvider 参数，
// 框架会自动注入该参数。

#pragma warning disable MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
var distanceSkill = new AgentInlineSkill(
    name: "distance-converter",
    description: "在距离单位之间转换。当要求将英里转换为千米或将千米转换为英里时使用。",
    instructions: """
        当用户要求在距离单位（英里和千米）之间转换时，请使用此技能。

        1. 查看 distance-table 资源，找到所请求转换的系数。
        2. 使用 convert 脚本，传入值和表中的系数。
        """)
    .AddResource("distance-table", (IServiceProvider serviceProvider) =>
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.GetDistanceTable();
    })
    .AddScript("convert", (double value, double factor, IServiceProvider serviceProvider) =>
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.Convert(value, factor);
    });
#pragma warning restore MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。

// =====================================================================
// 方法 2：使用 DI 的基于类的技能 (AgentClassSkill)
// =====================================================================
// 处理重量转换（磅 ↔ 千克）。
// 资源和脚本通过特性使用反射发现。
// 带有 IServiceProvider 参数的方法会自动接收 DI。
//
// 或者，基于类的技能也可以通过构造函数接收依赖项。
// 将技能类本身注册到 ServiceCollection 中，
// 然后从容器中解析它：
//
//   services.AddSingleton<WeightConverterSkill>();
//   var weightSkill = serviceProvider.GetRequiredService<WeightConverterSkill>();

var weightSkill = new WeightConverterSkill();
// --- 技能提供程序 ---
// 这两个技能注册到同一个提供程序中，因此智能体可以使用其中任意一个。
#pragma warning disable MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
var skillsProvider = new AgentSkillsProvider(distanceSkill, weightSkill);
#pragma warning restore MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。

// --- 智能体设置 ---
#pragma warning disable OPENAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(
        options: new ChatClientAgentOptions
        {
            Name = "UnitConverterAgent",
            ChatOptions = new()
            {
                Instructions = "你是一个可以转换单位的实用助手。",
            },
            AIContextProviders = [skillsProvider],
        },
        model: deploymentName,
        services: serviceProvider);
#pragma warning restore OPENAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。

// --- 示例：单位转换 ---
// 此提示跨越两个领域，因此智能体会使用这两个技能。
Console.WriteLine("使用 DI 驱动的技能转换单位");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync(
    "一场马拉松（26.2 英里）是多少千米？75 千克是多少磅？");

Console.WriteLine($"智能体：{response.Text}");

Console.ReadLine();

// ---------------------------------------------------------------------------
// 基于类的技能
// ---------------------------------------------------------------------------

/// <summary>
/// 一个定义为 C# 类并使用依赖注入的重量转换技能。
/// </summary>
/// <remarks>
/// 此技能会在其资源方法和脚本方法中从 DI 容器解析 <see cref="ConversionService"/>。
/// 带有 <see cref="IServiceProvider"/> 参数的方法会由框架自动注入。
/// 使用 <see cref="AgentSkillResourceAttribute"/> 和 <see cref="AgentSkillScriptAttribute"/>
/// 标注的属性和方法会通过反射自动发现。
/// </remarks>
#pragma warning disable MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
internal sealed class WeightConverterSkill : AgentClassSkill<WeightConverterSkill>
#pragma warning restore MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
{
    /// <inheritdoc/>
#pragma warning disable MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
#pragma warning restore MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
        "weight-converter",
        "在重量单位之间转换。当要求将磅转换为千克或将千克转换为磅时使用。");

    /// <inheritdoc/>
    protected override string Instructions => """
        当用户要求在重量单位（磅和千克）之间转换时，请使用此技能。

        1. 查看 weight-table 资源，找到所请求转换的系数。
        2. 使用 convert 脚本，传入值和表中的系数。
        3. 使用两个单位清晰呈现结果。
        """;

    /// <summary>
    /// 从 DI 注册的 <see cref="ConversionService"/> 返回重量转换表。
    /// </summary>
#pragma warning disable MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
    [AgentSkillResource("weight-table")]
#pragma warning restore MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
    [Description("重量转换乘法系数的查找表。")]
    private static string GetWeightTable(IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.GetWeightTable();
    }

    /// <summary>
    /// 使用 DI 注册的 <see cref="ConversionService"/> 按给定系数转换值。
    /// </summary>
#pragma warning disable MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
    [AgentSkillScript("convert")]
#pragma warning restore MAAI001 // 类型仅用于评估目的，未来更新中可能会更改或删除。禁止显示此诊断以继续。
    [Description("将值乘以转换系数，并以 JSON 形式返回结果。")]
    private static string Convert(double value, double factor, IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetRequiredService<ConversionService>();
        return service.Convert(value, factor);
    }
}

// ---------------------------------------------------------------------------
// 服务
// ---------------------------------------------------------------------------

/// <summary>
/// 提供单位之间的转换率。
/// 在实际应用程序中，这可以调用外部 API、从数据库读取，
/// 或应用随时间变化的汇率。
/// </summary>
internal sealed class ConversionService
{
    /// <summary>
    /// 返回受支持距离转换的 Markdown 表。
    /// </summary>
    public string GetDistanceTable() =>
        """
        # 距离转换

        公式：**结果 = 值 × 系数**

        | 从          | 到          | 系数     |
        |-------------|-------------|----------|
        | 英里        | 千米        | 1.60934  |
        | 千米        | 英里        | 0.621371 |
        """;
    /// <summary>
    /// 返回受支持重量转换的 Markdown 表。
    /// </summary>
    public string GetWeightTable() =>
        """
        # 重量转换

        公式：**结果 = 值 × 系数**

        | 从          | 到          | 系数     |
        |-------------|-------------|----------|
        | 磅          | 千克        | 0.453592 |
        | 千克        | 磅          | 2.20462  |
        """;
    /// <summary>
    /// 按给定系数转换值，并返回 JSON 结果。
    /// </summary>
    public string Convert(double value, double factor)
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    }
}