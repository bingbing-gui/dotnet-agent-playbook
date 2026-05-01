using AspNetCore.APIController.ActionReturnTypes.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace AspNetCore.APIController.ActionReturnTypes.Controllers
{
    [ApiController]
    [Route("products/iactionresult")]
    public class ActionResultProductsController : ControllerBase
    {
        private readonly ProductContext _productContext;
        public ActionResultProductsController(ProductContext productContext)
        {
            _productContext = productContext;
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Product))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetById_IActionResult(int id)
        {
            var product = _productContext.Products.Find(id);
            return product == null ? NotFound() : Ok(product);
        }
        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateAsync_IActionResult(Product product)
        {
            if (product.Description.Contains("XYZ Widget"))
            {
                return BadRequest();
            }
            _productContext.Products.Add(product);
            await _productContext.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById_IActionResult), new { id = product.Id }, product);
        }
    }
}
