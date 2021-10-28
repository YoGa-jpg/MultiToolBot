using System;
using DSharpPlus.CommandsNext;

namespace MultiToolBot.Model.MusicModel
{
    public class DequeuedTrack : Track
    {
        public DequeuedTrack(CommandContext ctx, Uri uri) : base(ctx, uri) { }
        public DequeuedTrack(ulong? guildId, Uri uri) : base(guildId, uri) { }
        public DequeuedTrack() { }
    }
}
