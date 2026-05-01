namespace SK.VectorStores.Services
{
    // /Application/Contracts/IHotelService.cs
    using Microsoft.Extensions.VectorData;
    using Microsoft.SemanticKernel.Connectors.PgVector;
    using SK.VectorStores.Models;

    public interface IHotelService
    {


        Task UpsertAsync(Hotel hotel, CancellationToken ct = default);

        Task<Hotel?> GetAsync(int id, CancellationToken ct = default);

        IAsyncEnumerable<(Hotel Record, double? Score)> SearchByTextAsync(
       string query, int topK = 5, CancellationToken ct = default);

        Task DeleteAsync(int id, CancellationToken ct = default);
    }

}
