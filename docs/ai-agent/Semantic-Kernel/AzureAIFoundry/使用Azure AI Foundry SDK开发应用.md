# 使用Azure AI Foundry SDK开发应用

## 什么是 Azure AI Foundry SDK？

Azure AI Foundry SDK 提供一组包和服务，帮助开发者在 Azure AI Foundry 项目中访问资源并与模型交互，包括向生成式 AI 模型发送提示及处理响应。该 SDK 支持 Python 和 C# .NET，部分功能仍在预览中。

## 安装 SDK 包

使用 Azure AI Foundry 项目的核心库 Azure AI Projects 访问并管理项目。在 C# 中使用以下命令安装：

```bash
dotnet add package Azure.AI.Projects --prerelease
```

## 使用 SDK 连接到项目

每个项目都有唯一的连接字符串，可在 Azure AI Foundry 门户中获得。通过此字符串创建 AIProjectClient 对象来访问项目资源：

```csharp
using Azure.Identity;
using Azure.AI.Projects;

var connectionString = "<region>.api.azureml.ms;<project_id>;<hub_name>;<project_name>";
var projectClient = new AIProjectClient(connectionString, new DefaultAzureCredential());
```

## 创建聊天客户端

如需与部署在 Azure AI Foundry 项目中的生成式 AI 模型互动，可使用以下部署方式：

1. Azure AI 模型推理服务
2. Azure OpenAI 服务
3. 无服务器 API
4. 托管计算

在 Azure AI Foundry 中开启相应选项即可将模型部署到所需终结点。
