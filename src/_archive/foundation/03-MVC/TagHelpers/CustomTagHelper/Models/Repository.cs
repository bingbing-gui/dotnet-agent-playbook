namespace AspNetCore.CustomTagHelpers.Models
{
    public class Repository
    {
        public interface IRepository
        {
            IEnumerable<Product> Products { get; }
            void AddProduct(Product newProduct);
        }
        public class ProductRepository : IRepository
        {
            private List<Product> products = new List<Product>
            {
                new Product { Name = "Men Shoes", Price = 99.99F, Quantity= 100},
                new Product { Name = "Women Shoes", Price = 199.99F, Quantity= 200},
                new Product { Name = "Children Games", Price = 299.99F, Quantity= 300},
                new Product { Name = "Coats", Price = 399.99F, Quantity= 400},
            };
            public IEnumerable<Product> Products => products;
            public void AddProduct(Product newProduct)
            {
                products.Add(newProduct);
            }
        }
    }
}
