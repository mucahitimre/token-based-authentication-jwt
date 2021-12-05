using JwtTokenize.Handlers;
using JwtTokenize.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace JwtTokenize.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        public ProductController()
        {
        }

        [HttpGet]
        [Route("GetProduct")]
        public ProductResponse GetProduct(int id)
        {
            if (id == default)
            {
                return null;
            }

            return new ProductResponse() { Id = id, ProductName = "Example product", ProductCode = "PR655" };
        }
    }
}
