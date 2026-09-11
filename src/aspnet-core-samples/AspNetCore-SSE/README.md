# AspNetCore-SSE

一个用于演示 ASP.NET Core 10 Server-Sent Events (SSE) 的最小可运行项目。

## 技术栈

- .NET 10 / ASP.NET Core Minimal API
- `TypedResults.ServerSentEvents()`
- `SseItem<T>`
- JavaScript `EventSource`
- Bootstrap 5.3.8（官方推荐的 jsDelivr CDN 引用）

## 演示内容

- 浏览器连接 / 断开 SSE
- 服务端每秒推送一条 JSON 消息
- 自定义 SSE Event Type：`server-message`
- `EventId`
- `ReconnectionInterval`
- 浏览器自动重连
- `Last-Event-ID`
- 每次 HTTP SSE 连接使用不同 ConnectionId，便于观察重连

## 运行环境

需要安装 .NET 10 SDK。

查看版本：

```bash
dotnet --version
```

## 运行

在项目目录执行：

```bash
dotnet run
```

默认 HTTP 地址：

```text
http://localhost:5088
```

浏览器打开该地址，点击“连接 SSE”。

## 自动重连演示

为了方便观察重连，服务端每个 HTTP SSE 连接只发送 10 条消息，然后主动结束连接。

浏览器原生 `EventSource` 会根据 SSE `retry` 值自动重新连接，并发送：

```http
Last-Event-ID: 10
```

服务端读取该 Header 后，从下一条 Event ID 继续发送。

> 注意：这个项目只是重连机制演示，并不是生产级可靠消息补偿。生产环境如果要求断线期间消息不丢失，需要把事件存储到 Redis、数据库、Kafka 等持久化/消息系统中，然后根据 `Last-Event-ID` 补发。
