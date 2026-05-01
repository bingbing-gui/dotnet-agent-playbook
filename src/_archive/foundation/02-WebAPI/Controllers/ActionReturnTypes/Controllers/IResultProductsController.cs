using AspNetCore.APIController.ActionReturnTypes.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Mime;

namespace AspNetCore.APIController.ActionReturnTypes.Controllers
{
    [ApiController]
    [Route("products/iresult")]
    public class IResultProductsController : ControllerBase
    {
        private readonly ProductContext _productContext;

        public IResultProductsController(ProductContext productContext)
        {
            _productContext = productContext;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IResult GetById(int id)
        {
            var product = _productContext.Products.Find(id);
            return product == null ? Results.NotFound() : Results.Ok(product);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IResult> CreateAsync(Product product)
        {
            if (product.Description.Contains("XYZ Widget"))
            {
                return Results.BadRequest();
            }
            _productContext.Products.Add(product);
            await _productContext.SaveChangesAsync();

            var location = Url.Action(nameof(CreateAsync), new { id = product.Id }) ?? $"/{product.Id}";
            return Results.Created(location, product);
        }
    }
}