using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace JwtTokenize.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IMembershipService _membershipService;

        public AuthenticationController(IMembershipService membershipService)
        {
            _membershipService = membershipService;
        }

        [HttpPost]
        [Route("Login")]
        [AllowAnonymous]
        public string Login(string email, string password)
        {
            var member = _membershipService.ValidateUser(email, password);
            if (member == null)
            {
                throw new UnauthorizedAccessException();
            }

            var token = _membershipService.GetToken(member);

            return token;
        }
    }

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
        public ProductDto GetProduct(int id)
        {
            if (id == default)
            {
                return null;
            }

            return new ProductDto() { Id = id, ProductName = "Example product", ProductCode = "PR655" };
        }
    }

    public class ProductDto
    {
        public int Id { get; set; }

        public string ProductName { get; set; }

        public string ProductCode { get; set; }
    }

    public class AuthorizationFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
            if (allowAnonymous)
                return;

            var user = context.HttpContext.Items["User"];
            if (user == null)
            {
                // not logged in or role not authorized
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }

            // role and permission check
        }
    }

    public class JwtMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IMembershipService membershipService)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            var user = membershipService.ValidateUserByToken(token);
            if (user != null)
            {
                // attach user to context on successful jwt validation
                context.Items["User"] = user;
            }

            await _next(context);
        }
    }

    public static class CustomMiddlewareExtension
    {
        public static IApplicationBuilder UseJwtAuthorization(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<JwtMiddleware>();

            return builder;
        }
    }
}
