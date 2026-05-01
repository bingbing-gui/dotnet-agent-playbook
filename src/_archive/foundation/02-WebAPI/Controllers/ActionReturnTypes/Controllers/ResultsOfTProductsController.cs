using AspNetCore.APIController.ActionReturnTypes.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace AspNetCore.APIController.ActionReturnTypes.Controllers
{
    [ApiController]
    [Route("products/resultsoft")]
    public class ResultsOfTProductsController : ControllerBase
    {
        private readonly ProductContext _productContext;

        public ResultsOfTProductsController(ProductContext productContext)
        {
            _productContext = productContext;
        }
        [HttpGet("{id}")]
        public Results<NotFound, Ok<Product>> GetById(int id)
        {
            var product = _productContext.Products.Find(id);
            return product == null ? TypedResults.NotFound() : TypedResults.Ok(product);
        }
        [HttpPost]
        public async Task<Results<BadRequest, Created<Product>>> CreateAsync(Product product)
        {
            if (product.Description.Contains("XYZ Widget"))
            {
                return TypedResults.BadRequest();
            }
            _productContext.Products.Add(product);
            await _productContext.SaveChangesAsync();

            var location = Url.Action(nameof(CreateAsync), new { id = product.Id }) ?? $"/{product.Id}";
            return TypedResults.Created(location, product);
        }
    }
}