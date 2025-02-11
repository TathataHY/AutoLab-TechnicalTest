using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

public class WebErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<WebErrorHandlingMiddleware> _logger;

    public WebErrorHandlingMiddleware(RequestDelegate next, ILogger<WebErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Error de comunicación con la API");
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = (int?)ex.StatusCode ?? 500;
                await context.Response.WriteAsJsonAsync(new { error = "Error de comunicación con el servidor" });
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error no manejado en la aplicación web");
            if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Ha ocurrido un error inesperado" });
            }
            else
            {
                context.Response.Redirect("/Home/Error");
            }
        }
    }
} 