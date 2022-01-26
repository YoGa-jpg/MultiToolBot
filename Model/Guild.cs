using System.Collections.Generic;
using MultiToolBot.Model.MusicModel;

namespace MultiToolBot.Model
{
    public class Guild
    {
        public ulong? Id { get; set; }
        public bool IsActive { get; set; } = false;
        public bool IsJoined { get; set; } = true;
        public bool IsConfigured { get; set; } = false;

        public Guild(ulong? guildId)
        {
            Id = guildId;
        }
        public Guild() { }
        public ICollection<Track> Tracks { get; set; }
        public TextChannel TextChannel { get; set; }
    }
}
