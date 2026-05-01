// /Application/Services/HotelService.cs
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.PgVector;
using Npgsql;
using Dapper;
using SK.VectorStores.Models;
using SK.VectorStores.Services;

public sealed class HotelService : IHotelService
{
    private readonly PostgresVectorStore _vectorStore;
    private string connectionString;
    private readonly IEmbeddingService _emb;
    public HotelService(PostgresVectorStore vectorStore, IEmbeddingService emb, IConfiguration configuration)
    {
        _vectorStore = vectorStore;
        _emb = emb;
        connectionString = configuration.GetConnectionString("Postgres");
        
    }

    // 获取集合（表名示例为 "Hotels"）
    public PostgresCollection<int, Hotel> GetCollection()
        => _vectorStore.GetCollection<int, Hotel>("hotels");



    public async Task UpsertAsync(Hotel hotel, CancellationToken ct = default)
    {
        if (!string.IsNullOrWhiteSpace(hotel.Description))
        {
            var vec = await _emb.CreateAsync(hotel.Description);
            hotel.DescriptionEmbedding = vec;
        }
        var col = GetCollection();
        await col.UpsertAsync(hotel);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var col = GetCollection();
        await col.DeleteAsync(id, ct);
    }


    public async Task<Hotel?> GetAsync(int id, CancellationToken ct = default)
    {
        var col = GetCollection();
        return await col.GetAsync(id);
    }

    /// <summary>
    /// 向量检索：传一个查询文本，先生成向量，再做相似度搜索
    /// </summary>
    public async IAsyncEnumerable<(Hotel Record, double? Score)> SearchByTextAsync(
        string query, int topK = 5, CancellationToken ct = default)
    {
        var queryVector = await _emb.CreateAsync(query, ct);

        var col = GetCollection();

        var options = new VectorSearchOptions<Hotel>
        {
            //Filter = h => h.HotelName == "Tokyo",
            VectorProperty = h => h.DescriptionEmbedding,
            Skip = 0,
            IncludeVectors = false
        };

        await foreach (var r in col.SearchAsync(queryVector, topK, options, ct))
        {
            yield return (r.Record, r.Score);
        }
    }

    public async Task<List<Hotel>> GetAllAsync(CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(connectionString);

        var sql = @"SELECT hotel_id as hotelid,hotel_name as hotelname,hotel_description as hoteldescription,description_embedding as descriptionembedding FROM public.hotels;";

        var hotels = await conn.QueryAsync<Hotel>(sql);

        return hotels.ToList();
    }
}
