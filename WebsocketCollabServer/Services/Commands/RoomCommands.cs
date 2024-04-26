using DisCatSharp.ApplicationCommands;
using DisCatSharp.ApplicationCommands.Attributes;
using DisCatSharp.ApplicationCommands.Context;
using DisCatSharp.Entities;
using DisCatSharp.Enums;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace WebsocketCollabServer.Services.Commands
{
    public class RoomCommands : ApplicationCommandsModule
    {
        public WebSocketServerManager WsManager { private get; set; }

        [SlashCommand("signup", "sign up to get a user/password", dmPermission: false)]
        public async Task SignUp(InteractionContext ctx)
        {
            using var db = new DataBaseContext();

            var user = await db.Users.FirstOrDefaultAsync(x => x.DiscordId == ctx.User.Id);

            if (user != null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Already signed up"
                });
                return;
            }

            user = new User() { DiscordId = ctx.User.Id, Name = ctx.User.Username, Password = GeneratePassword(8) };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            Log.Debug($"{user.Name} signed up");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = $"Signed up. Name: {user.Name}, Password: `{user.Password}`",
                IsEphemeral = true
            });
        }

        [SlashCommand("resetPassword", "reset and get a new Password", dmPermission: false)]
        public async Task ResetPassword(InteractionContext ctx)
        {
            using var db = new DataBaseContext();

            var user = await db.Users.FirstOrDefaultAsync(x => x.DiscordId == ctx.User.Id);

            if (user == null)
            {
                await SignUp(ctx);
                return;
            }

            user.Password = GeneratePassword(8);

            db.Users.Update(user);
            await db.SaveChangesAsync();

            Log.Debug($"{user.Name} reset password");

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = $"Signed up. Name: {user.Name}, Password: `{user.Password}`",
                IsEphemeral = true
            });
        }

        public string GeneratePassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Shared.Next(s.Length)]).ToArray());
        }

        [SlashCommand("create", "creates a room", dmPermission: false)]
        public async Task Create(InteractionContext ctx)
        {
            using var db = new DataBaseContext();

            var user = await db.Users.Include(x => x.Rooms)
                .FirstOrDefaultAsync(x => x.DiscordId == ctx.User.Id);

            if (user == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Sign up first"
                });
                return;
            }

            if (user.Rooms.Count >= 3)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Cant create more than 3 rooms"
                });
                return;
            }

            string name = WsManager.CreateRoom();

            Log.Debug($"{user.Name} created room {name}");
            user.Rooms.Add(new Room() { Name = name, Created = DateTime.UtcNow });
            db.Users.Update(user);
            await db.SaveChangesAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = $"Room created, id is: `{name}`",
                IsEphemeral = true
            });
        }

        [SlashCommand("remove", "removes a room", dmPermission: false)]
        public async Task Remove(InteractionContext ctx, [Option("room", "room id")] string name)
        {
            using var db = new DataBaseContext();

            var user = await db.Users.Include(x => x.Rooms)
                .FirstOrDefaultAsync(x => x.DiscordId == ctx.User.Id);

            if (user == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Sign up first"
                });
                return;
            }

            var room = user.Rooms.FirstOrDefault(x => x.Name == name);
            if (room == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Room doesnt exist"
                });
                return;
            }

            bool success = WsManager.RemoveRoom(room.Name);

            user.Rooms.Remove(room);
            db.Users.Update(user);
            await db.SaveChangesAsync();

            Log.Debug($"Removed room {room.Name} with success: {success}");


            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
            {
                Content = $"Removed successfully"
            });
        }

        [SlashCommand("rooms", "get all of your current rooms", dmPermission: false)]
        public async Task GetRooms(InteractionContext ctx)
        {
            using var db = new DataBaseContext();

            var user = await db.Users.Include(x => x.Rooms)
                .FirstOrDefaultAsync(x => x.DiscordId == ctx.User.Id);

            if (user == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                {
                    Content = $"Sign up first"
                });
                return;
            }

            var builder = new DiscordEmbedBuilder()
                .WithTitle("Your Rooms")
                .WithTimestamp(DateTime.UtcNow);

            for (int i = 0; i < user.Rooms.Count; i++)
            {
                var x = user.Rooms[i];
                builder.AddField(new DiscordEmbedField($"Room {i + 1}:", $"{user.Rooms[i].Name}"));
            }

            var response = new DiscordInteractionResponseBuilder() { IsEphemeral = true }.AddEmbed(builder.Build());
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
        }
    }
}
