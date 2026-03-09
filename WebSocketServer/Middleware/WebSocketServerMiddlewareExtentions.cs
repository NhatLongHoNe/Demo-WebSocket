using System;
using System.Net.WebSockets;
using System.Text;


namespace WebSocketServer.Middleware
{
    public static class WebSocketServerMiddlewareExtentions
    {
      public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
      {
        return builder.UseMiddleware<WebSocketServerMiddleware>();
      }

      public static IServiceCollection AddWebSocketServer(this IServiceCollection services)
      {
        services.AddSingleton<WebSocketServerConnectionManager>();
        return services;
      }
    }
}