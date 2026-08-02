using _03_Mem0MemoryProvider;

public class Mem0ProviderState
{
    public Mem0ProviderScope StorageScope { get; }
    public Mem0ProviderScope SearchScope { get; }

    public Mem0ProviderState(Mem0ProviderScope storageScope, Mem0ProviderScope? searchScope = null)
    {
        ArgumentNullException.ThrowIfNull(storageScope);

        StorageScope = storageScope;
        SearchScope = searchScope ?? storageScope;
    }
}