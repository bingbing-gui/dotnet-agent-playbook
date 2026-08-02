using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Mem0;
using Microsoft.Extensions.AI;
using System.Net.Http.Json;
using System.Text.Json;

namespace _03_Mem0MemoryProvider;

public sealed class Mem0MemoryProvider : MessageAIContextProvider, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ProviderSessionState<Mem0ProviderState> _sessionState;
    private bool _disposed;

    public Mem0MemoryProvider(
        string apiKey,
        Func<AgentSession?, Mem0ProviderState> stateInitializer)
    {
        ArgumentNullException.ThrowIfNull(stateInitializer);

        _sessionState = new ProviderSessionState<Mem0ProviderState>(
            stateInitializer,
            stateKey: GetType().Name);

        _httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://api.mem0.ai/")
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Token {apiKey}");
    }

    protected override async ValueTask<IEnumerable<ChatMessage>> ProvideMessagesAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        var userQuery = context.RequestMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text;
        if (string.IsNullOrWhiteSpace(userQuery))
        {
            return [];
        }

        var searchPayload = new Dictionary<string, object>
        {
            ["query"] = userQuery
        };

        var state = _sessionState.GetOrInitializeState(context.Session);
        ApplySearchScope(searchPayload, state.SearchScope);

        var response = await _httpClient.PostAsJsonAsync("v3/memories/search/", searchPayload, cancellationToken);
        response.EnsureSuccessStatusCode();

        var searchResults = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
        var retrievedMemories = new List<string>();

        if (searchResults.TryGetProperty("results", out var resultsArray) && resultsArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in resultsArray.EnumerateArray())
            {
                if (item.TryGetProperty("memory", out var memoryElement) && memoryElement.ValueKind == JsonValueKind.String)
                {
                    retrievedMemories.Add(memoryElement.GetString()!);
                }
            }
        }

        if (retrievedMemories.Count == 0)
        {
            return [];
        }
        var memoryContext = "Relevant information from previous conversations:\n" +
                            string.Join("\n- ", retrievedMemories);

        return [new ChatMessage(ChatRole.System, memoryContext)];
    }

    protected override async ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
    {
        var lastUserMessage = context.RequestMessages.LastOrDefault(m => m.Role == ChatRole.User)?.Text;
        var assistantResponse = context.ResponseMessages?.FirstOrDefault()?.Text;

        if (string.IsNullOrWhiteSpace(lastUserMessage) || string.IsNullOrWhiteSpace(assistantResponse))
        {
            return;
        }

        var addPayload = new Dictionary<string, object>
        {
            ["messages"] = new[]
            {
                new { role = "user", content = lastUserMessage },
                new { role = "assistant", content = assistantResponse }
            }
        };

        var state = _sessionState.GetOrInitializeState(context.Session);
        ApplyStorageScope(addPayload, state.StorageScope);

        var response = await _httpClient.PostAsJsonAsync("v3/memories/add/", addPayload, cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private static void ApplySearchScope(Dictionary<string, object> payload, Mem0ProviderScope scope)
    {
        if (scope == null) return;

        var filters = new Dictionary<string, string>();

        if (!string.IsNullOrWhiteSpace(scope.UserId)) filters["user_id"] = scope.UserId;
        if (!string.IsNullOrWhiteSpace(scope.AppId)) filters["app_id"] = scope.AppId;
        if (!string.IsNullOrWhiteSpace(scope.AgentId)) filters["agent_id"] = scope.AgentId;
        if (!string.IsNullOrWhiteSpace(scope.RunId)) filters["run_id"] = scope.RunId;

        if (filters.Count > 0)
        {
            payload["filters"] = filters;
        }
    }

    private static void ApplyStorageScope(Dictionary<string, object> payload, Mem0ProviderScope scope)
    {
        if (scope == null) return;

        if (!string.IsNullOrWhiteSpace(scope.UserId)) payload["user_id"] = scope.UserId;
        if (!string.IsNullOrWhiteSpace(scope.AppId)) payload["app_id"] = scope.AppId;
        if (!string.IsNullOrWhiteSpace(scope.AgentId)) payload["agent_id"] = scope.AgentId;
        if (!string.IsNullOrWhiteSpace(scope.RunId)) payload["run_id"] = scope.RunId;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _httpClient.Dispose();
        _disposed = true;
    }
}