using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;

namespace WebSocketServer.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _connectionManager;

        public WebSocketServerMiddleware(RequestDelegate next)
        {
            _next = next;
            _connectionManager = new WebSocketServerConnectionManager();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                // Fixed: call the instance method on a new Helpers instance
                WriteToConsoleParam(context);
                var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("WebSocket connection established.");

                var connectionId = _connectionManager.AddSocket(webSocket);
                await SendConnIDAsync(webSocket, connectionId);

                await ReceiceMessage(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("Message received:");
                        Console.WriteLine( "Message: " + Encoding.UTF8.GetString(buffer, 0, result.Count));
                        await RouteJsonMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        string id = _connectionManager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
                        _connectionManager.GetAllSockets().TryRemove(id, out WebSocket socket);

                        Console.WriteLine("WebSocket connection closed.");
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                        return;
                    }
                });
            }
            else
            {
                await _next(context);
            }
        }

        public static async Task SendConnIDAsync(WebSocket socket, string connID)
        {
            var buffer = Encoding.UTF8.GetBytes("ConnID: " + connID);
            await socket.SendAsync(buffer: new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }


        public static async Task ReceiceMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];


            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer)
                , cancellationToken: CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        public static void WriteToConsoleParam(HttpContext content)
        {
            Console.WriteLine("Request Method: {0}", content.Request.Method);
            Console.WriteLine("Request Protocol: {0}", content.Request.Protocol);

            if (content.Request.Headers != null)
            {
                foreach (var header in content.Request.Headers)
                {
                    Console.WriteLine("Header: {0} = {1}", header.Key, header.Value);
                }
            }
        }

        public async Task RouteJsonMessageAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message)) return;

            var routeOb = JsonConvert.DeserializeObject<dynamic>(message);

            if (routeOb == null || routeOb.To == null || routeOb.Message == null)
            {
                Console.WriteLine("Invalid message format received.");
                return;
            }

            if (Guid.TryParse(routeOb.To.ToString(), out Guid guidOutput))
            {
                Console.WriteLine("Targeted message to: " + routeOb.To);
                var socket = _connectionManager.GetAllSockets().FirstOrDefault(s => s.Key == routeOb.To.ToString());
                if (socket.Value != null && socket.Value.State == WebSocketState.Open)
                {
                    await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), 
                    WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else
                {
                    Console.WriteLine("Targeted socket not found or not open: " + routeOb.To);
                }
            }
            else 
            { 
                Console.WriteLine("Boardcast");
                foreach (var socket in _connectionManager.GetAllSockets())
                {
                    if (socket.Value.State == WebSocketState.Open)
                    {
                        await socket.Value.SendAsync(Encoding.UTF8.GetBytes(routeOb.Message.ToString()), 
                        WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
        }
    }
}