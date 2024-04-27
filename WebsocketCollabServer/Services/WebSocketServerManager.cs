using Microsoft.Extensions.Options;
using Serilog;
using System.Security.Principal;
using WebsocketCollabServer.Helpers;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

namespace WebsocketCollabServer.Services
{
    public class WebSocketServerManager
    {
        public WebSocketServer Wssv { get; }

        public Dictionary<string, DateTime> Rooms { get; set; } = new();

        public WebSocketServerManager(IOptions<Config> config)
        {
            Wssv = new WebSocketServer(config.Value.Host);
            Wssv.AuthenticationSchemes = AuthenticationSchemes.Basic;
            Wssv.Realm = "Websocket Collab";
            Wssv.UserCredentialsFinder = Verify;
            Wssv.Log.Level = WebSocketSharp.LogLevel.Trace;
            Wssv.Start();
        }

        public string CreateRoom()
        {
            // generate room id and make sure it's unique
            string room = GenerateRoomId(8);
            while (Rooms.ContainsKey(room))
            {
                room = GenerateRoomId(8);
            }
            Wssv.AddWebSocketService("/" + room, () => new Chat(Rooms, room));

            Rooms.Add(room, DateTime.UtcNow);
            Log.Debug($"Created room: {room}");
            return room;
        }

        public bool RemoveRoom(string path)
        {
            Rooms.Remove(path);
            return Wssv.RemoveWebSocketService(path);
        }

        public string GenerateRoomId(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        private NetworkCredential? Verify(IIdentity identity)
        {
            var name = identity.Name;

            using var db = new DataBaseContext();

            var user = db.Users.FirstOrDefault(x => x.Name == name);

            return user == null ? null : new NetworkCredential(user.Name, user.Password);
        }
    }

    public class Chat : WebSocketBehavior
    {
        private Dictionary<string, DateTime> rooms;
        private string id;

        public Chat(Dictionary<string, DateTime> rooms, string id)
        {
            this.rooms = rooms;
            this.id = id;
        }

        protected override void OnMessage(MessageEventArgs e)
        {
            // save last message timestamp
            rooms[id] = DateTime.UtcNow;

            // debug
            Serilog.Log.Debug($"New Message in: {id}");

            // broadcast raw message to all
            Sessions.Broadcast(e.Data);
        }
    }
}
