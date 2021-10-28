﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MultiToolBot.Model;

namespace MultiToolBot.Migrations
{
    [DbContext(typeof(GuildContext))]
    [Migration("20211027153830_Init")]
    partial class Init
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.10")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("MultiToolBot.Model.DequeuedTrack", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal?>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Uri")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Dequeued");
                });

            modelBuilder.Entity("MultiToolBot.Model.Guild", b =>
                {
                    b.Property<decimal>("Id")
                        .HasColumnType("decimal(20,0)");

                    b.Property<bool>("IsActive")
                        .HasColumnType("bit");

                    b.Property<bool>("IsJoined")
                        .HasColumnType("bit");

                    b.HasKey("Id");

                    b.ToTable("DiscordGuilds");
                });

            modelBuilder.Entity("MultiToolBot.Model.QueuedTrack", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<decimal?>("GuildId")
                        .HasColumnType("decimal(20,0)");

                    b.Property<string>("Uri")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("GuildId");

                    b.ToTable("Queued");
                });

            modelBuilder.Entity("MultiToolBot.Model.DequeuedTrack", b =>
                {
                    b.HasOne("MultiToolBot.Model.Guild", "Guild")
                        .WithMany("Dequeued")
                        .HasForeignKey("GuildId");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("MultiToolBot.Model.QueuedTrack", b =>
                {
                    b.HasOne("MultiToolBot.Model.Guild", "Guild")
                        .WithMany("Queued")
                        .HasForeignKey("GuildId");

                    b.Navigation("Guild");
                });

            modelBuilder.Entity("MultiToolBot.Model.Guild", b =>
                {
                    b.Navigation("Dequeued");

                    b.Navigation("Queued");
                });
#pragma warning restore 612, 618
        }
    }
}
