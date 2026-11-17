# AgentSkillsProvider 示例

五个项目分别演示 ClassBased、CodeDefined、DI、FileBased 和 Mixed 技能。
每个项目都包含自己的 `SkillAgentFactory.cs`，方便独立查看和运行。

通过环境变量 `AGENT_PROVIDER` 选择 `Foundry` 或 `DeepSeek`，不设置时使用 Foundry。

- Foundry：设置 `AZURE_OPENAI_ENDPOINT` 为 Foundry 项目终结点，设置
  `AZURE_OPENAI_DEPLOYMENT_NAME` 为模型部署名称；未设置部署名称时默认为 `gpt-5.4-mini`。
- DeepSeek：设置 `DEEPSEEK_API_KEY`，可选设置 `DEEPSEEK_ENDPOINT` 和
  `DEEPSEEK_MODEL`。ClassBased 和 Mixed 默认使用 `deepseek-chat`，其余项目默认使用
  `deepseek-v4-pro`。

例如在 PowerShell 中：

```powershell
$env:AGENT_PROVIDER = "DeepSeek"
$env:DEEPSEEK_API_KEY = "<API key>"
dotnet run --project .\AIContextProvider.AgentSkillsProvider.ClassBased
```

切换为 Foundry 时设置 `$env:AGENT_PROVIDER = "Foundry"`，并配置上述 Foundry 环境变量。
