using System.ClientModel;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace _21_DeepSeekSkillsDemo
{
    internal class DeepSeekChatClient : IChatClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _model;
        private readonly ILogger _logger;

        public DeepSeekChatClient(string apiKey, string model = "deepseek-chat", HttpClient? httpClient = null, ILogger? logger = null)
        {
            _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
            _model = model;
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            _logger = logger ?? NullLogger.Instance;
        }

        public object? ServiceProvider => null;

        public async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // 构建工具列表
            var tools = new List<DeepSeekTool>();
            if (options?.Tools != null)
            {
                foreach (var tool in options.Tools)
                {
                    // 从AITool中提取函数信息
                    var functionName = tool.Name ?? GetFunctionNameFromTool(tool);
                    var functionDescription = GetFunctionDescriptionFromTool(tool);
                    var parameters = GetFunctionParametersFromTool(tool);

                    tools.Add(new DeepSeekTool
                    {
                        Type = "function",
                        Function = new DeepSeekFunction
                        {
                            Name = functionName,
                            Description = functionDescription,
                            Parameters = parameters
                        }
                    });
                }
            }

            var request = new DeepSeekRequest
            {
                Model = _model,
                Messages = messages.Select(m => new DeepSeekMessage
                {
                    Role = m.Role.ToString().ToLowerInvariant(),
                    Content = string.Join(" ", m.Contents.OfType<TextContent>().Select(c => c.Text))
                }).ToList(),
                Stream = false,
                Tools = tools.Count > 0 ? tools : null
            };

            var json = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            });
            _logger.LogDebug("DeepSeek Request: {Request}", json);

            using var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://api.deepseek.com/v1/chat/completions", content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                throw new Exception($"DeepSeek API error: {response.StatusCode} - {errorContent}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogDebug("DeepSeek Response: {Response}", responseJson);

            var result = JsonSerializer.Deserialize<DeepSeekResponse>(responseJson, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (result?.Choices == null || result.Choices.Count == 0)
            {
                return new ChatResponse(new List<ChatMessage>());
            }

            var choice = result.Choices[0];
            var responseMessage = new ChatMessage(ChatRole.Assistant, choice.Message.Content ?? "");

            // 处理工具调用
            if (choice.Message.ToolCalls != null && choice.Message.ToolCalls.Count > 0)
            {
                var contents = new List<AIContent>();
                foreach (var toolCall in choice.Message.ToolCalls)
                {
                    var args = string.IsNullOrEmpty(toolCall.Function.Name)
                        ? new Dictionary<string, object?>()
                        : JsonSerializer.Deserialize<Dictionary<string, object?>>(toolCall.Function.Name) ?? new Dictionary<string, object?>();

                    contents.Add(new FunctionCallContent(
                        toolCall.Function.Name,
                        toolCall.Id,
                        args
                    ));
                }
                responseMessage.Contents = contents;
            }

            return new ChatResponse(new[] { responseMessage });
        }

        public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            // 简化实现，仅演示基本功能
            var response = await GetResponseAsync(messages, options, cancellationToken);
            foreach (var message in response.Messages)
            {
                yield return new ChatResponseUpdate(message, ChatRole.Assistant);
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
        }

        // 辅助方法：从AITool提取函数名称
        private string GetFunctionNameFromTool(AITool tool)
        {
            // 尝试通过反射获取FunctionName
            var property = tool.GetType().GetProperty("FunctionName");
            if (property != null)
            {
                var value = property.GetValue(tool);
                if (value != null)
                    return value.ToString() ?? "unknown_function";
            }

            // 如果没有FunctionName属性，尝试从ToString获取
            return tool.ToString() ?? "unknown_function";
        }

        // 辅助方法：从AITool提取函数描述
        private string? GetFunctionDescriptionFromTool(AITool tool)
        {
            // 尝试通过反射获取FunctionDescription
            var property = tool.GetType().GetProperty("FunctionDescription");
            if (property != null)
            {
                var value = property.GetValue(tool);
                return value?.ToString();
            }

            // 尝试获取Description属性
            property = tool.GetType().GetProperty("Description");
            if (property != null)
            {
                var value = property.GetValue(tool);
                return value?.ToString();
            }

            return null;
        }

        // 辅助方法：从AITool提取函数参数
        private object? GetFunctionParametersFromTool(AITool tool)
        {
            // 尝试通过反射获取FunctionParameters
            var property = tool.GetType().GetProperty("FunctionParameters");
            if (property != null)
            {
                var value = property.GetValue(tool);
                if (value != null)
                {
                    // 如果已经是一个对象，直接返回
                    if (value is string jsonString)
                    {
                        try
                        {
                            return JsonSerializer.Deserialize<object>(jsonString);
                        }
                        catch
                        {
                            return value;
                        }
                    }
                    return value;
                }
            }

            // 尝试获取Parameters属性
            property = tool.GetType().GetProperty("Parameters");
            if (property != null)
            {
                var value = property.GetValue(tool);
                return value;
            }

            return null;
        }

        public object? GetService(Type serviceType, object? serviceKey = null)
        {
            throw new NotImplementedException();
        }

        // DeepSeek API 数据模型
        private class DeepSeekRequest
        {
            public string Model { get; set; } = "";
            public List<DeepSeekMessage> Messages { get; set; } = new();
            public bool Stream { get; set; }
            public List<DeepSeekTool>? Tools { get; set; }
        }

        private class DeepSeekMessage
        {
            public string Role { get; set; } = "";
            public string Content { get; set; } = "";
            public List<DeepSeekToolCall>? ToolCalls { get; set; }
        }

        private class DeepSeekTool
        {
            public string Type { get; set; } = "function";
            public DeepSeekFunction Function { get; set; } = new();
        }

        private class DeepSeekFunction
        {
            public string Name { get; set; } = "";
            public string? Description { get; set; }
            public object? Parameters { get; set; }
        }

        private class DeepSeekToolCall
        {
            public string Id { get; set; } = "";
            public DeepSeekFunction Function { get; set; } = new();
        }

        private class DeepSeekResponse
        {
            public List<DeepSeekChoice> Choices { get; set; } = new();
        }

        private class DeepSeekChoice
        {
            public DeepSeekResponseMessage Message { get; set; } = new();
        }

        private class DeepSeekResponseMessage
        {
            public string? Content { get; set; }
            public List<DeepSeekToolCall>? ToolCalls { get; set; }
        }
    }
}
