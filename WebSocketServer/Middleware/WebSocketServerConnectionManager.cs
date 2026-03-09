using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;


namespace WebSocketServer.Middleware
{
    public class WebSocketServerConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public ConcurrentDictionary<string, WebSocket> GetAllSockets()
        {
            return _sockets;
        }

        public string AddSocket(WebSocket socket)
        {
            var id = Guid.NewGuid().ToString();
            _sockets.TryAdd(id, socket);
            Console.WriteLine("Socket added: " + id);
            return id;
        }
        
        
    }
}