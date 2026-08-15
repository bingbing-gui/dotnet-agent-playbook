我们在上一篇介绍了 Agent Framework 中的 File Search。File Search 可以让 Agent 检索上传到向量存储中的文件，并基于文件内容回答用户问题。

今天我们介绍另外一种工具调用模式，叫 Web Search。

## 什么是 Web Search

Web Search 允许 Agent 搜索互联网以获取最新信息。借助该工具，Agent 可以回答与当前事件相关的问题、查找文档，并获取超出其训练数据范围的信息。

> **注意：**
>
> 该工具本身并不实现 Web 搜索功能。它只是一个标记（Marker），用于告知底层服务：如果该服务本身具备 Web 搜索能力，那么允许它执行 Web 搜索。HostedWebSearchTool 底层通过 Grounding with Bing Search 实现。

## 如何使用 Web Search

接下来介绍如何在 Agent Framework 中使用 Web Search。

示例中会让 Agent 查询今天东京的天气。完整流程如下：

1. 创建 Microsoft Foundry 客户端。
2. 为 Agent 配置 `HostedWebSearchTool`。
3. 运行 Agent 搜索网络内容。
4. 获取回答中的网页引用。

与 File Search 不同，Web Search 不需要提前上传文件，也不需要创建向量存储。

### 安装 NuGet 包

首先在项目中安装 Microsoft Foundry Provider 和 Azure 身份认证相关的 NuGet 包。

```xml
<ItemGroup>
	<PackageReference Include="Azure.Identity" Version="1.21.0" />
	<PackageReference Include="Microsoft.Agents.AI.Foundry" Version="1.5.0" />
</ItemGroup>
```

### 创建 Microsoft Foundry 客户端

首先从环境变量中读取 Microsoft Foundry 项目终结点和模型部署名称，然后创建 `AIProjectClient`。

```csharp
string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
	?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
	?? "gpt-4o";

AIProjectClient aiProjectClient = new(
	new Uri(endpoint),
	new DefaultAzureCredential());
```

这里使用 `DefaultAzureCredential` 完成身份认证。它适合本地开发；在生产环境中，建议根据部署方式使用 `ManagedIdentityCredential` 等更加明确的凭据。

Web Search 的可用性取决于底层 Provider 和模型。Microsoft Foundry Responses API 的官方文档说明，Web Search 支持 GPT-4 及更新的模型。使用前还需要确保目标区域和订阅允许使用该功能。

### 为 Agent 配置 Web Search

接下来创建 `AIAgent`，并通过 `HostedWebSearchTool` 启用 Web Search。

```csharp
const string AgentInstructions =
	"你是一个乐于助人的助手，可以搜索网络以查找最新信息并准确回答问题。";

const string AgentName = "WebSearchAgent-RAPI";

AIAgent agent = aiProjectClient.AsAIAgent(
	deploymentName,
	instructions: AgentInstructions,
	name: AgentName,
	tools: [new HostedWebSearchTool()]);
```

`HostedWebSearchTool` 表示允许底层 Provider 执行托管的网络搜索。开发者不需要自己创建 Bing 搜索客户端，也不需要实现网页抓取和搜索结果解析。

是否真正调用 Web Search，由模型根据用户问题和 Agent 指令决定。如果问题可以直接回答，模型可能不会搜索网络；如果希望它检索最新信息，可以在指令和用户问题中明确要求搜索网络并引用来源。

### 运行 Agent 搜索网络

现在向 Agent 询问今天东京的天气。

```csharp
AgentResponse response = await agent.RunAsync("今天东京的天气怎么样？");

Console.WriteLine($"响应: {response.Text}");
```

天气属于实时信息，模型会调用 Web Search 查找相关网页，再根据搜索结果生成回答。

搜索结果取决于网络内容和 Bing 的索引更新时间，因此不同时间运行示例可能得到不同答案。对于天气、价格等变化频繁的信息，仍应在界面上展示查询时间和来源链接。

### 获取网页引用

Web Search 的响应中可能包含网页引用。我们可以遍历响应消息中的 `Annotations`，读取来源网页的标题和 URL。

```csharp
foreach (AIAnnotation annotation in response.Messages
	.SelectMany(message => message.Contents)
	.SelectMany(content => content.Annotations ?? []))
{
#pragma warning disable OPENAI001
	if (annotation.RawRepresentation is UriCitationMessageAnnotation urlCitation)
	{
		Console.WriteLine($$"""
			网页引用:

			  标题: {{urlCitation.Title}}
			  URL: {{urlCitation.Uri}}
			""");
	}
#pragma warning restore OPENAI001
}
```


### 运行效果

运行程序后，Agent 会搜索与东京天气相关的公开网页，输出基于实时搜索结果生成的回答，并列出引用网页的标题和 URL。

图片


## 总结

Web Search 允许 Agent 检索公网上查询最新内容，并基于搜索结果回答用户问题。在 Agent Framework 中，只需要为 Agent 添加 `HostedWebSearchTool`，底层 Provider 就可以负责搜索、结果处理和来源引用。


