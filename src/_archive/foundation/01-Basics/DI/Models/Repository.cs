namespace AspNetCore.DependencyInjection.Models
{
    public class Repository : IRepository
    {
        private IStorage _storage;
        public Repository(IStorage storage)
        {
            _storage = storage;
            new List<Product> {
                new Product { Name = "Women Shoes", Price = 99M },
                new Product { Name = "Skirts", Price = 29.99M },
                new Product { Name = "Pants", Price = 40.5M }
            }.ForEach(p => AddProduct(p));
        }
        public IEnumerable<Product> Products => _storage.Items;
        public Product this[string name] => _storage[name];
        public void AddProduct(Product product) => _storage[product.Name] = product;
        public void DeleteProduct(Product product) => _storage.RemoveItem(product.Name);

        private string guid = Guid.NewGuid().ToString();
        public override string ToString()
        {
            return guid;
        }
    }
}
