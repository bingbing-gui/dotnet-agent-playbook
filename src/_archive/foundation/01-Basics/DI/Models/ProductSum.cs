namespace AspNetCore.DependencyInjection.Models
{
    public class ProductSum
    {
        public IRepository Repository { get; set; }
        public ProductSum(IRepository repo)
        {
            Repository = repo;
        }
        public decimal Total => Repository.Products.Sum(p => p.Price);
    }
}
