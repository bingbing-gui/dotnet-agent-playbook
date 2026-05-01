// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to create and use a simple AI agent with Azure OpenAI as the backend.

using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI;
using System.Text;


Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

var endpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? throw new InvalidOperationException("AZURE_OPENAI_ENDPOINT is not set.");
var deploymentName = Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT_NAME") ?? "gpt-4o-mini";




# region Azure CLI 认证
//AIAgent agent = new AzureOpenAIClient(
//    new Uri(endpoint),
//    new AzureCliCredential()
//    )
//    .GetChatClient(deploymentName)
//    .CreateAIAgent(instructions: "你是一个诗人", name: "Joker");

//// Invoke the agent and output the text result.
//Console.WriteLine(await agent.RunAsync("请帮我写一首诗。"));

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

AIAgent agent = new AzureOpenAIClient(
    new Uri(endpoint),
    new System.ClientModel.ApiKeyCredential(""))
    .GetChatClient(deploymentName)
    .CreateAIAgent(instructions: "你是一个诗人", name: "Joker");

// Invoke the agent and output the text result.
Console.WriteLine(await agent.RunAsync("请帮我写一首关于爱情的诗。"));

Console.ReadLine();
#endregion

// 流式方式返回结果
//await foreach (var update in agent.RunStreamingAsync("请帮我写一首诗。"))
//{
//    Console.WriteLine(update);
//}