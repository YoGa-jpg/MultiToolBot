using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace MultiToolBot.Model
{
    sealed class GuildContext : DbContext
    {
        public DbSet<DequeuedTrack> Dequeued { get; set; }
        public DbSet<QueuedTrack> Queued { get; set; }
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
            modelBuilder.Entity<Guild>()
                .Property(q => q.Id)
                .ValueGeneratedNever();

            modelBuilder.Entity<DequeuedTrack>()
                .HasOne(track => track.Guild)
                .WithMany(guild => guild.Dequeued)
                .HasForeignKey(track => track.GuildId);
            modelBuilder.Entity<QueuedTrack>()
                .HasOne(track => track.Guild)
                .WithMany(guild => guild.Queued)
                .HasForeignKey(track => track.GuildId);
        }
    }
}
