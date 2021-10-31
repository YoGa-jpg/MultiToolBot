using Microsoft.EntityFrameworkCore;
using MultiToolBot.Model.MusicModel;

namespace MultiToolBot.Model
{
    sealed class GuildContext : DbContext
    {
        public DbSet<Track> Tracks { get; set; }
        public DbSet<Guild> DiscordGuilds { get; set; }

        public GuildContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=VoiceDB;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Guild>()
            //    .Property(q => q.Id)
            //    .ValueGeneratedOnAdd();

            modelBuilder.Entity<Track>()
                .HasOne(track => track.Guild)
                .WithMany(guild => guild.Tracks)
                .HasForeignKey(track => track.GuildId);
        }
    }
}
