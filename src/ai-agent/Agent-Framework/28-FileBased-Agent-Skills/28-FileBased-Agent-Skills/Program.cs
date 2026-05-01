// Copyright (c) Microsoft. All rights reserved.

// 本示例演示如何在 ChatClientAgent 中使用基于文件的 Agent Skills。
// Skills 会从磁盘上的 SKILL.md 文件中自动发现，并遵循“渐进式加载（progressive disclosure）”的设计模式：
//
// 1. 广播（Advertise）—— 在系统提示中提供技能的名称和描述
// 2. 加载（Load）—— 在需要时通过 load_skill 工具加载完整的技能说明
// 3. 读取资源（Read resources）—— 通过 read_skill_resource 工具读取技能所依赖的参考文件
// 4. 执行脚本（Run scripts）—— 通过 run_skill_script 工具调用子进程执行脚本
//
// 本示例使用了一个单位转换技能，用于在英里、公里、磅和千克之间进行转换。
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using OpenAI.Responses;
using System.Diagnostics;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- Configuration ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5.4-mini";


// --- Skills Provider ---
// 从 'skills' 目录中发现技能，这些技能以 SKILL.md 文件的形式存在。
// 脚本执行器会以本地子进程的方式运行基于文件的脚本（例如 Python）。
#pragma warning disable MAAI001
AgentFileSkillScriptRunner myRunner = async (skill, script, args, ct) =>
{

    try
    {
        Console.WriteLine($"运行脚本: {script.Name}");
        // 你在这个方法里面：
        // ✔ 调 Python
        // ✔ 调 HTTP API
        // ✔ 调数据库
        // ✔ 调你自己的 C# 方法
        var psi = new ProcessStartInfo("python")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.ArgumentList.Add(Path.Combine(skill.Path, script.FullPath));
        if (args != null)
        {
            foreach (var (key, value) in args)
            {
                if (value is not null && !string.IsNullOrWhiteSpace(value.ToString()))
                {
                    psi.ArgumentList.Add(key);
                    psi.ArgumentList.Add(value.ToString()!);
                }
            }
        }
        using var process = Process.Start(psi)!;
        string output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        Console.WriteLine("STDOUT: " + output);
        //Console.WriteLine("STDERR: " + error);
        await process.WaitForExitAsync();

        return output.Trim();
    }
    catch (Exception ex)
    {
        throw;
    }
};

var skillsProvider = new AgentSkillsProvider(Path.Combine(AppContext.BaseDirectory, "skills"), myRunner);

#pragma warning restore MAAI001
// --- Agent Setup ---
#pragma warning disable OPENAI001 
AIAgent agent = new AzureOpenAIClient(new Uri(endpoint),
    new AzureCliCredential())
    .GetResponsesClient()
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个可以调用工具进行单位转换的助手。",
        },

        AIContextProviders = [skillsProvider],
    },
    model: deploymentName);

#pragma warning restore OPENAI001

Console.WriteLine("正在使用基于文件的技能进行单位转换");

Console.WriteLine(new string('-', 60));

var stringBuilder = new StringBuilder();
await foreach (var response in agent.RunStreamingAsync("请严格用脚本计算。马拉松（26.2 英里）等于多少公里？另外，75 千克等于多少磅？"))
{
    stringBuilder.Append(response.Text);
}
Console.WriteLine(stringBuilder.ToString());
Console.ReadLine();


