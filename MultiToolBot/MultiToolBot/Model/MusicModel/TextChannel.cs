using System;
using System.Collections.Generic;
using System.Text;

namespace MultiToolBot.Model.MusicModel
{
    public class TextChannel
    {
        public ulong Id { get; set; }
        public ulong? GuildId { get; set; }
        public Guild Guild { get; set; }

        public TextChannel(ulong id, ulong? guildId)
        {
            Id = id;
            GuildId = guildId;
        }

        public TextChannel() { }
    }
}
