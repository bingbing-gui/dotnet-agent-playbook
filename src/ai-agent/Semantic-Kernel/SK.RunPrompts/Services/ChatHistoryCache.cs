using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel.ChatCompletion;

public interface IChatHistoryCache
{
    ChatHistory GetOrCreate(string connectionId);
}

public class ChatHistoryCache : IChatHistoryCache
{
    private readonly IMemoryCache _cache;

    public ChatHistoryCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public ChatHistory GetOrCreate(string connectionId)
    {
        return _cache.GetOrCreate(connectionId, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(30); // 可配置
            return new ChatHistory();
        });
    }
}
