namespace AspNetCore.DependencyInjection.Models
{
    public interface IStorage
    {
        IEnumerable<Product> Items { get; }
        Product this[string key] { get; set; }
        bool ContainsKey(string key);
        void RemoveItem(string key);
    }
}
