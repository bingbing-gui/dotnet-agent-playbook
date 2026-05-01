using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Sqlids.AspNetCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SqlidsDemoController : ControllerBase
    {
        private readonly SqidsEncoder<int> _sqids;
        public SqlidsDemoController(SqidsEncoder<int> sqids)
        {
            _sqids = sqids;
        }

        [HttpGet("generate")]
        public IActionResult Generate()
        {
            // 生成一个简单的ID
            int id = (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % 1000000);
            string encoded = _sqids.Encode(id);
            string url = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/api/sqlidsdemo/generate?id={encoded}";
            return Ok(new { url, id, encoded });
        }
    }
}
