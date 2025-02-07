using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace CarRental.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthenticationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var protectedPaths = new[] { "/CustomerCar/Book", "/CustomerCar/BookingConfirmation" };

            if (protectedPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                
                if (!context.Request.Cookies.ContainsKey("AuthToken"))
                {
                    
                    context.Response.Redirect("/Login/Index");
                    return;
                }
            }


            await _next(context);
        }
    }
}
