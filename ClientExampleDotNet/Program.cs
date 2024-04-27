using ClientExampleDotNet;
using System.Text.Json;
using WebSocketSharp;

internal class Program
{
    private static string self = "user";
    private static void Main(string[] args)
    {
        string user = "";
        string password = "";

        // this is the base url of the server + room id
        string host = "ws://<server>/<id>";
        using (var ws = new WebSocket(host))
        {
            ws.SetCredentials(user, password, true);
            ws.OnMessage += NewMessage;
            ws.Connect();


            ws.Send(JsonSerializer.Serialize(new Message()
            {
                From = self,
                To = new List<string> { "receiver"},
                Type = "message",
                Payload = new() { Name = self, Content = "Hello world!"}
            }
            ));
            Console.ReadKey(true);
        }
        Console.ReadKey(true);
    }

    private static void NewMessage(object? sender, MessageEventArgs e)
    {
        var msg = JsonSerializer.Deserialize<Message>(e.Data);

        // check if addressed to self
        if (!msg.To.Contains(self))
            return;

        if (msg.Type == "message")
        {
            //use message
            var name = msg.Payload.Name;
            var content = msg.Payload.Content;
        }
    }
}