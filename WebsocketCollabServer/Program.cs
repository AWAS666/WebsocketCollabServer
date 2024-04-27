using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using WebsocketCollabServer.Helpers;
using WebsocketCollabServer.Services;

namespace WebsocketCollabServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var cfgSection = builder.Configuration.GetSection("Secrects");
            builder.Services.Configure<Config>(cfgSection);

            builder.Services.AddSingleton<DiscordBot>();
            builder.Services.AddSingleton<WebSocketServerManager>();
            var app = builder.Build();

            // db migration
            using var db = new DataBaseContext();
            db.Database.Migrate();

            // remove all rooms
            db.Rooms.RemoveRange(db.Rooms.ToList());
            db.SaveChanges();

            app.MapGet("/", () => "Hello World!");

            // forceful startup otherwise those only start once injected
            var discord = app.Services.GetRequiredService<DiscordBot>();
            app.Services.GetRequiredService<WebSocketServerManager>();
            var config = app.Services.GetService<IOptions<Config>>().Value;

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.DiscordSerilogSink(discord, config.LogChannel)
            .CreateLogger();

            app.Run();
        }
    }
}
