using System.Collections.Generic;
using MultiToolBot.Model.MusicModel;

namespace MultiToolBot.Model
{
    public class Guild
    {
        public ulong? Id { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsJoined { get; set; } = true;

        public Guild(ulong? guildId)
        {
            Id = guildId;
        }
        public Guild() { }
        public ICollection<QueuedTrack> Queued { get; set; }
        public ICollection<DequeuedTrack> Dequeued { get; set; }
    }
}
