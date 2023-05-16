using APILogger.Services;
using APILogger.Services.DB;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.FileProviders;
using System.Net;
using ZeroLevel;

namespace APILogger
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost.ConfigureKestrel(serverOptions =>
            {
                serverOptions.Listen(IPAddress.Any, 57717, listenOptions =>
                {
                    listenOptions.UseConnectionLogging();
                });
            });
            var config = AppSettings.Create();
            AuthProvider.Init(config, new UserRepository("users"));

            builder.Services.AddSingleton<LogRepository>(new LogRepository("logdb"));
            builder.Services.AddControllers();
            var app = builder.Build();
            app.Use(async (context, next) =>
            {
                try
                {
                    await Middlewares.HandleStartRequest(context);
                }
                catch
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
                try
                {
                    await next();
                }
                catch (Exception ex)
                {
                    var controllerActionDescriptor = context?
                    .GetEndpoint()?
                    .Metadata?
                    .GetMetadata<ControllerActionDescriptor>();
                    var controllerName = controllerActionDescriptor?.ControllerName ?? string.Empty;
                    var actionName = controllerActionDescriptor?.ActionName ?? string.Empty;
                    Log.Error(ex, $"[{controllerName}.{actionName}]");
                    if (context != null)
                    {
                        switch (ex)
                        {
                            case KeyNotFoundException
                                or FileNotFoundException:
                                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                                break;
                            case UnauthorizedAccessException:
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                break;
                            default:
                                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                await context.Response.WriteAsync(ex.Message ?? "Error");
                                break;
                        }

                    }
                }
            });
            app.UseAuthorization();
            app.MapControllers();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(builder.Environment.ContentRootPath, "web")),
                RequestPath = "/web"
            });
            app.Run();
        }
    }
}