// Copyright (c) Microsoft. All rights reserved.

// FileAccessProvider — Give an agent scoped, read-only access to local files.
//
// The file store root is the application's output-directory "working" folder.
// The agent can list and read files in that folder, but cannot access paths
// outside it or modify its contents.

using System.Text;
using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
    ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

var workingDirectory = Path.Combine(AppContext.BaseDirectory, "working");
var salesFile = Path.Combine(workingDirectory, "sales.csv");

if (!File.Exists(salesFile))
{
    throw new FileNotFoundException(
        "未找到示例数据。请确认 sales.csv 已复制到输出目录的 working 文件夹。",
        salesFile);
}

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());

#pragma warning disable MAAI001
using FileAccessProvider fileAccessProvider = new(
    new FileSystemAgentFileStore(workingDirectory),
    new FileAccessProviderOptions
    {
        DisableWriteTools = true,
        DisableReadOnlyToolApproval = true,
    });
#pragma warning restore MAAI001

#pragma warning disable MAAI001, OPENAI001
AIAgent agent = aiProjectClient
    .GetProjectOpenAIClient()
    .GetProjectResponsesClient()
    .AsIChatClientWithStoredOutputDisabled(deploymentName)
    .AsAIAgent(new ChatClientAgentOptions
    {
        Name = "FileAccessDataAnalyst",
        Description = "读取受限 working 目录中的文件并分析销售数据。",
        ChatOptions = new ChatOptions
        {
            ModelId = deploymentName,
            Instructions =
                """
                你是一名销售数据分析助手，只能访问 file_access_* 工具公开的文件。
                回答前必须先列出可用文件，再读取相关文件。
                只能根据实际读取到的文件内容回答，不要编造数据。
                """,
        },
        AIContextProviders = [fileAccessProvider],
    });
#pragma warning restore MAAI001, OPENAI001

Console.WriteLine($"允许访问的目录：{workingDirectory}");
Console.WriteLine("正在分析 sales.csv...\n");

AgentResponse response = await agent.RunAsync(
    """
    请先列出可用文件，再读取 sales.csv。
    按商品计算销售额（quantity * unit_price），最后给出全部商品的总销售额。
    请使用简洁的表格展示计算过程和结果。
    """);

Console.WriteLine(response);
