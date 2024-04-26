using DisCatSharp.Entities;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Plugins;

namespace WebsocketCollabServer.Services
{
    public class DataBaseContext : DbContext
    {
        public string DbPath { get; }
        public DbSet<User> Users { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DataBaseContext(string db = "database.db")
        {
            var folder = Environment.SpecialFolder.LocalApplicationData;
            var env = Environment.GetFolderPath(folder);
            var path = Path.Combine(env, "WebsocketCollab");
            Directory.CreateDirectory(path);
            DbPath = Path.Join(path, db);
        }

        // The following configures EF to create a Sqlite database file in the
        // special "local" folder for your platform.
        protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={DbPath}");

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }

    public class User
    {
        public int UserId { get; set; }
        public ulong DiscordId { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public virtual List<Room> Rooms { get; set; } = new List<Room>();
    }

    public class Room
    {
        public int RoomId { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public virtual User User { get; set; }
    }
}

