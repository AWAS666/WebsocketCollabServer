using DisCatSharp.Entities;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using Serilog;
using WebsocketCollabServer.Services;

namespace WebsocketCollabServer.Helpers
{
    public class DiscordSerilogSink : ILogEventSink
    {
        private DiscordBot _discord;
        private ulong _channel;

        public DiscordSerilogSink(DiscordBot discord, ulong channelId)
        {
            _discord = discord;
            _channel = channelId;
        }

        public async void Emit(LogEvent logEvent)
        {
            if (logEvent.Level >= LogEventLevel.Warning)
            {
                await _discord.PostMessage($"{logEvent.RenderMessage()}", _channel);
            }
        }
    }

    public static class DiscordSinkExtension
    {
        public static LoggerConfiguration DiscordSerilogSink(
                  this LoggerSinkConfiguration loggerConfiguration,
                    DiscordBot discord, ulong channelId)
        {
            return loggerConfiguration.Sink(new DiscordSerilogSink(discord, channelId));
        }
    }
}
