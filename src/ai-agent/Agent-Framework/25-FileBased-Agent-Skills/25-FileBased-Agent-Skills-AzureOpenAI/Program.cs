// Copyright (c) Microsoft. All rights reserved.

// 本示例演示如何在 ChatClientAgent 中使用基于文件的 Agent Skills。
// Skills 会从磁盘上的 SKILL.md 文件中自动发现，并遵循"渐进式加载（progressive disclosure）"的设计模式：
//
// 1. 广播（Advertise）—— 在系统提示中提供技能的名称和描述
// 2. 加载（Load）—— 在需要时通过 load_skill 工具加载完整的技能说明
// 3. 读取资源（Read resources）—— 通过 read_skill_resource 工具读取技能所依赖的参考文件
// 4. 执行脚本（Run scripts）—— 通过 run_skill_script 工具调用子进程执行脚本
//
// 本示例使用了一个单位转换技能，用于在英里、公里、磅和千克之间进行转换。
//
// 【注意事项】
// AgentSkillsProvider 在 Responses API 路径下能自动将技能工具注册到 ChatOptions.Tools，
// 但 Chat Completions API 路径下不支持此功能（工具的广告注入能正常工作，但工具本身
// 不会成为模型可调用的 function tool）。
// 本示例在 Chat API 路径下通过 AIFunctionFactory.Create() 手动从文件创建技能工具并注册。
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- Configuration ---
string endpoint = Environment.GetEnvironmentVariable("OPENAI_ENDPOINT") ?? throw new InvalidOperationException("OPENAI_ENDPOINT is not set.");
string apikey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");
string modelId = Environment.GetEnvironmentVariable("OPENAI_MODEL_NAME") ?? "gpt-5.4-mini";

// --- 从文件中读取技能定义 ---
string skillsDir = Path.Combine(AppContext.BaseDirectory, "skills", "unit-converter");

// 读取 SKILL.md
string skillMd = File.ReadAllText(Path.Combine(skillsDir, "SKILL.md"));

// 读取换算表
string conversionTableMd = File.ReadAllText(Path.Combine(skillsDir, "references", "conversion-table.md"));

// Python 脚本路径
string scriptPath = Path.Combine(skillsDir, "scripts", "convert.py");

Console.WriteLine($"已从文件加载技能: {Path.Combine(skillsDir, "SKILL.md")}");
Console.WriteLine($"已从文件加载换算表: {Path.Combine(skillsDir, "references", "conversion-table.md")}");
Console.WriteLine($"脚本路径: {scriptPath}");

// --- 基于文件创建技能工具 ---
// 工具名与 AgentSkillsProvider 的标准名称一致

// 1. load_skill: 返回 SKILL.md 的完整内容
var loadSkillTool = AIFunctionFactory.Create(
    () => skillMd,
    new AIFunctionFactoryOptions
    {
        Name = "load_skill",
        Description = "加载 unit-converter 技能的详细说明",
    });

// 2. read_skill_resource: 读取技能依赖的参考文件（如换算表）
var readResourceTool = AIFunctionFactory.Create(
    (string path) =>
    {
        // path 可能是 "references/conversion-table.md" 或 "conversion-table.md"
        string fileName = Path.GetFileName(path);
        string fullPath = Path.Combine(skillsDir, "references", fileName);
        return File.Exists(fullPath)
            ? File.ReadAllText(fullPath)
            : $"错误: 找不到文件 '{path}'";
    },
    new AIFunctionFactoryOptions
    {
        Name = "read_skill_resource",
        Description = "读取技能所依赖的参考文件（如换算表），传入文件路径",
    });

// 3. run_skill_script: 执行 Python 脚本进行单位换算
var runScriptTool = AIFunctionFactory.Create(
    async (double value, double factor) =>
    {
        Console.WriteLine($"=== 脚本执行开始 ===");
        Console.WriteLine($"value: {value}, factor: {factor}");
        var psi = new ProcessStartInfo("python")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(scriptPath);
        psi.ArgumentList.Add("--value");
        psi.ArgumentList.Add(value.ToString());
        psi.ArgumentList.Add("--factor");
        psi.ArgumentList.Add(factor.ToString());

        using var process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync();
        string error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine("STDERR: " + error);
        }
        Console.WriteLine("转换结果: " + output);
        return output.Trim();
    },
    new AIFunctionFactoryOptions
    {
        Name = "run_skill_script",
        Description = "执行 convert.py 脚本进行单位转换，传入 value（数值）和 factor（换算系数）",
    });

// --- 创建 AI Agent ---
var openAIClient = new OpenAIClient(
    new ApiKeyCredential(apikey),
    new OpenAIClientOptions { Endpoint = new Uri(endpoint) });

var agent = openAIClient
    .GetChatClient(modelId)
    .AsAIAgent(
        new ChatClientAgentOptions
        {
            Name = "UnitConverterAgent",
            ChatOptions = new()
            {
                Instructions = $"""
                    你是一个可以调用工具进行单位转换的助手。

                    可用技能：unit-converter（在英里/公里、磅/千克之间进行单位转换）

                    技能定义（来自 skills/unit-converter/SKILL.md）：
                    ---
                    {skillMd}
                    ---

                    换算表（来自 skills/unit-converter/references/conversion-table.md）：
                    ---
                    {conversionTableMd}
                    ---

                    请严格按以下流程操作：
                    1. 调用 load_skill 加载技能说明
                    2. 调用 read_skill_resource 读取换算表获取换算系数
                    3. 调用 run_skill_script 执行 python 脚本进行换算，传入 value 和 factor
                    4. 直接给出转换结果，并同时标明原单位和目标单位
                    """,
                Tools = [loadSkillTool, readResourceTool, runScriptTool],
            },
        });
/*
 // --- 自动审批所有技能工具的调用 加不加都一样---
    .AsBuilder()
    .UseToolApproval(new ToolApprovalAgentOptions
    {
        // 自动审批所有技能工具的调用
        AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
    })
    .Build()
 */

Console.WriteLine(new string('-', 60));
Console.WriteLine("正在使用基于文件的技能进行单位转换");
Console.WriteLine(new string('-', 60));

var stringBuilder = new StringBuilder();
await foreach (var response in agent.RunStreamingAsync("请严格用脚本计算。马拉松（26.2 英里）等于多少公里？另外，75 千克等于多少磅？"))
{
    Console.Write(response.Text);
    stringBuilder.Append(response.Text);
}

Console.WriteLine();
Console.WriteLine(new string('-', 60));
Console.WriteLine(stringBuilder.ToString());
Console.ReadLine();
