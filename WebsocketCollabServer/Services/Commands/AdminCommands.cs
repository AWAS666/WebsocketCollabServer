using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace WebsocketCollabServer.Services.Commands
{
    public class AdminCommands : ApplicationCommandsModule
    {
        public WebSocketServerManager WsManager { private get; set; }

        [SlashCommand("removeAdmin", "remove any room by id", dmPermission: false, defaultMemberPermissions: (long)Permissions.Administrator)]
        public async Task RemoveRoom(InteractionContext ctx, [Option("room", "room id")] string name)
        {
            using var db = new DataBaseContext();

            var room = await db.Rooms.FirstOrDefaultAsync(x => x.Name == name);

            if (room == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Room doesnt exist."
                });
                return;
            }

            bool success = WsManager.RemoveRoom(room.Name);

            db.Rooms.Remove(room);
            await db.SaveChangesAsync();

            Log.Debug($"Removed room {room.Name} with success: {success}");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = $"Removed successfully"
            });
        }



        [SlashCommand("allRooms", "get all rooms", dmPermission: false)]
        public async Task GetRooms(InteractionContext ctx)
        {
            using var db = new DataBaseContext();

            var rooms = await db.Rooms.Include(x => x.User).ToListAsync();

            var builder = new DiscordEmbedBuilder()
                .WithTitle("All Rooms")
                .WithTimestamp(DateTime.UtcNow);

            int i = 0;
            foreach (var room in rooms.TakeLast(10))
            {
                builder.AddField(new DiscordEmbedField($"Room {i + 1}:", $"{room.Name} by {room.User.Name}"));
                i++;
            }

            var response = new DiscordInteractionResponseBuilder().AddEmbed(builder.Build());
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }
    }
}

