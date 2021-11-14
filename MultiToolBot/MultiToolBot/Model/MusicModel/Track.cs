using System;
using DSharpPlus.CommandsNext;

namespace MultiToolBot.Model.MusicModel
{
    public class Track : ICloneable
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public ulong? GuildId { get; set; }
        public Guild Guild { get; set; }
        public bool Pointer { get; set; }
        public Track(CommandContext ctx, Uri uri)
        {
            GuildId = ctx.Channel.GuildId;
            Uri = uri.ToString();
        }
        public Track(ulong? guildId, Uri uri)
        {
            GuildId = guildId;
            Uri = uri.ToString();
        }
        public Track() { }
        public object Clone()
        {
            return new Track()
            {
                Uri = this.Uri,
                GuildId = this.GuildId,
                Pointer = this.Pointer
            };
        }
    }
}