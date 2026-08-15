

我们在之前介绍过 Agent Framework 中 Function Call 函数调用。Function Call 可以让 LLM 调用并执行本地代码。

如下文章请查看：

- <https://mp.weixin.qq.com/s/Tj9Bem2BtYPsJDqnOIU19g>
- <https://mp.weixin.qq.com/s/Tj9Bem2BtYPsJDqnOIU19g>

今天我们介绍另外一种工具调用的模式，叫 Code Interpreter。

## 什么是 Code Interpreter

Code Interpreter 允许 Agent 在沙箱环境中编写并执行代码。

它适用于数据分析、数学计算、文件处理，以及其他能够通过代码执行来完成的任务。

> **注意：**
>
> 它本身不会执行代码，只是告诉底层 AI 服务：如果你本身支持代码执行，那么允许你执行模型生成的代码。

Code Interpreter 是否可用依赖于底层的 Provider。如下是微软官方提供的可用的 Provider：

1. Azure OpenAI
2. OpenAI
3. Microsoft Foundry
4. Anthropic
5. Google Gemini


## 如何使用 Code Interpreter

接下来介绍如何在 Agent Framework 中使用 Code Interpreter。由于这个功能由 Provider 提供，本篇将使用 Microsoft Foundry 进行演示。

我们会让 Agent 生成一段`python`脚本，用来统计 CSV 文件中的数据。脚本会在沙箱环境中运行，无法直接访问本地目录。因此，需要先把本地文件上传到沙箱，生成的代码才能读取并分析文件。

### 创建 Microsoft Foundry 客户端

首先创建 Microsoft Foundry 客户端，这个前面的文章已经用过很多次了。

```csharp
AIProjectClient aiProjectClient = new(new Uri("https://rg-maf.services.ai.azure.com/api/projects/maf"), new DefaultAzureCredential());
```

### 将文件上传到沙箱中

由于 Code Interpreter 运行在隔离的沙箱环境中，无法直接访问本地文件系统，因此需要先将待分析的 CSV 文件上传到 Microsoft Foundry。

如下代码通过 `OpenAIFileClient` 上传文件。运行前，请确保项目目录下存在 `data` 文件夹，并在其中放置名为 `sales_zh.csv` 的文件。这里将文件用途设置为 `FileUploadPurpose.Assistants`，以便后续将其提供给 Agent 使用。

```csharp
OpenAIFileClient fileClient = aiProjectClient
    .GetProjectOpenAIClient()
    .GetOpenAIFileClient();

OpenAIFile uploadedFile = (await fileClient.UploadFileAsync(
filePath: Path.Combine(Directory.GetCurrentDirectory(),"data","sales_zh.csv"),purpose: FileUploadPurpose.Assistants)).Value;
```

文件上传成功后，返回的 `OpenAIFile` 对象中会包含文件 ID。仅上传文件还不够，还需要将该 ID 传递给 Agent，使 Code Interpreter 能够在沙箱中访问对应文件。

这里使用 `HostedCodeInterpreterTool` 启用代码解释器，并通过 `Inputs` 属性添加 `HostedFileContent`，将上传后的文件作为工具输入提供给 Agent。这样，Agent 在生成并执行 Python 代码时，就可以读取该 CSV 文件并完成后续分析。

```csharp
AIAgent agent = aiProjectClient
    .GetProjectOpenAIClient()
    .GetProjectResponsesClient()
    .AsAIAgent(
        model: "gpt-4o",
        instructions: AgentInstructions,
        name: AgentName,
        tools: [new HostedCodeInterpreterTool() { Inputs = [new HostedFileContent(uploadedFile.Id)] }]);
```


### 运行 Agent 分析数据

运行agent生成代码，并解析文件。

```csharp
AgentResponse response = await agent.RunAsync($"帮我使用Python脚本统计 {uploadedFile.Id} 文件下不同区域的销售总数,并输出结果");
```




## 免责声明 / 风险与责任说明

如果您使用 Microsoft Agent Framework 构建与任何第三方服务器、Agent、代码或非 Azure Direct 模型（“第三方系统”）协同运行的应用程序，则由您自行承担风险。根据 Microsoft 产品条款，第三方系统属于非 Microsoft 产品，并受其各自第三方许可条款的约束。您需要对任何使用行为及其相关费用负责。

我们建议您审查与第三方系统共享以及从第三方系统接收的所有数据，并了解第三方在数据处理、共享、保留和存储位置方面的做法。您有责任管理您的数据是否会流出您所在组织的 Azure 合规性和地理边界，以及由此产生的任何相关影响，并确保已配置适当的权限、边界和审批机制。

您有责任结合自己的具体使用场景，仔细审查和测试使用 Microsoft Agent Framework 构建的应用程序，并做出所有适当的决策和自定义。这包括实施您自己的负责任 AI 缓解措施，例如元提示（metaprompt）、内容筛选器或其他安全系统，并确保您的应用程序满足适当的质量、可靠性、安全性和可信度标准。







	

