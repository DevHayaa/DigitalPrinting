using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Online_Order_for_digital_printing_of_Photos.Middleware
{
    public class RoleBasedAccessMiddleware
    {
        private readonly RequestDelegate _next;

        public RoleBasedAccessMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var path = context.Request.Path.ToString().ToLower();
            var role = context.Session.GetString("Role");

            // Skip checks for login and register pages, allow access even if the user is logged in
            if (path.StartsWith("/account/login") || path.StartsWith("/admin/login") ||
                path.StartsWith("/account/register") || path.StartsWith("/admin/register") ||
                path.StartsWith("/css") || path.StartsWith("/js") || path.StartsWith("/images"))
            {
                // If the user is already logged in and trying to access login or register, redirect them to the appropriate dashboard
                if (!string.IsNullOrEmpty(role))
                {
                    if (role == "Admin")
                    {
                        context.Response.Redirect("/Admin/Index");
                        return;
                    }
                    if (role == "User")
                    {
                        context.Response.Redirect("/User/Index");
                        return;
                    }
                }
                await _next(context);
                return;
            }

            // Restrict admin routes for non-admin users
            if (path.StartsWith("/admin") && role != "Admin")
            {
                context.Response.Redirect("/Admin/Login");
                return;
            }

            // Restrict user routes for non-user roles
            if (path.StartsWith("/account") && role != "User")
            {
                context.Response.Redirect("/Account/Login");
                return;
            }

            await _next(context); // Pass the request to the next middleware
        }
    }
}
