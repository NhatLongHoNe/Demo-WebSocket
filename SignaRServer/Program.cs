using SignaRServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors(builder => builder
        .WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials());

app.MapHub<ChatHub>("/chatHub");

//app.MapGet("/", () => "Hello World!");

app.Run();
