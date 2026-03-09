using System;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SignaRServer
{
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync(){
            Console.WriteLine($"{Context.ConnectionId} connected");
            await Clients.Client(Context.ConnectionId).SendAsync("ReceiveConnectionId", $"System {Context.ConnectionId} connected");
            await base.OnConnectedAsync();
        }
        public async Task SendMessageAsync(string message){
            var routeOb = JsonConvert.DeserializeObject<dynamic>(message);
            string toClient = routeOb?.To ?? string.Empty;
            Console.WriteLine($"Message Received on : {Context.ConnectionId}");


            if(toClient == string.Empty)
            {
                await Clients.All.SendAsync("ReceiveMessage", $"System {Context.ConnectionId} sent message: {message}");
            }
            else
            {
                await Clients.Client(toClient).SendAsync("ReceiveMessage", $"System {Context.ConnectionId} sent message: {message}");
            }
        }
    }
}