# CompactionProvider 对话压缩示例

这个示例使用 `SummarizationCompactionStrategy` 总结较早的对话，并保留最近的消息，
减少发送给模型的上下文。对话使用同一个会话；终端中的 `[压缩摘要]` 表示模型返回的摘要。
示例关闭了 Responses API 的服务端响应存储，让 Agent 在本地管理对话历史，
这样 `CompactionProvider` 才能在调用模型前压缩上下文。

为了方便观察，示例在消息数超过 6 条时触发压缩，并至少保留最近 2 个消息组。
压缩目标是将待保留的原始消息降至 4 条以内，再加入一条摘要；保留限制可能影响实际结果。
这只是演示配置，实际使用时应根据上下文长度调整。摘要可能遗漏信息，不能代替完整记录；
原始会话历史的消息数量也不一定等于压缩后发送给模型的消息数量。

## 运行

安装 .NET 11，准备 Foundry 项目和模型部署，并通过 Azure CLI 或其他
`DefaultAzureCredential` 支持的方式登录。然后在 PowerShell 中设置：

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<资源名>.services.ai.azure.com/api/projects/<项目名>"
$env:FOUNDRY_MODEL = "<模型部署名称>"
dotnet run
```

示例使用同一个模型回答问题并生成摘要。两者都需要联网调用模型，摘要调用也会产生费用。
