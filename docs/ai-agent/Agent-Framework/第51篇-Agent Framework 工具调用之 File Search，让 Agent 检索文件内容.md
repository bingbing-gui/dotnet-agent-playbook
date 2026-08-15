我们在上一篇介绍了 Agent Framework 中的 Code Interpreter。Code Interpreter 可以让 Agent 在沙箱环境中生成并执行代码，适合数据分析、数学计算和文件处理等场景。

如下文章请查看：

- [第50篇-Agent Framework 工具调用之 Code Interpreter，让 Agent 执行模型生成的代码](./第50篇-Agent%20Framework%20工具调用之%20Code%20Interpreter，让%20Agent%20执行模型生成的代码.md)

今天我们介绍另外一种工具调用模式，叫 File Search。

## 什么是 File Search

File Search 是 Microsoft Foundry 提供的托管式 Retrieval-Augmented Generation (RAG)能力。如果你不想自己实现RAG机制，可以直接使用 File Search功能，让 Agent 在上传的文件中检索相关内容，并基于检索结果回答用户问题。

它适用于企业知识库、产品手册、规章制度、合同文档和内部资料问答等场景。

如下是 File Search 支持的每个文件的大小以及每个向量存储的文件数限制。

| 指标 | 最大限制 | 计算方式 |
| --- | ---: | --- |
| 单个文件大小 | 512 MB | 按上传文件的实际大小计算 |
| 单个文件 Token 数 | 5,000,000 Token | 按文件解析并提取后的文本计算 |
| 每个向量存储的文件数 | 10,000 个 | 按文件对象数量计算，与页数和 Chunk 数无关 |

> 以上限制以 [Microsoft Foundry 官方文档](https://learn.microsoft.com/en-us/azure/foundry/agents/how-to/tools/file-search?pivots=csharp#vector-stores)的最新说明为准。


## 如何使用 File Search

接下来介绍如何在 Agent Framework 中使用 File Search。

示例中会创建一份员工目录文件，并让 Agent 回答“谁是最年轻的员工？”。完整流程如下：

1. 创建 Microsoft Foundry 客户端。
2. 将本地文件上传到 Microsoft Foundry。
3. 使用上传后的文件创建向量存储。
4. 为 Agent 配置 `HostedFileSearchTool`。
5. 运行 Agent 并获取文件引用。
6. 删除示例创建的云端和本地资源。

### 创建 Microsoft Foundry 客户端

首先从环境变量中读取 Microsoft Foundry 项目终结点和模型部署名称，然后创建 `AIProjectClient`。

```csharp
string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
	?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
	?? "gpt-5.4-mini";

AIProjectClient aiProjectClient = new(
	new Uri(endpoint),
	new DefaultAzureCredential());
```

这里使用 `DefaultAzureCredential` 完成身份认证。

### 创建待检索文件

为了方便演示，先在系统临时目录中创建一份员工目录文件。

```csharp
var searchFilePath = Path.Combine(
	Path.GetTempPath(),
	Path.GetRandomFileName() + "_lookup.txt");

File.WriteAllText(
	path: searchFilePath,
	contents: """
		员工目录：
		- 桂兵兵，40岁，软件架构师，软件开发部
		- 张三，35岁，销售经理，销售部
		- 李四，42岁，人力资源总监，人力资源部
		- 王五，31岁，客户支持主管，支持部
		""");
```

### 将文件上传到 Microsoft Foundry

File Search 无法直接访问本地文件，因此需要通过 `ProjectFilesClient` 将文件上传到 Microsoft Foundry。

```csharp
ProjectOpenAIClient projectOpenAIClient =
	aiProjectClient.GetProjectOpenAIClient();

ProjectFilesClient projectFilesClient =
	projectOpenAIClient.GetProjectFilesClient();

OpenAIFile uploadedFile = projectFilesClient.UploadFile(
	filePath: searchFilePath,
	purpose: FileUploadPurpose.Assistants);

Console.WriteLine($"已上传文件，文件 ID: {uploadedFile.Id}");
```

这里将文件用途设置为 `FileUploadPurpose.Assistants`。上传成功后，返回的 `OpenAIFile` 对象中会包含文件 ID，后续创建向量存储时需要使用该 ID。

### 创建向量存储

仅上传文件还不够。File Search 需要通过向量存储对文件进行索引和检索，因此还需要创建 `ProjectVectorStoresClient`，并将上传后的文件加入向量存储。

```csharp
ProjectVectorStoresClient projectVectorStoresClient =
	projectOpenAIClient.GetProjectVectorStoresClient();

var vectorStoreResult = await projectVectorStoresClient.CreateVectorStoreAsync(
	options: new()
	{
		FileIds = { uploadedFile.Id },
		Name = "EmployeeDirectory_VectorStore"
	});

string vectorStoreId = vectorStoreResult.Value.Id;
Console.WriteLine($"已创建向量存储，向量存储 ID: {vectorStoreId}");
```

向量存储负责管理文件的解析、切分和索引。创建完成后，我们会得到一个向量存储 ID，Agent 将通过这个 ID 确定需要检索的知识范围。

### 为 Agent 配置 File Search

接下来创建 `AIAgent`，并通过 `HostedFileSearchTool` 启用 File Search。

```csharp
const string AgentInstructions =
	"你是一个乐于助人的助手，可以搜索上传的文件以回答问题。";

AIAgent agent = aiProjectClient.AsAIAgent(
	deploymentName,
	instructions: AgentInstructions,
	name: "FileSearchAgent-RAPI",
	tools:
	[
		new HostedFileSearchTool()
		{
			Inputs = [new HostedVectorStoreContent(vectorStoreId)]
		}
	]);
```

`HostedFileSearchTool` 表示允许底层 Provider 执行托管的文件检索。`HostedVectorStoreContent` 则把刚才创建的向量存储提供给 Agent。

这样，当用户的问题需要查询文件内容时，模型就可以调用 File Search，在向量存储中查找相关片段，并基于检索结果生成回答。

### 运行 Agent 检索文件

现在向 Agent 提问，让它从员工目录中找出年龄最小的员工。

```csharp
AgentResponse response = await agent.RunAsync("谁是最年轻的员工？");

Console.WriteLine($"响应: {response}");
```

Agent 会调用 File Search 检索员工目录，并根据文件内容回答：王五是最年轻的员工，年龄为 31 岁。


### 清理资源

示例运行完成后，删除创建的向量存储、云端文件和本地临时文件，避免产生不必要的资源占用。

```csharp
await projectVectorStoresClient.DeleteVectorStoreAsync(vectorStoreId);
await projectFilesClient.DeleteFileAsync(uploadedFile.Id);
File.Delete(searchFilePath);

Console.WriteLine("清理完成。");
```

生产环境中的知识库通常需要长期保留，因此不应在每次问答后删除文件和向量存储。可以在文档更新、知识库下线或文件过期时统一清理。

### 运行效果




## 总结

File Search 允许 Agent 从上传的文件中检索相关内容，并基于检索结果回答用户问题。背后其实替你实现了一套 Retrieval-Augmented Generation (RAG) 机制。当然，如果你自己不想用它，也可以选择自己实现。



