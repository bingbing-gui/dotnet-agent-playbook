// Copyright (c) Microsoft. All rights reserved.

// This sample shows how to use OpenAPI Tools with AI Agents.

using Azure.AI.Projects;
using Azure.AI.Projects.Agents;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
using System.Text;

Console.InputEncoding = Encoding.UTF8;
Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

string endpoint = Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set.");
string deploymentName = Environment.GetEnvironmentVariable("FOUNDRY_MODEL") ?? "gpt-5.4-mini";

const string AgentInstructions = "你是一位乐于助人的助手，能够利用 Frankfurter API 获取最新的货币汇率。请务必调用 API 获取实时数据，而不要进行推测。";

AIProjectClient aiProjectClient = new(new Uri(endpoint), new DefaultAzureCredential());


#pragma warning disable OPENAI001 
AITool openApiTool = FoundryAITool.CreateOpenApiTool(CreateOpenAPIFunctionDefinition());
#pragma warning restore OPENAI001 

AIAgent agent = aiProjectClient.AsAIAgent(deploymentName,
    instructions: AgentInstructions,
    name: "OpenAPIToolsAgent",
    tools: [openApiTool]);

// Run the agent with a question about EUR exchange rates
Console.WriteLine(await agent.RunAsync("最新的美元(USD)对欧元(EUR)汇率是多少？最新的美元(USD)和英镑(GBP)的汇率是多少？最新的美元(USD)对日元(JPY)的汇率是多少？最新的美元(USD)对人民币(CNY)的汇率是多少？"));

OpenApiFunctionDefinition CreateOpenAPIFunctionDefinition()
{
    // OpenAPI spec for Frankfurter — a free, no-auth exchange rate API backed by ECB data.
    // See https://www.frankfurter.dev/ for documentation.
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

    return new(
        "get_exchange_rates",
        BinaryData.FromString(FrankfurterOpenApiSpec),
        new OpenAPIAnonymousAuthenticationDetails())
    {
        Description = "获取来自欧洲中央银行的实时货币汇率，通过 Frankfurter API 提供"
    };
}