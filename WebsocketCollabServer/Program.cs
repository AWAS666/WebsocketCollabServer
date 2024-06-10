using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Serilog;
using WebsocketCollabServer.Helpers;
using WebsocketCollabServer.Services;

namespace WebsocketCollabServer
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var cfgSection = builder.Configuration.GetSection("Secrects");
            builder.Services.Configure<Config>(cfgSection);

            builder.Services.AddSingleton<DiscordBot>();
            builder.Services.AddSingleton<WebSocketServerManager>();

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
            }); ;
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            var app = builder.Build();

            // db migration
            using var db = new DataBaseContext();
            db.Database.Migrate();


            app.MapGet("/", () => "Hello World!");
            app.MapControllers();
            app.UseSwagger();
            app.UseSwaggerUI();


            // forceful startup otherwise those only start once injected
            var discord = app.Services.GetRequiredService<DiscordBot>();
            var websocket = app.Services.GetRequiredService<WebSocketServerManager>();
            var config = app.Services.GetService<IOptions<Config>>().Value;

            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.DiscordSerilogSink(discord, config.LogChannel)
            .CreateLogger();

            // recreate all rooms
            var rooms = await db.Rooms.ToListAsync();
            foreach (var room in rooms)
            {
                websocket.CreateRoom(room.Name);
            }

            app.Run();
        }
    }
}
