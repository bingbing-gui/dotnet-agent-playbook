using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using OpenAI.Embeddings;
using SK.VectorStores.Models;

namespace SK.VectorStores.Services
{
    public sealed class AzureOpenAIEmbeddingService : IEmbeddingService
    {
        private readonly AzureOpenAIClient _client;

        private readonly string _deploymentName;
        private readonly int _expectedDim;

        public AzureOpenAIEmbeddingService(
            AzureOpenAIClient client,
            IOptions<AzureEmbeddingOptions> options)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            var opt = options?.Value ?? throw new ArgumentNullException(nameof(options));

            if (string.IsNullOrWhiteSpace(opt.DeploymentName))
                throw new ArgumentException("Embedding deployment name is required.", nameof(options));

            if (opt.ExpectedDimension <= 0)
                throw new ArgumentException("ExpectedDimension must be a positive integer.", nameof(options));

            _deploymentName = opt.DeploymentName;
            _expectedDim = opt.ExpectedDimension;
        }

        public async Task<ReadOnlyMemory<float>> CreateAsync(string text, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentException("Text to embed cannot be null or empty.", nameof(text));

            try
            {
                EmbeddingClient embClient = _client.GetEmbeddingClient(_deploymentName);


                var clientResult = await embClient.GenerateEmbeddingAsync(text, cancellationToken: ct);


                var vec = clientResult.Value.ToFloats();

                // 维度校验（强烈推荐，避免和 pgvector 列定义不一致）
                if (vec.Length != _expectedDim)
                {
                    throw new InvalidOperationException(
                        $"Embedding dimension mismatch. Expected: {_expectedDim}, Actual: {vec.Length}. " +
                        $"Make sure your model and database column (VECTOR({_expectedDim})) match.");
                }

                return vec;
            }
            catch (RequestFailedException ex)
            {
                // Azure SDK 的标准异常，抛出更友好的信息
                throw new InvalidOperationException($"Failed to generate embedding from Azure OpenAI: {ex.Message}", ex);
            }
        }
    }
}
