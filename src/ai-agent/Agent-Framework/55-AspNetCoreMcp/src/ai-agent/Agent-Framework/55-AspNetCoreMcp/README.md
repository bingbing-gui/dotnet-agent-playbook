# 把 ASP.NET Core Web API 暴露为 MCP 工具

前面我们介绍过 MCP 的调用方式，并使用 `McpClient` 调用了 Microsoft Learn 提供的 MCP 服务，把 Microsoft Learn提供的MCP Tools转换成本地Agent可调用的工具并实现自然语言的交互。
那么，我们能不能把现有的 ASP.NET Core Web API 转换成 MCP 工具，让外部大模型或 Agent 通过自然语言来调用呢？答案当然是可以的。

这节我们就来分享一下，如何把自己的 ASP.NET Core 项目作为 MCP 工具暴露出去。

如果 API 本身是公开的，可以不做鉴权；如果 API 需要认证，那么调用 MCP 服务时同样需要携带 Token。这里我们使用 JWT 完成认证和授权。

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

这里主要看 MCP Server 的实现。项目使用 ASP.NET Core Web API，并安装了下面几个包：

- `ModelContextProtocol.AspNetCore`：MCP 官方 C# SDK 的 ASP.NET Core 支持
- `Microsoft.AspNetCore.Authentication.JwtBearer`：JWT 认证和授权
- `Microsoft.AspNetCore.OpenApi`：OpenAPI 支持

服务端主要处理三件事：

1. 把天气查询能力暴露为 MCP Tool。
2. 使用 JWT 对 MCP 请求进行认证。
3. 配置跨域访问，允许 ASP.NET Core MCP Client 调用。

### 注册 MCP 服务

打开 `AspNetCoreMcpServer/Program.cs`，把 MCP 服务添加到依赖注入容器：

```csharp
builder.Services.AddMcpServer()
	.WithHttpTransport(options =>
		options.SessionMode = HttpServerSessionMode.Stateless)
	.WithTools<WeatherTools>();
```

`HttpServerSessionMode.Stateless` 表示使用无状态模式。这个示例不需要保存 MCP 会话，也不会发送“服务器到客户端”的请求，因此使用无状态模式更合适。

`WithTools<WeatherTools>()` 会把 `WeatherTools` 中定义的工具注册到 MCP Server。

### 添加 MCP 路由

接着添加 MCP 路由，并要求调用方通过认证：

```csharp
app.MapMcp()
	.RequireAuthorization()
	.RequireCors("McpBrowserClient");
```

由于调用方也是一个 ASP.NET Core Web 应用，所以这里同时启用了名为 `McpBrowserClient` 的 CORS 策略，只允许示例客户端地址访问。

### 添加 CORS 策略

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("McpBrowserClient", policy =>
    {
        policy.WithOrigins("http://localhost:5164", "https://localhost:7133")
            .WithMethods("POST")
            .WithHeaders(HeaderNames.ContentType, HeaderNames.Authorization, "MCP-Protocol-Version")
            .WithExposedHeaders(HeaderNames.WWWAuthenticate);
    });
});
```

### 定义 WeatherTools

接下来看一下 `WeatherTools`。我们需要在类上添加 `[McpServerToolType]`，在需要暴露的方法上添加 `[McpServerTool]`：

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

`[McpServerTool]` 表示这是一个可以被 MCP Client 调用的工具，`Description` 用来说明工具和参数的作用。大模型会参考这些描述，判断什么时候调用工具，以及应该传入什么参数。

具体的天气查询逻辑这里不过多展开。示例使用 `HttpClient` 调用 [Open-Meteo](https://open-meteo.com/)，先根据中国城市名称查询经纬度，再获取实时天气和未来三天的天气预报，不需要额外申请 API Key。

### JWT 认证

服务端通过 `/auth/token` 提供 Token 获取接口。客户端使用配置中的用户名和密码请求 Token，后续访问 MCP 服务时再通过 `Authorization: Bearer <token>` 携带凭据。

如何你是一名ASP.NET Core的开发人员对下面代码应该非常熟悉，演示用的账号和固定签名密钥放在 `AspNetCoreMcpServer/appsettings.json` 中：

```csharp
builder.Services.AddAuthentication(options =>
{
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidAudience = "AspNetCoreMcpServer",
        ValidIssuer = "everyone",
        IssuerSigningKey = signingKey,
        NameClaimType = ClaimTypes.Name,
        RoleClaimType = "roles"
    };
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var name = context.Principal?.Identity?.Name ?? "unknown";
            var email = context.Principal?.FindFirstValue("preferred_username") ?? "unknown";
            Console.WriteLine($"Token validated for: {name} ({email})");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Authentication failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine("Challenging client to provide a bearer token");
            return Task.CompletedTask;
        }
    };
});
```

```json
{
  "Auth": {
	"Username": "admin",
	"Password": "ChangeMe123!",
	"SigningKey": "VGhpc0lzQVN1cGVyU2VjcmV0S2V5Rm9yRGVtbzEyMzQ1Ng=="
  }
}
```

这里主要是为了方便本地演示。正式项目中需要替换默认账号、密码和签名密钥，并根据实际业务接入完整的身份认证方案。

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

### 把 MCP 工具交给 Agent

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

客户端页面本身提供了自然语言输入框。比如输入：

> 明天杭州会下雨吗？早上出门需要带伞吗？

请求会发送到 `/api/mcp/chat`。Agent 根据问题自动选择 MCP 工具、查询天气，然后结合工具返回的数据给出回答，不需要用户手动选择工具或填写 JSON 参数。

## 配置项目

MCP Client 在 `appsettings.json` 中配置服务端地址和演示账号：

```json
{
  "McpServer": {
	"Endpoint": "http://localhost:7049",
	"TokenEndpoint": "http://localhost:7049/auth/token",
	"Username": "admin",
	"Password": "ChangeMe123!"
  }
}
```

另外还需要配置 Foundry 项目地址和模型：

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "你的 Foundry Project Endpoint"
$env:FOUNDRY_MODEL = "你的模型部署名称"
```

如果没有设置 `FOUNDRY_MODEL`，示例默认使用 `gpt-5.4-mini`。客户端通过 `DefaultAzureCredential` 访问 Foundry，因此本地运行前还要确保当前开发环境已经具备对应的 Azure 身份和访问权限。

## 运行项目

先启动 MCP Server：

```powershell
dotnet run --project .\AspNetCoreMcpServer\AspNetCoreMcpServer.csproj --launch-profile http
```

再启动 MCP Client：

```powershell
dotnet run --project .\AspNetCoreMcpClient\AspNetCoreMcpClient.csproj --launch-profile http
```

启动后访问：

```text
http://localhost:5164
```

在页面中输入自然语言问题，客户端会完成下面这条调用链：

```text
浏览器自然语言输入
	-> ASP.NET Core MCP Client
	-> Microsoft Foundry Agent
	-> 携带 JWT 连接 MCP Server
	-> 调用天气 MCP Tool
	-> Open-Meteo
	-> Agent 整理结果并返回页面
```



## 总结

整体思路就是这样。我们把现有 ASP.NET Core 业务能力封装成 MCP Tool，再交给大模型或 Agent 根据自然语言自动选择和调用。
这样既能复用原来的业务逻辑，也能继续使用现有的认证和授权机制。


