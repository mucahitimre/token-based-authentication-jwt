using JwtTokenize.Handlers;

namespace JwtTokenize.Extensions
{
    public static class CustomMiddlewareExtension
    {
        public static IApplicationBuilder UseJwtAuthorization(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<JwtMiddleware>();

            return builder;
        }
    }
}
