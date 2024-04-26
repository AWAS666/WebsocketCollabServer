using DisCatSharp.ApplicationCommands;
using DisCatSharp.Enums;
using DisCatSharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;
using DisCatSharp.Entities;
using WebsocketCollabServer.Helpers;
using WebsocketCollabServer.Services.Commands;

namespace WebsocketCollabServer.Services
{
    public class DiscordBot
    {
        public DiscordClient Client { get; private set; }

        public DiscordBot(IOptions<Config> secretOptions, IServiceProvider service)
        {
            //https://docs.dcs.aitsys.dev/articles/topics/logging/third_party
            var logFactory = new LoggerFactory().AddSerilog();
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = secretOptions.Value.DiscordToken,
                TokenType = TokenType.Bot,
                Intents = DiscordIntents.All,
                LoggerFactory = logFactory,
                MinimumLogLevel = LogLevel.Information,
            });

            //https://docs.dcs.aitsys.dev/articles/modules/commandsnext/dependency_injection
            var appCommands = Client.UseApplicationCommands(new ApplicationCommandsConfiguration
            {
                DebugStartup = true,
                ServiceProvider = service
            });
            appCommands.RegisterGlobalCommands<RoomCommands>();

            StartAsync().GetAwaiter().GetResult();

        }

        public async Task StartAsync()
        {
            await Client.ConnectAsync();
        }

        public async Task PostMessage(string msg, ulong id)
        {
            var channel = await Client.GetChannelAsync(id, true);
            await Client.SendMessageAsync(channel, msg);
        }

        public async Task PostMessage(DiscordEmbed msg, ulong id)
        {
            var channel = await Client.GetChannelAsync(id, true);
            await Client.SendMessageAsync(channel, msg);
        }
    }
}
