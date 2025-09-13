using Microsoft.AspNetCore.Http;

namespace Pizzaria.Middleware
{
    public class AdminAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Verifica se está acessando uma rota de admin
            if (context.Request.Path.StartsWithSegments("/Home/AdminPedidos") ||
                context.Request.Path.StartsWithSegments("/Home/AtualizarStatus"))
            {
                var isAdmin = context.Session.GetString("IsAdmin");
                
                if (string.IsNullOrEmpty(isAdmin) || isAdmin != "True")
                {
                    // Redireciona para não autorizado
                    context.Response.Redirect("/Home/NaoAutorizado");
                    return;
                }
            }

            await _next(context);
        }
    }
}