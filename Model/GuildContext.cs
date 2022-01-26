using Microsoft.EntityFrameworkCore;
using MultiToolBot.Model.MusicModel;

namespace MultiToolBot.Model
{
    sealed class GuildContext : DbContext
    {
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Guild> DiscordGuilds { get; set; }
        public DbSet<TextChannel> TextChannels { get; set; }

        //public GuildContext()
        //{
        //    Database.EnsureCreated();
        //}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=localhost\SQLEXPRESS;Database=VoiceDB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Guild>()
            //    .Property(q => q.Id)
            //    .ValueGeneratedNever();

            //modelBuilder.Entity<TextChannel>()
            //    .Property(chan => chan.Id)
            //    .ValueGeneratedNever();

            //modelBuilder.Entity<Track>()
            //    .Property(track => track.Id)
            //    .ValueGeneratedOnAdd();

            modelBuilder.
                Entity<Track>()
                .HasOne(track => track.Guild)
                .WithMany(guild => guild.Tracks)
                .HasForeignKey(track => track.GuildId);

            modelBuilder
                .Entity<Guild>()
                .HasOne(guild => guild.TextChannel)
                .WithOne(chan => chan.Guild)
                .HasForeignKey<TextChannel>(chan => chan.GuildId);

            modelBuilder.Entity<TextChannel>()
                .Property(chan => chan.Id)
                .ValueGeneratedNever();
        }
    }
}
