using System.Net.WebSockets;
using WebSocketServer.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddWebSocketServer();

var app = builder.Build();

app.UseWebSockets();

app.UseWebSocketServer();

app.MapGet("/", () => "Hello World!");

app.Run();

