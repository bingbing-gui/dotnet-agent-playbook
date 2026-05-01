using AspNetCore.APIController.ActionReturnTypes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AspNetCore.APIController.ActionReturnTypes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _productContext;

        public ProductsController(ProductContext productContext)
        {
            _productContext = productContext;
        }
        [HttpGet]
        public Task<List<Product>> Get() =>
        _productContext.Products.OrderBy(p => p.Name).ToListAsync();


        [HttpGet("syncsale")]
        public IEnumerable<Product> GetOnSaleProducts()
        {
            var products = _productContext.Products.OrderBy(p => p.Name).ToList();

            foreach (var product in products)
            {
                if (product.IsOnSale)
                {
                    yield return product;
                }
            }
        }

        [HttpGet("asyncsale")]
        public async IAsyncEnumerable<Product> GetOnSaleProductsAsync()
        {
            var products = _productContext.Products.OrderBy(p => p.Name).AsAsyncEnumerable();

            await foreach (var product in products)
            {
                if (product.IsOnSale)
                {
                    yield return product;
                }
            }
        }

    }
}