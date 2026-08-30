# 把 ASP.NET Core Web API能力转化为 MCP 工具，提供Agent调用

前面我们介绍过 MCP 的调用方式，并使用 `McpClient` 调用了 Microsoft Learn 提供的 MCP 服务，把 Microsoft Learn提供的MCP Tools转换成本地Agent可调用的工具并实现自然语言的交互。
那么，我们能不能把现有的 ASP.NET Core Web API 转换成 MCP 工具，让外部大模型或 Agent 通过自然语言来调用呢？答案当然是可以的。

这一节，我们就把一个现有的 ASP.NET Core 天气查询能力封装成 MCP Tool，并使用 ASP.NET Core MCP Client + Microsoft Foundry Agent 完成完整调用。

如果 API 本身是公开的，可以不做鉴权；如果 API 需要认证，那么调用 MCP 服务时同样需要携带 Token。这里我们使用 JWT 完成认证和授权。

## 整体架构

整个项目的核心，就是把传统的 Web API 能力包装成 MCP Tool，再通过 MCP Client 把这些工具交给 Agent，由模型根据用户自然语言自动决定是否调用、调用哪个工具以及传入什么参数。



## 项目结构

这个包含两个项目：

1. **AspNetCoreMcpServer**

   这是实际的业务系统，也就是服务端。我们把天气查询能力转换成 MCP Tool，对外提供中国城市的实时天气和未来三天天气预报。

2. **AspNetCoreMcpClient**

   这是调用 MCP Server 的客户端，同时也是一个 ASP.NET Core Web 应用。页面提供自然语言输入框，用户输入问题后，由 Microsoft Foundry 上的模型自动选择并调用 MCP 工具，再根据天气数据给出回答。

   你也可以用DeepSeek或其他支持工具调用的模型替换 Microsoft Foundry。

如果想实现自己的 MCP 服务，少不了官方提供的 C# SDK：

- Model Context Protocol C# SDK`https://github.com/modelcontextprotocol/csharp-sdk`

两个项目都基于 .NET 10。

## ASP.NET Core MCP Server

`AspNetCoreMcpServer` 是实际的业务系统，也是 MCP 工具的提供方。。项目使用 ASP.NET Core Web API，并安装了下面几个包：

- `ModelContextProtocol.AspNetCore`：MCP 官方 C# SDK 的 ASP.NET Core 支持
- `Microsoft.AspNetCore.Authentication.JwtBearer`：JWT 认证和授权
- `Microsoft.AspNetCore.OpenApi`：OpenAPI 支持


服务端主要处理下面几件事：

1. 注册 MCP Server
2. 定义 MCP Tool
3. 映射 MCP Endpoint
4. 配置 JWT 认证和授权
5. 配置 CORS
6. 调用 Open-Meteo 获取天气数据

### 注册 MCP 服务

首先在 `AspNetCoreMcpServer/Program.cs` 中注册 MCP Server：

```csharp
builder.Services.AddMcpServer()
    .WithHttpTransport(options =>
        options.SessionMode = HttpServerSessionMode.Stateless)
    .WithTools<WeatherTools>();
```

`WithHttpTransport` 表示 MCP Server 通过 HTTP Transport 对外提供服务。
这里将：
```csharp
options.SessionMode = HttpServerSessionMode.Stateless
```
设置为无状态模式。因为这个示例不需要保存 MCP 会话状态，也不会使用服务器主动向客户端发送请求等能力，所以使用无状态模式更加合适。

真正把 `WeatherTools` 注册为 MCP 工具集合的是：
```csharp
.WithTools<WeatherTools>()
```
完成注册以后，MCP Server 就能够发现 `WeatherTools` 中定义的 MCP Tool。

## 定义 WeatherTools

接下来定义真正对外暴露的 MCP Tool。

```csharp
[McpServerToolType]
public sealed class WeatherTools(IHttpClientFactory httpClientFactory)
{
    [McpServerTool(Name = "get_china_weather_forecast")]
    [Description("按中国城市名称查询实时天气和未来三天天气预报。")]
    public async Task<string> GetChinaWeatherForecast(
        [Description("中国城市名称，例如：北京、上海、广州、深圳。")] string city,
        CancellationToken cancellationToken)
    {
        // 调用天气服务并返回查询结果
    }
}
```

这里主要使用了两个 MCP SDK 提供的特性。

`[McpServerToolType]` 用来声明当前类是一个 MCP Tool 类型。
`[McpServerTool]` 用来声明某个方法可以作为 MCP Tool 暴露给 MCP Client。


## 映射 MCP Endpoint

定义好 MCP Tool 以后，还需要把 MCP Server 映射成 HTTP Endpoint：
```csharp
app.MapMcp()
    .RequireAuthorization()
    .RequireCors("McpBrowserClient");
```

`AddMcpServer()` 负责注册 MCP Server 相关服务。
`WithTools<WeatherTools>()` 负责注册我们定义的 MCP Tools。
`MapMcp()` 则真正把 MCP Server 暴露成可以通过 HTTP 访问的 MCP Endpoint。


## 配置 JWT 认证和授权

MCP 并不意味着需要重新设计一套认证体系。MCP Endpoint 本质上仍然运行在 ASP.NET Core 应用中，因此 ASP.NET Core 原有的 Authentication 和 Authorization 机制依然可以继续使用。

在这个示例中，我们使用 JWT Bearer Token 对 MCP Endpoint 进行保护。

```csharp
app.MapMcp()
    .RequireAuthorization();
```

这里的：
```csharp
.RequireAuthorization()
```

表示访问 MCP Endpoint 的请求必须先通过 ASP.NET Core 的授权检查。
服务端通过 `/auth/token` 提供 Token 获取接口。

也就是说，我们把现有 ASP.NET Core API 暴露成 MCP Tool 以后，原来的认证和授权体系仍然可以继续复用。

如果 MCP Endpoint 本身就是公开服务，也可以根据实际业务需求不添加：
```csharp
.RequireAuthorization()
```

### 配置 CORS

项目中还配置了：

```csharp
.RequireCors("McpBrowserClient");
```

以及对应的 CORS 策略：

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("McpBrowserClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5164",
                "https://localhost:7133")
            .WithMethods("POST")
            .WithHeaders(
                HeaderNames.ContentType,
                HeaderNames.Authorization,
                "MCP-Protocol-Version")
            .WithExposedHeaders(HeaderNames.WWWAuthenticate);
    });
});
```

这里需要注意，**CORS 和 MCP 本身没有直接关系**。CORS 主要用于限制浏览器中的跨源请求。

如果调用 MCP Server 的是 ASP.NET Core Client 后端，那么这属于服务器之间的 HTTP 请求，一般不会受到浏览器 CORS 的限制。

只有当浏览器代码直接跨域访问 MCP Server 时，CORS 策略才会真正参与浏览器的跨域检查。

因此是否需要配置 CORS，应该根据实际部署方式和调用链决定，而不是因为使用了 MCP 就必须配置。

### 调用 Open-Meteo 获取天气数据

`WeatherTools` 最终仍然调用现有的业务能力，我们这里模仿天气查询的业务场景，你在实际项目中，
以把这里的天气查询替换成自己的业务。

Open-Meteo 不需要额外申请 API Key，因此比较适合作为这个 MCP 示例的数据来源。

这里需要强调的是，**Open-Meteo 并不是 MCP 的一部分**。

它只是这个示例背后的业务数据源。


## ASP.NET Core MCP Client

客户端首先从 MCP Server 获取可用工具。需要注意的是，访问受保护的 MCP 服务和调用普通 Web API 一样，也要带上 Token，否则无法获取和调用工具。

### 创建 McpClient

客户端先请求 `/auth/token` 获取 JWT，然后把认证信息放入 `AdditionalHeaders`：

```csharp
private async Task<McpClient> CreateMcpClientAsync(
	CancellationToken cancellationToken)
{
	var additionalHeaders = await GetAuthenticationHeadersAsync(cancellationToken);
	var transport = new HttpClientTransport(
		new HttpClientTransportOptions
		{
			Endpoint = new Uri(mcpServerOptions.Value.Endpoint),
			TransportMode = HttpTransportMode.AutoDetect,
			Name = "ASP.NET Core MCP web client",
			AdditionalHeaders = additionalHeaders
		},
		loggerFactory);

	return await McpClient.CreateAsync(
		transport,
		loggerFactory: loggerFactory,
		cancellationToken: cancellationToken);
}
```

### 获取MCP Tools 并转交给 Agent

接下来的思路和前面的例子一样：先通过 `ListToolsAsync` 获取 MCP Tools，再把它们转换成 Agent 可以调用的 `AITool`。

```csharp
public async Task<McpChatResult> AskAsync(
	string message,
	CancellationToken cancellationToken)
{
	ArgumentException.ThrowIfNullOrWhiteSpace(message);

	if (!Uri.TryCreate(
		foundryOptions.ProjectEndpoint,
		UriKind.Absolute,
		out var projectEndpoint))
	{
		throw new InvalidOperationException(
			"未配置有效的 FOUNDRY_PROJECT_ENDPOINT。请设置环境变量后重启应用。");
	}

	await using var mcpClient = await CreateMcpClientAsync(cancellationToken);
	var mcpTools = await mcpClient.ListToolsAsync(
		cancellationToken: cancellationToken);

	List<AITool> tools = [.. mcpTools.Cast<AITool>()];

	AIProjectClient projectClient = new(
		projectEndpoint,
		new DefaultAzureCredential());

	AIAgent agent = projectClient.AsAIAgent(
		model: foundryOptions.Model,
		instructions: "你是一个乐于助人的助手。根据用户的自然语言请求，选择并调用可用的 MCP 工具；使用工具结果准确回答，并明确说明工具无法完成的请求。",
		name: "WebMcpAgent",
		tools: tools);

	AgentResponse response = await agent.RunAsync(
		message,
		cancellationToken: cancellationToken);

	return new McpChatResult(
		response.ToString(),
		mcpTools.Select(tool => tool.Name).ToArray());
}
```

这里使用的是 Microsoft Foundry 上的模型。你也可以替换成其他支持工具调用的模型，例如 DeepSeek，具体接入方式可以参考前面的文章。

## 运行效果


## 总结

整体思路是我们把现有 ASP.NET Core 业务能力封装成 MCP Tool，再交给大模型或 Agent 根据自然语言自动选择和调用。
这样既能复用原来的业务逻辑，也能继续使用现有的认证和授权机制。


