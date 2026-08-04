// Copyright (c) Microsoft. All rights reserved.
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- Configuration ---
string endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-5.4-mini";



var skillsProvider = new AgentSkillsProvider(
    Path.Combine(AppContext.BaseDirectory, "skills"),
    RunAsync);
#pragma warning disable MAAI001
AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "UnitConverterAgent",
        ChatOptions = new()
        {
            ModelId = deploymentName,
            Instructions = "你是一个可以调用工具进行单位转换的助手。",
        },
        AIContextProviders = [skillsProvider],
    })
    .AsBuilder()
    .UseToolApproval(new ToolApprovalAgentOptions
    {
        // 注意：为了简化演示，本示例会自动批准所有 Skill 工具的调用。
        // 在实际生产环境中，应在执行脚本之前先向用户请求授权。
         AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
    })
    .Build();

// --- Example: Unit conversion ---
Console.WriteLine("正在使用基于文件的技能进行单位转换");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync(
    "将26.2 英里转换成公里。将75千克转换成磅。");

Console.WriteLine($"Agent: {response.Text}");

static async Task<object?> RunAsync(
        AgentFileSkill skill,
        AgentFileSkillScript script,
        JsonElement? arguments,
        IServiceProvider? serviceProvider,
        CancellationToken cancellationToken)
{
    if (!File.Exists(script.FullPath))
    {
        return $"错误: 找不到脚本文件: {script.FullPath}";
    }

    string extension = Path.GetExtension(script.FullPath);
    string? interpreter = extension switch
    {
        ".py" => "python3",
        ".js" => "node",
        ".sh" => "bash",
        ".ps1" => "pwsh",
        _ => null,
    };

    var startInfo = new ProcessStartInfo
    {
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        UseShellExecute = false,
        CreateNoWindow = true,
        WorkingDirectory = Path.GetDirectoryName(script.FullPath) ?? ".",
    };

    if (arguments is { ValueKind: JsonValueKind.Array } jsonArray)
    {
        foreach (JsonElement element in jsonArray.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    $"数组参数只能包含字符串，收到：{element.ValueKind}");
            }

            startInfo.ArgumentList.Add(element.GetString()!);
        }
    }
    else if (arguments is { ValueKind: JsonValueKind.Object } jsonObject)
    {
        if (!jsonObject.TryGetProperty("value", out JsonElement value) ||
            !jsonObject.TryGetProperty("factor", out JsonElement factor))
        {
            throw new InvalidOperationException(
                $"脚本参数必须包含 value 和 factor。实际参数：{jsonObject}");
        }

        startInfo.ArgumentList.Add("--value");
        startInfo.ArgumentList.Add(value.ToString());

        startInfo.ArgumentList.Add("--factor");
        startInfo.ArgumentList.Add(factor.ToString());
    }
    else if (arguments is not null &&
         arguments.Value.ValueKind is not JsonValueKind.Null
             and not JsonValueKind.Undefined)
    {
        throw new InvalidOperationException(
            $"不支持的脚本参数类型：{arguments.Value.ValueKind}");
    }

    Process? process = null;
    try
    {
        process = Process.Start(startInfo);
        if (process is null)
        {
            return $"错误: 无法启动脚本 '{script.Name}' 的进程。";
        }

        Task<string> outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        Task<string> errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        string output = await outputTask.ConfigureAwait(false);
        string error = await errorTask.ConfigureAwait(false);

        if (!string.IsNullOrEmpty(error))
        {
            output += $"\n标准错误输出:\n{error}";
        }

        if (process.ExitCode != 0)
        {
            output += $"\n脚本以代码 {process.ExitCode} 退出";
        }

        return string.IsNullOrEmpty(output) ? "(无输出)" : output.Trim();
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
        // Kill the process on cancellation to avoid leaving orphaned subprocesses.
        process?.Kill(entireProcessTree: true);
        throw;
    }
    catch (OperationCanceledException)
    {
        throw;
    }
    catch (Exception ex)
    {
        return $"错误: 执行脚本 '{script.Name}' 失败: {ex.Message}";
    }
    finally
    {
        process?.Dispose();
    }
}


