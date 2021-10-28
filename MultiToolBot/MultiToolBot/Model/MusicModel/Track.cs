using System;
using DSharpPlus.CommandsNext;

namespace MultiToolBot.Model.MusicModel
{
    public class Track
    {
        public Guid Id { get; set; }
        public string Uri { get; set; }
        public ulong? GuildId { get; set; }
        public Guild Guild { get; set; }

        protected Track(CommandContext ctx, Uri uri)
        {
            GuildId = ctx.Channel.GuildId;
            Uri = uri.ToString();
        }
        protected Track(ulong? guildId, Uri uri)
        {
            GuildId = guildId;
            Uri = uri.ToString();
        }
        protected Track() { }
    }
}