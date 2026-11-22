// 版权所有 (c) Microsoft。保留所有权利。

// 此示例演示了一个高级场景：在单个智能体中使用 AgentSkillsProviderBuilder
// 组合多种技能类型。该构建器适用于简单的 AgentSkillsProvider
// 构造函数不足以满足需求的情况——例如你需要混合技能来源、应用筛选，
// 或在同一处配置跨领域选项。
//
// 这里注册了三种不同的技能来源：
// 1. 基于文件：unit-converter（英里↔公里、磅↔千克），来自磁盘上的 SKILL.md
// 2. 代码定义：volume-converter（加仑↔升），使用 AgentInlineSkill
// 3. 基于类：temperature-converter（°F↔°C↔K），使用带特性的 AgentClassSkill
//
// 对于更简单的单一来源场景，请参阅本示例系列前面的步骤
// （例如：Step01 对应基于文件，Step02 对应代码定义，Step03 对应基于类）。

using Microsoft.Agents.AI;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

// --- 1. 代码定义技能：volume-converter ---
#pragma warning disable MAAI001 // 该类型仅用于评估用途，未来更新中可能会变更或移除。抑制此诊断以继续。
var volumeConverterSkill = new AgentInlineSkill(
    name: "volume-converter",
    description: "使用乘法系数在加仑和升之间进行转换。",
    instructions: """
        当用户请求在加仑和升之间转换时，使用此技能。

        1. 查看 volume-conversion-table 资源，找到正确的系数。
        2. 使用 convert-volume 脚本，并传入数值与系数。
        """)
    .AddResource("volume-conversion-table",
        """
        # 体积换算表

        公式：**result = value × factor**

        | 从      | 到      | 系数    |
        |---------|---------|---------|
        | gallons | liters  | 3.78541 |
        | liters  | gallons | 0.264172|
        """)
    .AddScript("convert-volume", (double value, double factor) =>
    {
        double result = Math.Round(value * factor, 4);
        return JsonSerializer.Serialize(new { value, factor, result });
    });
#pragma warning restore MAAI001

// --- 2. 基于类的技能：temperature-converter ---
var temperatureConverter = new TemperatureConverterSkill();

// --- 3. 构建同时组合三种来源类型的提供程序 ---
#pragma warning disable MAAI001 // 该类型仅用于评估用途，未来更新中可能会变更或移除。抑制此诊断以继续。
var skillsProvider = new AgentSkillsProviderBuilder()
    .UseFileSkill(Path.Combine(AppContext.BaseDirectory, "skills"))    // 基于文件：unit-converter
    .UseSkill(volumeConverterSkill)                                    // 代码定义：volume-converter
    .UseSkill(temperatureConverter)                                    // 基于类：temperature-converter
    .UseFileScriptRunner(myRunnerAsync)
    .Build();
#pragma warning restore MAAI001 // 该类型仅用于评估用途，未来更新中可能会变更或移除。抑制此诊断以继续。
AIAgent agent = SkillAgentFactory.Create(new ChatClientAgentOptions
    {
        Name = "MultiConverterAgent",
        ChatOptions = new()
        {
            Instructions = "你是一个乐于助人的助手，可以进行单位、体积和温度转换。",
        },
        AIContextProviders = [skillsProvider],
    }, deepSeekDefaultModel: "deepseek-chat");
// --- 示例：使用三种技能 ---
Console.WriteLine("使用混合技能进行转换（文件 + 代码 + 类）");
Console.WriteLine(new string('-', 60));

AgentResponse response = await agent.RunAsync(
    "我需要三个换算：" +
    "1）马拉松（26.2 英里）是多少公里？" +
    "2）5 加仑桶是多少升？" +
    "3）98.6°F 换算成摄氏度是多少？");
Console.WriteLine($"智能体：{response.Text}");

Console.ReadLine();

#pragma warning disable MAAI001 
static async Task<object?> myRunnerAsync(
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

    if (arguments is { ValueKind: JsonValueKind.Array } json)
    {
        // Positional CLI arguments
        foreach (var element in json.EnumerateArray())
        {
            if (element.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    $"错误: 文件型技能脚本只接受字符串类型的命令行参数，但收到的 JSON 元素类型为 '{element.ValueKind}'。 " +
                    "所有数组元素必须是 JSON 字符串。");
            }
            startInfo.ArgumentList.Add(element.GetString()!);
        }
    }
    else if (arguments is { ValueKind: JsonValueKind.Object } jsonObject)
    {
        if (!jsonObject.TryGetProperty("value", out JsonElement value) ||
            !jsonObject.TryGetProperty("factor", out JsonElement factor))
        {
            throw new InvalidOperationException($"脚本参数必须包含 value 和 factor。实际参数：{jsonObject}");
        }

        startInfo.ArgumentList.Add("--value");
        startInfo.ArgumentList.Add(value.ToString());
        startInfo.ArgumentList.Add("--factor");
        startInfo.ArgumentList.Add(factor.ToString());
    }
    else if (arguments is not null && arguments.Value.ValueKind != JsonValueKind.Null && arguments.Value.ValueKind != JsonValueKind.Undefined)
    {
        throw new InvalidOperationException(
            $"错误: 预期一个 JSON 数组作为命令行参数，但收到的类型为 {arguments.Value.ValueKind}。 " +
            "文件型技能脚本期望位置参数为 JSON 字符串数组。");
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
/// <summary>
/// 一个使用特性进行发现、以 C# 类定义的 temperature-converter 技能。
/// </summary>
/// <remarks>
/// 使用 <see cref="AgentSkillResourceAttribute"/> 标注的属性会被自动发现为技能资源，
/// 使用 <see cref="AgentSkillScriptAttribute"/> 标注的方法会被自动发现为技能脚本。
/// </remarks>
#pragma warning disable MAAI001 
internal sealed class TemperatureConverterSkill : AgentClassSkill<TemperatureConverterSkill>
#pragma warning restore MAAI001
{
    /// <inheritdoc/>
#pragma warning disable MAAI001
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
#pragma warning restore MAAI001
        "temperature-converter",
        "在不同温标之间进行转换（华氏、摄氏、开尔文）。");

    /// <inheritdoc/>
    protected override string Instructions => """
        当用户请求进行温度换算时，使用此技能。

        1. 查看 temperature-conversion-formulas 资源，找到正确的公式。
        2. 使用 convert-temperature 脚本，并传入数值、源温标和目标温标。
        3. 清晰展示换算结果，并标注两种温标。
        """;

    /// <summary>
    /// 温度换算公式参考表。
    /// </summary>
#pragma warning disable MAAI001
    [AgentSkillResource("temperature-conversion-formulas")]
#pragma warning restore MAAI001 
    [Description("华氏、摄氏与开尔文之间的换算公式。")]
    public string ConversionFormulas => """
        # 温度换算公式

        | 从          | 到          | 公式                       |
        |-------------|-------------|---------------------------|
        | Fahrenheit  | Celsius     | °C = (°F − 32) × 5/9     |
        | Celsius     | Fahrenheit  | °F = (°C × 9/5) + 32     |
        | Celsius     | Kelvin      | K = °C + 273.15          |
        | Kelvin      | Celsius     | °C = K − 273.15          |
        """;

    /// <summary>
    /// 在不同温标之间转换温度数值。
    /// </summary>
#pragma warning disable MAAI001
    [AgentSkillScript("convert-temperature")]
#pragma warning restore MAAI001
    [Description("将温度值从一种温标转换到另一种温标。")]
    private static string ConvertTemperature(double value, string from, string to)
    {
        double result = (from.ToUpperInvariant(), to.ToUpperInvariant()) switch
        {
            ("FAHRENHEIT", "CELSIUS") => Math.Round((value - 32) * 5.0 / 9.0, 2),
            ("CELSIUS", "FAHRENHEIT") => Math.Round(value * 9.0 / 5.0 + 32, 2),
            ("CELSIUS", "KELVIN") => Math.Round(value + 273.15, 2),
            ("KELVIN", "CELSIUS") => Math.Round(value - 273.15, 2),
            _ => throw new ArgumentException($"不支持的换算：{from} → {to}")
        };

        return JsonSerializer.Serialize(new { value, from, to, result });
    }
}
