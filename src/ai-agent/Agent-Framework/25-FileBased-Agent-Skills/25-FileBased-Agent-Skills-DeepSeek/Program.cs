// Copyright (c) Microsoft. All rights reserved.
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- DeepSeek configuration ---
var apiKey = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY") ?? throw new InvalidOperationException("DEEPSEEK_API_KEY 未设置。");
var endpoint = Environment.GetEnvironmentVariable("DEEPSEEK_ENDPOINT") ?? "https://api.deepseek.com/v1";
var model = Environment.GetEnvironmentVariable("DEEPSEEK_MODEL")?? "deepseek-v4-pro";
// --- File-based Skills provider ---
string skillsPath = Path.Combine(AppContext.BaseDirectory, "skills");
if (!Directory.Exists(skillsPath))
{
    throw new DirectoryNotFoundException($"Skills directory was not copied to the output: {skillsPath}");
}
var skillsProvider = new AgentSkillsProvider(
    skillsPath,
    RunAsync);

var chatClient = new OpenAIClient(
        new ApiKeyCredential(apiKey),
        new OpenAIClientOptions { Endpoint = new Uri(endpoint) })
    .GetChatClient(model)
    .AsIChatClient();

#pragma warning disable MAAI001
AIAgent agent = chatClient
    .AsAIAgent(new ChatClientAgentOptions
    {

        Name = "DeepSeekUnitConverterAgent",
        ChatOptions = new()
        {
            Temperature = 0,
            Instructions = "你是一个可以进行单位转换的助手。",
        },
        AIContextProviders = [skillsProvider],
    })
    .AsBuilder()
    .UseToolApproval(new ToolApprovalAgentOptions
    {
        // 仅用于演示。生产环境应在运行 Skill 脚本之前请求用户授权。
        AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
    })
    .Build();
#pragma warning restore MAAI001

Console.WriteLine($"正在使用 DeepSeek 模型 '{model}' 调用基于文件的 Skill");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync("对如下单位进行转换：马拉松（26.2 英里）等于多少公里？另外，75 千克等于多少磅？");


Console.WriteLine($"Agent: {response.Text}");

Console.ReadLine();

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
        ".py" => OperatingSystem.IsWindows() ? "python" : "python3",
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

    if (interpreter is not null)
    {
        startInfo.FileName = interpreter;
        startInfo.ArgumentList.Add(script.FullPath);
    }
    else
    {
        startInfo.FileName = script.FullPath;
    }

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
    else if (arguments is { ValueKind: JsonValueKind.String } commandLine)
    {
        foreach (string argument in TokenizeCommandLine(commandLine.GetString()!))
        {
            startInfo.ArgumentList.Add(argument);
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
            return $"错误: 无法启动脚本 '{script.Name}'。";
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
        process?.Kill(entireProcessTree: true);
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

static IReadOnlyList<string> TokenizeCommandLine(string commandLine)
{
    var arguments = new List<string>();
    var current = new StringBuilder();
    char? quote = null;
    bool tokenStarted = false;

    for (int index = 0; index < commandLine.Length; index++)
    {
        char character = commandLine[index];

        if (character == '\\' &&
            index + 1 < commandLine.Length &&
            commandLine[index + 1] is '\'' or '"')
        {
            current.Append(commandLine[++index]);
            tokenStarted = true;
        }
        else if (character is '\'' or '"')
        {
            if (quote is null)
            {
                quote = character;
                tokenStarted = true;
            }
            else if (quote == character)
            {
                quote = null;
            }
            else
            {
                current.Append(character);
            }
        }
        else if (char.IsWhiteSpace(character) && quote is null)
        {
            if (tokenStarted)
            {
                arguments.Add(current.ToString());
                current.Clear();
                tokenStarted = false;
            }
        }
        else
        {
            current.Append(character);
            tokenStarted = true;
        }
    }

    if (quote is not null)
    {
        throw new InvalidOperationException("脚本参数包含未闭合的引号。");
    }

    if (tokenStarted)
    {
        arguments.Add(current.ToString());
    }

    return arguments;
}
