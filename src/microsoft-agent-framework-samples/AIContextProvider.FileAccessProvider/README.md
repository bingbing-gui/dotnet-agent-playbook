# AIContextProvider.FileAccessProvider

这个示例演示如何通过 `ChatClientAgentOptions.AIContextProviders` 直接配置
`FileAccessProvider`，让 Agent 列出并读取本地 CSV 文件，然后计算每种商品的
销售额和总销售额。

## 安全边界

- `FileSystemAgentFileStore` 的根目录是应用输出目录下的 `working` 文件夹。
- `sales.csv` 会在构建时复制到 `working\sales.csv`。
- `DisableWriteTools = true`，Agent 不会获得写入、删除或替换文件的工具。
- `DisableReadOnlyToolApproval = true`，只读演示可以无人值守运行。

因此，Agent 的文件工具只能访问 `working` 文件夹中的内容，不能读取项目或机器上
的其他文件。

`FileAccessProviderOptions` 的这些配置来自当前预览版 API，所以该示例使用
`Microsoft.Agents.AI.Foundry 1.26.0-preview.261023.1`。

## 配置

```powershell
$env:FOUNDRY_PROJECT_ENDPOINT = "https://<resource>.services.ai.azure.com/api/projects/<project>"
$env:FOUNDRY_MODEL = "gpt-5.4-mini"
```

`FOUNDRY_MODEL` 未设置时默认使用 `gpt-5.4-mini`。

## 运行

```powershell
dotnet run --project .\AIContextProvider.FileAccessProvider.csproj
```

Agent 会先调用文件列表和读取工具，再根据 `sales.csv` 返回以下汇总：

| 商品 | 销售额 |
|---|---:|
| Widget | 75.00 |
| Gadget | 75.00 |
| Cable | 40.00 |
| **总计** | **190.00** |
