using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Microsoft.EntityFrameworkCore;
using MultiToolBot.Model;
using MultiToolBot.Model.MusicModel;

namespace MultiToolBot.Commands
{
    class VoiceCommands : BaseCommandModule
    {
        private readonly GuildContext _context = new GuildContext();
        private readonly DbSet<QueuedTrack> _queued;
        private readonly DbSet<DequeuedTrack> _dequeued;
        private readonly DbSet<Guild> _guilds;
        //private LavalinkNodeConnection Lavalink { get; set; }
        //private LavalinkGuildConnection LavalinkVoice { get; set; }
        //private DiscordChannel ContextChannel { get; set; }
        //private LinkedList<LavalinkTrack> QueuedTracks { get; set; }
        //private Stack<LavalinkTrack> DequeuedTracks { get; set; }
        //private bool IsActive { get; set; }
        //private bool IsJoined { get; set; }
        public VoiceCommands()
        {
            _queued = _context.Queued;
            _dequeued = _context.Dequeued;
            _guilds = _context.DiscordGuilds;
        }

        [Command("join")]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {
            channel = ctx.Member.VoiceState.Channel;

            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Внутренняя ошибка");
                return;
            }

            var lavalink = lava.ConnectedNodes.Values.First(); // Lavalink

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Некорректный канал");
                return;
            }

            await lavalink.ConnectAsync(channel);
            //var lavalinkVoice = lavalink.GetGuildConnection(ctx.Member.VoiceState.Guild); //LavalinkVoice

            if (!_guilds.Any(guild => guild.Id == channel.GuildId))
            {
                _guilds.Add(new Guild(channel.GuildId));
                await _context.SaveChangesAsync();
            }

            _guilds.Single(guild => guild.Id == channel.GuildId).IsJoined = true;
            await _context.SaveChangesAsync();

            await ctx.RespondAsync($"Зашел в {channel.Name}!");
        }

        [Command("play"), Description("Добавление в очередь")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] Uri uri)
        {
            //if (LavalinkVoice != null && !LavalinkVoice.IsConnected)
            //{
            //    await LeaveAsync(ctx);
            //}

            //if (!IsJoined)
            //    await Join(ctx);
            if (!_guilds.Any(guild => guild.Id == ctx.Channel.GuildId & guild.IsJoined))
                await Join(ctx);
            var lavalinkVoice = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            //if (LavalinkVoice == null)
            //    return;

            //this.ContextChannel = ctx.Channel;

            //var trackLoad = await Lavalink.Rest.GetTracksAsync(uri).ConfigureAwait(false);
            //var loaded = lavalinkVoice.Rest.GetTracksAsync(uri).Result;
            //var track = trackLoad.Tracks.First();
            //QueuedTracks.AddLast(track);
            foreach (var lavalinkTrack in lavalinkVoice.Rest.GetTracksAsync(uri).Result.Tracks)
            {
                _queued.Add(new QueuedTrack(ctx, lavalinkTrack.Uri));
                //QueuedTracks.AddLast(track);
            }
            await _context.SaveChangesAsync();

            //if (IsActive == false & LavalinkVoice.CurrentState.CurrentTrack == null)
            //{
            //    IsActive = true;
            //    var dequeuedTrack = QueuedTracks.First();
            //    await LavalinkVoice.PlayAsync(dequeuedTrack);
            //    //QueuedTracks.RemoveFirst();
            //}

            if (_guilds.Single(guild => guild.Id == ctx.Channel.GuildId).IsActive)
                return;
            _guilds.Single(guild => guild.Id == ctx.Channel.GuildId).IsActive = true;
            var dequeued = _queued.First(track => track.GuildId == ctx.Channel.GuildId);
            await _context.SaveChangesAsync();
            await lavalinkVoice.GetGuildConnection(ctx.Guild)
                .PlayAsync(lavalinkVoice.Rest.GetTracksAsync(dequeued.Uri).Result.Tracks.First());
        }

        [Command("stop")]
        public async Task StopAsync(CommandContext ctx)
        {
            _queued.RemoveRange(_queued.Where(track => track.GuildId == ctx.Channel.GuildId));
            _dequeued.RemoveRange(_dequeued.Where(track => track.GuildId == ctx.Channel.GuildId));
            _guilds.Single(guild => guild.Id == ctx.Channel.GuildId).IsActive = false;
            await _context.SaveChangesAsync();
            await SkipAsync(ctx);
        }

        [Command("skip")]
        public async Task SkipAsync(CommandContext ctx)
        {
            var lavalinkVoice = ctx.Client.GetLavalink().ConnectedNodes.Values.First()
                .GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (lavalinkVoice == null)
                return;

            await lavalinkVoice.StopAsync();
        }

        [Command("shuffle")]
        public async Task ShuffleAsync(CommandContext ctx)
        {
            Random rnd = new Random();
            //QueuedTracks = new LinkedList<LavalinkTrack>(QueuedTracks.OrderBy(q => rnd.Next()));
            var queued = _queued.Where(track => track.GuildId == ctx.Channel.GuildId);
            _queued.RemoveRange(queued);
            _queued.AddRange(queued.OrderBy(track => rnd.Next()));
            await _context.SaveChangesAsync();
            await ctx.RespondAsync("Очередь перемешана");
        }

        [Command("prev")]
        public async Task PreviousAsync(CommandContext ctx)
        {
            //QueuedTracks.AddFirst(DequeuedTracks.Pop());
            //QueuedTracks.AddFirst(DequeuedTracks.Pop());
            //await LavalinkVoice.StopAsync();
            for (int i = 0; i < 2; i++)
            {
                Track dequeued = _dequeued.Last(track => track.GuildId == ctx.Channel.GuildId);
                _queued.ToList().Insert(0, (QueuedTrack)dequeued);
                _dequeued.Remove((DequeuedTrack) dequeued);
                _context.SaveChanges();
            }

            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
        }

        [Command("queue")]
        public async Task QueueAsync(CommandContext ctx)
        {
            //var track = ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).CurrentState.CurrentTrack;
            //var queue = _queued.Where(track => track.GuildId == ctx.Channel.GuildId).ToArray().Select((x, y) =>
            //            $"{y + 1} - {Formatter.Bold(Formatter.Sanitize(x.Title))} | {Formatter.Bold(Formatter.Sanitize(x.Author))}")
            //        .Aggregate((x, y) => x + "\n" + y);

            //var track = LavalinkVoice.CurrentState.CurrentTrack;
            //var queue = QueuedTracks.ToArray().Select((x, y) =>
            //        $"{y + 1} - {Formatter.Bold(Formatter.Sanitize(x.Title))} | {Formatter.Bold(Formatter.Sanitize(x.Author))}")
            //    .Aggregate((x, y) => x + "\n" + y);

            //await ctx.RespondAsync($"Сейчас играет: {Formatter.Bold(Formatter.Sanitize(track.Title))} | {Formatter.Bold(Formatter.Sanitize(track.Author))}.\n" + queue);
        }

        [Command, Description("Leaves a voice channel.")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            var lavalink = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var lavalinkVoice = lavalink.GetGuildConnection(ctx.Member.VoiceState.Guild);
            var guild = _guilds.Single(guild => guild.Id == ctx.Channel.GuildId);
            //if (this.LavalinkVoice == null)
            //    return;
            if (lavalinkVoice.IsConnected)
            {
                await lavalinkVoice.DisconnectAsync().ConfigureAwait(false);
            }
            lavalink = null;
            lavalinkVoice = null;
            _queued.RemoveRange(_queued.Where(track => track.GuildId == ctx.Channel.GuildId));
            _dequeued.RemoveRange(_dequeued.Where(track => track.GuildId == ctx.Channel.GuildId));
            guild.IsActive = false;
            guild.IsJoined = false;
            //QueuedTracks = null;
            //DequeuedTracks = null;
            //IsJoined = false;
            //IsActive = false;
            await _context.SaveChangesAsync();

            await ctx.RespondAsync("До связи").ConfigureAwait(false);
        }

        private async Task LavalinkVoice_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            //if(QueuedTracks.Count < 1)
            //    return;
            var lavalinkVoice = sender.Node;
            var dequeued = _dequeued.First(track => track.GuildId == sender.Channel.GuildId);
            await e.Player.PlayAsync(lavalinkVoice.Rest.GetTracksAsync(dequeued.Uri).Result.Tracks.First());
            //await LavalinkVoice.PlayAsync(QueuedTracks.First());
        }

        private async Task LavalinkVoice_PlaybackStarted(LavalinkGuildConnection sender, TrackStartEventArgs e)
        {
            //DequeuedTracks.Push(e.Track);
            //QueuedTracks.RemoveFirst();
            var track = e.Track;
            _dequeued.Add(new DequeuedTrack(sender.Channel.GuildId, track.Uri));
            _queued.Remove(_queued.First(track => track.GuildId == sender.Channel.GuildId));
            await _context.SaveChangesAsync();
            await sender.Channel.SendMessageAsync(
                $"Играет: {Formatter.Bold(Formatter.Sanitize(track.Title))} | {Formatter.Bold(Formatter.Sanitize(track.Author))}.");
        }
    }
}
