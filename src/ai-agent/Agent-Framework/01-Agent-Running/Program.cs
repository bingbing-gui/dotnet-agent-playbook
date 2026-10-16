// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with Azure OpenAI as the backend.

using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Identity.Client;
using OpenAI;
using OpenAI.Chat;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";

# region Azure CLI 认证
// ============================================================
// 方式一：通过 AzureOpenAIClient / ChatClient 创建 Agent
// ============================================================
AIAgent openAIAgent = new AzureOpenAIClient(
    new Uri(endpoint),
    new AzureCliCredential())
    .GetChatClient(deploymentName)
    .AsIChatClient()
    .AsAIAgent(instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");


Console.WriteLine(string.Concat(Enumerable.Repeat('-', 120)));
Console.WriteLine(await openAIAgent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。"));
// ============================================================
// 方式二：通过 AIProjectClient 创建 Agent
// 关联包：Azure.Identity、Microsoft.Agents.AI.Foundry
// ============================================================
AIAgent foundryAgent = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential())
    .AsAIAgent(model: deploymentName, instructions: "你是一位江湖说书人，擅长用幽默、接地气的方式讲笑话和故事。", name: "Joker");

Console.WriteLine(string.Concat(Enumerable.Repeat('-', 120)));
Console.WriteLine(await foundryAgent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。"));


AgentResponse response = await foundryAgent.RunAsync("给我讲一个发生在茶馆里的段子，轻松一点的那种。");

foreach (Microsoft.Extensions.AI.ChatMessage message  in response.Messages)
{
    foreach (AIContent content in message.Contents)
    {
        switch (content)
        {
            case TextContent text:
                Console.WriteLine(text.Text);
                break;

            case DataContent data
                when data.MediaType.StartsWith("image/"):
                // 保存二进制数据，或者交给 UI 显示
                break;

            case UriContent uri
                when uri.MediaType?.StartsWith("image/") == true:
                // 在 UI 中加载图片 URI
                break;
        }
    }
}

#endregion


#region 使用服务主体 (Service Principal) 认证
// 1. 定义 DefaultAzureCredential 的选项
//var options = new DefaultAzureCredentialOptions
//{
//    ExcludeEnvironmentCredential = true,     // 不用系统环境变量
//    ExcludeManagedIdentityCredential = false, // 允许托管身份
//    ExcludeAzureCliCredential = false,        // 允许 CLI 登录
//    ExcludeVisualStudioCodeCredential = true
//};

//// 2. 手动定义一个服务主体凭证
//var environmentCredential = new ClientSecretCredential(
//    tenantId: "",
//    clientId: "",
//    clientSecret: ""
//);

//// 3. 创建凭证链（优先使用 environmentCredential）
//var credentialChain = new ChainedTokenCredential(
//    environmentCredential,
//    new DefaultAzureCredential(options)
//);
// 4. 创建 AI Agent（注意这里指向OpenAI的URL地址，这个和CLI的认证是不一样的）
//var agent = new AzureOpenAIClient(
//    new Uri("https://maf.openai.azure.com/"),
//    credentialChain)
//    .GetChatClient(deploymentName)
//    .CreateAIAgent(instructions: "你是一个诗人", name: "Joker");

//Console.WriteLine(await agent.RunAsync("请帮我写一首诗。希望类型"));
//Console.ReadLine();

#endregion


#region 使用 API 密钥认证
//AIAgent agent = new AzureOpenAIClient(
//    new Uri(endpoint),
//    new System.ClientModel.ApiKeyCredential(""))
//    .GetChatClient(deploymentName)
//    .AsAIAgent(instructions: "你是一个诗人", name: "Joker");
//    //.CreateAIAgent(instructions: "你是一个诗人", name: "Joker");已废弃

//// Invoke the agent and output the text result.
//Console.WriteLine(await agent.RunAsync("请帮我写一首关于爱情的诗。"));

//Console.ReadLine();
#endregion

// 流式方式返回结果
//await foreach (var update in agent.RunStreamingAsync("请帮我写一首诗。"))
//{
//    Console.WriteLine(update);
//}