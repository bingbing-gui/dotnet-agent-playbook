namespace SK.TextEmbeddingGeneration.Services
{
    public interface IEmbeddingService
    {
        Task<ReadOnlyMemory<float>> CreateAsync(string text, CancellationToken ct = default);
    }
}
