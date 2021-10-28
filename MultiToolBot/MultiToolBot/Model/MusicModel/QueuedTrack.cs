using System;
using DSharpPlus.CommandsNext;

namespace MultiToolBot.Model.MusicModel
{
    public class QueuedTrack : Track
    {
        public QueuedTrack(CommandContext ctx, Uri uri) : base(ctx, uri) { }
        public QueuedTrack(ulong? guildId, Uri uri) : base(guildId, uri) { }
        public QueuedTrack() { }
    }
}
