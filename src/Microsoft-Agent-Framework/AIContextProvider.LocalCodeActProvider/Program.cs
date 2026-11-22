// Copyright (c) Microsoft. All rights reserved.

using System.ComponentModel;
using System.Text;
using System.Text.Json;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.LocalCodeAct;
using Microsoft.Extensions.AI;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT");
if (string.IsNullOrWhiteSpace(endpoint))
{
    throw new InvalidOperationException("请设置 FOUNDRY_PROJECT_ENDPOINT 为 Foundry 项目终结点。");
}

var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL");
if (string.IsNullOrWhiteSpace(deploymentName))
{
    throw new InvalidOperationException("请设置 FOUNDRY_MODEL 为 Foundry 模型部署名称。");
}

var pythonExecutable = Environment.GetEnvironmentVariable("LOCAL_CODEACT_PYTHON");
if (string.IsNullOrWhiteSpace(pythonExecutable))
{
    pythonExecutable = OperatingSystem.IsWindows() ? "python.exe" : "python3";
}

Console.WriteLine("安全提示：LocalCodeAct 不是安全沙箱，生成的 Python 会在本机执行。");
Console.WriteLine("请仅在隔离环境中运行，并在每次执行前仔细检查代码。\n");
Console.WriteLine($"Python 解释器：{pythonExecutable}");
Console.WriteLine($"模型部署：{deploymentName}\n");

#pragma warning disable MAAI001
using var codeAct = new LocalCodeActProvider(
    pythonExecutable,
    new LocalCodeActProviderOptions
    {
        Tools =
        [
            AIFunctionFactory.Create(Compute, name: "compute"),
            AIFunctionFactory.Create(FetchData, name: "fetch_data"),
        ],
        ApprovalMode = LocalCodeActApprovalMode.AlwaysRequire,
        ExecutionLimits = new ProcessExecutionLimits { TimeoutSeconds = 15 },
    });

AIAgent agent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "LocalCodeActAssistant",
        Description = "通过本地 Python 调用计算和模拟数据查询工具。",
        ChatOptions = new ChatOptions
        {
            ModelId = deploymentName,
            Instructions = """
                你是一名中文数据分析助手。请使用 execute_code 编写 Python，
                并通过 await call_tool(...) 调用工具。不要直接猜测模拟数据。
                Python 代码已经运行在异步环境中，请在顶层直接使用 await；
                不要使用 asyncio.run，也不要仅调用异步函数而不等待结果。
                使用 print 输出结果；仅给 result 变量赋值不会输出结果。
                不要依赖第三方 Python 包。
                fetch_data 可查询 users 和 products；compute 可执行基本算术运算。
                fetch_data 直接返回记录列表，compute 直接返回数值。
                例如：await call_tool("fetch_data", table="products")；
                await call_tool("compute", operation="add", a=1, b=2)。
                请用中文简洁地展示分析过程和结果。
                """,
        },
        AIContextProviders = [codeAct],
    })
    .AsBuilder()
    .Use(async (agent, context, next, cancellationToken) =>
    {
        try
        {
            return await next(context, cancellationToken);
        }
        catch (Exception ex) when (context.Function.Name == "execute_code")
        {
            Console.Error.WriteLine($"Python 执行失败：{ex.Message}");
            throw;
        }
    })
    .Build();
#pragma warning restore MAAI001

const string question = """
    请查询 products 表，计算所有商品的总价和平均价格；
    再查询 users 表，列出角色为“管理员”的用户姓名。
    请使用工具获取数据并进行计算。
    """;

Console.WriteLine($"问题：\n{question}\n");
AgentSession session = await agent.CreateSessionAsync();
AgentResponse response = await agent.RunAsync(question, session);

while (true)
{
    var requests = response.Messages
        .SelectMany(message => message.Contents)
        .OfType<ToolApprovalRequestContent>()
        .ToList();
    if (requests.Count == 0)
    {
        break;
    }

    var approvals = new List<ChatMessage>();
    foreach (var request in requests)
    {
        if (request.ToolCall is not FunctionCallContent { Name: "execute_code" } call ||
            call.Arguments is null ||
            !call.Arguments.TryGetValue("code", out object? codeArgument))
        {
            throw new InvalidOperationException("无法获取待审批的 Python 代码，已停止执行。");
        }

        string? code = codeArgument switch
        {
            string value => value,
            JsonElement { ValueKind: JsonValueKind.String } value => value.GetString(),
            _ => null,
        };
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("待审批的 Python 代码为空或格式不正确，已停止执行。");
        }

        Console.WriteLine($"\n待执行的 Python 代码：\n{code}\n");
        Console.Write("确认在本机执行以上代码？输入 Y 批准，其他输入退出：");
        bool approved = string.Equals(Console.ReadLine()?.Trim(), "Y", StringComparison.OrdinalIgnoreCase);
        if (!approved)
        {
            Console.WriteLine("已取消执行。");
            return;
        }

        approvals.Add(new ChatMessage(ChatRole.User, [request.CreateResponse(true)]));
    }

    response = await agent.RunAsync(approvals, session);
}

Console.WriteLine($"\n助手：\n{response}");

[Description("执行加、减、乘、除运算。")]
static double Compute(
    [Description("运算类型：add、subtract、multiply 或 divide。")] string operation,
    [Description("第一个数值。")] double a,
    [Description("第二个数值。")] double b) => operation switch
    {
        "add" => a + b,
        "subtract" => a - b,
        "multiply" => a * b,
        "divide" when b != 0 => a / b,
        "divide" => throw new ArgumentException("除数不能为零。", nameof(b)),
        _ => throw new ArgumentException($"不支持的运算：{operation}。", nameof(operation)),
    };

[Description("查询模拟数据表，支持 users（用户）和 products（商品）。")]
static IReadOnlyList<IReadOnlyDictionary<string, object>> FetchData(
    [Description("要查询的表名：users 或 products。")] string table)
{
    Dictionary<string, IReadOnlyList<IReadOnlyDictionary<string, object>>> data = new()
    {
        ["users"] =
        [
            new Dictionary<string, object> { ["id"] = 1, ["name"] = "张伟", ["role"] = "管理员" },
            new Dictionary<string, object> { ["id"] = 2, ["name"] = "李娜", ["role"] = "普通用户" },
            new Dictionary<string, object> { ["id"] = 3, ["name"] = "王芳", ["role"] = "管理员" },
        ],
        ["products"] =
        [
            new Dictionary<string, object> { ["id"] = 101, ["name"] = "笔记本电脑", ["price"] = 5000.0 },
            new Dictionary<string, object> { ["id"] = 102, ["name"] = "鼠标", ["price"] = 100.0 },
        ],
    };

    return data.TryGetValue(table, out var rows)
        ? rows
        : throw new ArgumentException($"未知的数据表：{table}。", nameof(table));
}
