

我们在上一篇介绍了 Agent Framework 中的 Web Search。Web Search 可以让 Agent 检索公开网络中的最新内容，并基于搜索结果回答用户问题。

今天我们介绍另外一种工具调用模式：使用 `FoundryAITool` 根据 OpenAPI Specification 调用外部 REST API。

## 什么是 OpenAPI Specification

OpenAPI Specification（OAS）是一种标准的、与编程语言无关的 HTTP API 接口描述规范。它允许人和计算机在不访问源代码、不依赖额外文档、也不检查网络流量的情况下，发现并理解一个服务所提供的能力。

OpenAPI 是一种与编程语言无关的 HTTP API 描述标准，用于以机器可读的方式描述 API 的接口、参数、请求、响应及相关能力，使开发者和工具能够在无需查看源代码的情况下理解并调用 API。


## FoundryAITool 类介绍

`FoundryAITool` 是一个静态工厂类，用于创建或转换 Microsoft Foundry/OpenAI Responses 专属工具，并统一返回 Agent Framework 使用的 `AITool`。

它封装了 `AgentTool` 和 `ResponseTool` 的创建及转换过程，避免开发者手动进行类型转换并调用 `.AsAITool()`。它本身不负责执行工具，实际能力由 Microsoft Foundry、OpenAI Responses 或其连接的外部服务提供。

### Microsoft Foundry 与 OpenAI Responses 的工具定义理解

OpenAI Responses 提供模型原生的通用工具定义，这些工具在 .NET SDK 中通常表示为 `ResponseTool`。

Microsoft Foundry 支持或适配其中一部分 Responses 工具，并提供Azure与企业场景相关的工具。这些Foundry工具通常由 `AgentTool` 及其配置类型表示。

两套工具定义存在部分重叠，但并不完全相同。`FoundryAITool` 将这两种来源的工具创建或转换为 Agent Framework 统一使用的 `AITool`：

#### OpenAI Responses 工具

OpenAI Responses API 主要提供以下工具定义：

| 工具 | 作用 |
| --- | --- |
| Function Tool | 让模型生成结构化函数调用参数 |
| Web Search | 搜索公开网络 |
| File Search | 检索 Vector Store 中的文件 |
| Code Interpreter | 在托管容器中执行代码 |
| Image Generation | 生成或编辑图片 |
| Computer Use | 操作屏幕、鼠标和键盘 |
| MCP Tool | 调用远程 MCP Server |
| OpenAPI Tool | 根据 OpenAPI 规范调用 REST API |

这些工具在 .NET SDK 中通常表示为 `OpenAI.Responses.ResponseTool` 或其具体派生类型。

#### Microsoft Foundry 工具

Microsoft Foundry 除了支持部分 OpenAI Responses 工具，还提供 Azure 和企业数据相关的工具定义：

| 工具 | 作用 |
| --- | --- |
| OpenAPI Tool | 调用 OpenAPI 3.0/3.1 描述的接口 |
| Azure AI Search | 检索 Azure AI Search 索引 |
| Bing Grounding | 使用 Bing 搜索为回答提供依据 |
| Bing Custom Search | 在自定义网站范围内搜索 |
| SharePoint Grounding | 检索 SharePoint 内容 |
| Microsoft Fabric | 查询 Fabric Data Agent |
| Browser Automation | 执行浏览器自动化 |
| A2A Tool | 通过 A2A 协议调用其他 Agent |
| Hosted MCP Toolbox | 使用 Foundry 托管的 Toolbox |
| Structured Outputs | 按指定结构捕获输出 |

这些工具通常由 `Azure.AI.Projects.Agents.AgentTool` 及其配置类型表示。

因此，`FoundryAITool` 可以理解为 Microsoft Foundry/OpenAI Responses 工具与 Agent Framework 之间的适配层，而不是这些工具的实际执行引擎。

## FoundryAITool 提供的能力

下面是 `FoundryAITool` 提供的主要能力分类和方法，我们在前面文章中也介绍过其中的几种工具调用的能力。

### 搜索与知识检索

| 方法 | 作用 |
| --- | --- |
| `CreateWebSearchTool(...)` | 搜索公开互联网，可配置用户位置、搜索上下文大小和过滤条件 |
| `CreateBingGroundingTool(...)` | 使用 Bing 搜索结果为模型回答提供事实依据 |
| `CreateBingCustomSearchTool(...)` | 在 Bing Custom Search 配置的特定网站范围内搜索 |
| `CreateFileSearchTool(...)` | 在指定 Vector Store 中检索文件内容 |
| `CreateAzureAISearchTool(...)` | 检索 Azure AI Search 中的索引数据 |
| `CreateSharepointTool(...)` | 检索 SharePoint 中的企业内容 |

### 数据与企业服务

| 方法 | 作用 |
| --- | --- |
| `CreateMicrosoftFabricTool(...)` | 连接 Microsoft Fabric Data Agent，查询 Fabric 数据 |
| `CreateOpenApiTool(...)` | 根据 OpenAPI Specification 调用外部 REST API |

### Agent 与协议集成

| 方法 | 作用 |
| --- | --- |
| `CreateA2ATool(Uri, String)` | 通过 Agent-to-Agent 协议调用其他 Agent |
| `CreateMcpTool(String, Uri, ...)` | 通过 MCP Server 地址连接 MCP 工具 |
| `CreateMcpTool(String, McpToolConnectorId, ...)` | 通过 Foundry Connector ID 连接 MCP 工具 |

### 代码与计算机操作

| 方法 | 作用 |
| --- | --- |
| `CreateCodeInterpreterTool(...)` | 在托管沙箱中执行模型生成的代码 |
| `CreateBrowserAutomationTool(...)` | 执行托管的浏览器自动化操作 |
| `CreateComputerTool(...)` | 执行屏幕、鼠标和键盘等计算机交互 |

### 函数与输出控制

| 方法 | 作用 |
| --- | --- |
| `CreateFunctionTool(...)` | 根据函数名称、描述和 JSON Schema 创建函数声明工具 |
| `CreateStructuredOutputsTool(...)` | 按指定 Schema 捕获结构化输出 |


### 内容生成

| 方法 | 作用 |
| --- | --- |
| `CreateImageGenerationTool(...)` | 调用图像生成模型，并配置尺寸、质量、格式、背景和压缩率等参数 |

### 工具转换

| 方法 | 作用 |
| --- | --- |
| `FromResponseTool(ResponseTool)` | 将已有的 OpenAI `ResponseTool` 转换为 Agent Framework 的 `AITool` |


## 使用 FoundryAITool

接下来介绍如何通过 `FoundryAITool` 为 Agent 添加 OpenAPI Tool。

示例中会使用 Frankfurter 提供的公开汇率 API，让 Agent 查询美元对欧元、英镑、日元和人民币的最新汇率。完整流程如下：

1. 创建 Microsoft Foundry 客户端。
2. 编写 API 的 OpenAPI Specification。
3. 创建 `OpenApiFunctionDefinition`。
4. 使用 `FoundryAITool.CreateOpenApiTool` 创建工具。
5. 将工具提供给 Agent 并运行。

### 安装 NuGet 包

首先在项目中安装 Microsoft Foundry Provider 和 Azure 身份认证相关的 NuGet 包。

```xml
<ItemGroup>
	<PackageReference Include="Azure.Identity" Version="1.21.0" />
	<PackageReference Include="Microsoft.Agents.AI.Foundry" Version="1.5.0" />
</ItemGroup>
```

### 创建 Microsoft Foundry 客户端

从环境变量中读取 Microsoft Foundry 项目终结点和模型部署名称，然后创建 `AIProjectClient`。

```csharp
string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
	?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");

string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL")
	?? "gpt-5.4-mini";

AIProjectClient aiProjectClient = new(
	new Uri(endpoint),
	new DefaultAzureCredential());
```

这里使用 `DefaultAzureCredential` 完成身份认证。它适合本地开发；在生产环境中，建议根据部署方式使用 `ManagedIdentityCredential` 等更加明确的凭据。

### 编写 OpenAPI Specification

接下来使用 OpenAPI 3.1 描述 Frankfurter 的 `/latest` 接口（该结构来自微软官方推荐案例）。

```csharp
const string FrankfurterOpenApiSpec = """
{
	"openapi": "3.1.0",
	"info": {
		"title": "Frankfurter Exchange Rate API",
		"description": "Free currency exchange rates from the European Central Bank",
		"version": "v1"
	},
	"servers": [
		{
			"url": "https://api.frankfurter.dev/v1"
		}
	],
	"paths": {
		"/latest": {
			"get": {
				"description": "Get the latest exchange rates for a given base currency",
				"operationId": "GetLatestExchangeRates",
				"parameters": [
					{
						"name": "from",
						"in": "query",
						"description": "Base currency code (e.g. EUR, USD, GBP). Defaults to EUR.",
						"required": false,
						"schema": {
							"type": "string"
						}
					},
					{
						"name": "to",
						"in": "query",
						"description": "Comma-separated list of target currency codes (e.g. USD,GBP,JPY).",
						"required": false,
						"schema": {
							"type": "string"
						}
					}
				],
				"responses": {
					"200": {
						"description": "Latest exchange rates",
						"content": {
							"application/json": {
								"schema": {
									"type": "object"
								}
							}
						}
					}
				}
			}
		}
	}
}
""";
```

`servers` 定义 API 的基础地址，`paths` 定义可以调用的接口，`parameters` 描述接口参数。

其中，`operationId` 非常重要。模型会通过它和接口描述判断应该调用哪个操作。

### 创建 OpenApiFunctionDefinition

有了 OpenAPI 规范后，使用它创建 `OpenApiFunctionDefinition`。

```csharp
OpenApiFunctionDefinition CreateOpenAPIFunctionDefinition()
{
	return new(
		"get_exchange_rates",
		BinaryData.FromString(FrankfurterOpenApiSpec),
		new OpenAPIAnonymousAuthenticationDetails())
	{
		Description = "获取来自欧洲中央银行的实时货币汇率，通过 Frankfurter API 提供"
	};
}
```

构造函数中的三个参数分别表示：

1. OpenAPI 工具名称。
2. OpenAPI Specification 的二进制内容。
3. 调用目标 API 时采用的认证方式。

Frankfurter 是公开且无须认证的 API，因此这里使用 `OpenAPIAnonymousAuthenticationDetails`。

Microsoft Foundry 的 OpenAPI Tool 支持以下三类认证方式：

| 认证方式 | 适用场景 |
| --- | --- |
| Anonymous | 不需要认证的公开 API |
| API Key | 使用请求头或查询参数传递密钥的 API |
| Managed Identity | 受 Microsoft Entra ID 保护的 Azure 服务或自定义 API |

对于 API Key 和 Token，不应将密钥直接写入 OpenAPI 文档或源代码。应该在 Microsoft Foundry 项目中创建连接，并由托管服务在调用时注入凭据。

### 使用 FoundryAITool 创建工具

接下来通过 `FoundryAITool.CreateOpenApiTool` 将 `OpenApiFunctionDefinition` 转换为 Agent Framework 中的 `AITool`。

```csharp
#pragma warning disable OPENAI001
AITool openApiTool = FoundryAITool.CreateOpenApiTool(
	CreateOpenAPIFunctionDefinition());
#pragma warning restore OPENAI001
```

`FoundryAITool` 是 Microsoft Foundry Provider 提供的适配器。它负责把 Foundry 特有的 OpenAPI 工具定义接入 Agent Framework 的统一 `AITool` 抽象。

当前相关 API 仍带有实验性标记，因此示例使用 `#pragma warning` 暂时关闭 `OPENAI001` 警告。后续升级 SDK 时，应检查 API 是否发生变化。

### 为 Agent 配置 OpenAPI Tool

创建 Agent 时，将 `openApiTool` 添加到 `tools` 集合中。

```csharp
const string AgentInstructions =
	"你是一位乐于助人的助手，能够利用 Frankfurter API 获取最新的货币汇率。" +
	"请务必调用 API 获取实时数据，而不要进行推测。";

AIAgent agent = aiProjectClient.AsAIAgent(
	deploymentName,
	instructions: AgentInstructions,
	name: "OpenAPIToolsAgent",
	tools: [openApiTool]);
```

这里在 Agent 指令中明确要求调用 API 获取数据，避免模型直接使用已有知识推测汇率。

Agent 获得工具后，模型会读取工具名称、描述、`operationId` 和参数说明。当用户提出汇率问题时，模型可以选择 `GetLatestExchangeRates` 操作，并将币种代码填入 `from` 和 `to` 查询参数。

### 运行 Agent 调用接口

现在让 Agent 查询多种货币的最新汇率。

```csharp
AgentResponse response = await agent.RunAsync(
	"最新的美元(USD)对欧元(EUR)、英镑(GBP)、日元(JPY)和人民币(CNY)的汇率是多少？");

Console.WriteLine(response.Text);
```


### 运行效果

运行程序后，Agent 会调用 Frankfurter API 获取最新汇率，并根据接口返回的数据回答用户问题。

图片

## 总结

我们可以看到FoundryAITool 除了可以调用OpenAPI接口，还提供了别的大量的工具箱，包括搜索、知识检索、数据与企业服务、Agent与协议集成、代码与计算机操作、函数与输出控制、内容生成和工具转换等能力。

