using System;
using DSharpPlus.Lavalink;

namespace MultiToolBot.Model
{
    public abstract class Track : LavalinkTrack
    {
        public Guid Id { get; set; }
        public new string Uri => base.Uri.ToString();
        public ulong? GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}