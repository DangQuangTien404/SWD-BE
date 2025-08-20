using System.Threading.Tasks;
using BookLibwithSub.Repo.Interfaces;
using Microsoft.AspNetCore.Http;

namespace BookLibwithSub.API.Middleware
{
    public class TokenValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public TokenValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IUserRepository userRepository)
        {
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.EndsWith("/login") || path.EndsWith("/register")))
            {
                await _next(context);
                return;
            }

            var authHeader = context.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                var token = authHeader.Substring("Bearer ".Length).Trim();
                var user = await userRepository.GetByTokenAsync(token);
                if (user == null)
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Invalid session");
                    return;
                }
            }

            await _next(context);
        }
    }
}
