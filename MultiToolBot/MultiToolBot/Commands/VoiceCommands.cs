using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;

namespace MultiToolBot.Commands
{
    class VoiceCommands : BaseCommandModule
    {
        private LavalinkNodeConnection Lavalink { get; set; }
        private LavalinkGuildConnection LavalinkVoice { get; set; }
        private DiscordChannel ContextChannel { get; set; }
        private LinkedList<LavalinkTrack> QueuedTracks { get; set; }
        private Stack<LavalinkTrack> DequeuedTracks { get; set; }
        private bool IsActive { get; set; }

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

            Lavalink = lava.ConnectedNodes.Values.First();

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Некорректный канал");
                return;
            }

            await Lavalink.ConnectAsync(channel);
            LavalinkVoice = Lavalink.GetGuildConnection(ctx.Member.VoiceState.Guild);

            DequeuedTracks = new Stack<LavalinkTrack>();
            QueuedTracks = new LinkedList<LavalinkTrack>();

            LavalinkVoice.PlaybackFinished += LavalinkVoice_PlaybackFinished;
            LavalinkVoice.PlaybackStarted += LavalinkVoice_PlaybackStarted;

            await ctx.RespondAsync($"Зашел в {channel.Name}!");
        }

        [Command("play"), Description("Добавление в очередь")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] Uri uri)
        {
            if (LavalinkVoice == null)
                return;

            this.ContextChannel = ctx.Channel;

            var trackLoad = await Lavalink.Rest.GetTracksAsync(uri).ConfigureAwait(false);
            //var track = trackLoad.Tracks.First();

            //QueuedTracks.AddLast(track);
            foreach (var track in trackLoad.Tracks)
            {
                QueuedTracks.AddLast(track);
            }

            if (IsActive == false & LavalinkVoice.CurrentState.CurrentTrack == null)
            {
                IsActive = true;
                var dequeuedTrack = QueuedTracks.First();
                await LavalinkVoice.PlayAsync(dequeuedTrack);
                QueuedTracks.RemoveFirst();
            }
        }

        [Command("shuffle")]
        public async Task ShuffleAsync(CommandContext ctx)
        {
            Random rnd = new Random();
            QueuedTracks = new LinkedList<LavalinkTrack>(QueuedTracks.OrderBy(q => rnd.Next()));
            await ctx.RespondAsync("Очередь перемешана");
        }

        [Command("skip")]
        public async Task SkipAsync(CommandContext ctx)
        {
            if(LavalinkVoice == null)
                return;

            await LavalinkVoice.StopAsync();
        }

        [Command("prev")]
        public async Task PreviousAsync(CommandContext ctx)
        {
            QueuedTracks.AddFirst(DequeuedTracks.Pop());
            QueuedTracks.AddFirst(DequeuedTracks.Pop());
            await LavalinkVoice.StopAsync();
        }

        [Command("queue")]
        public async Task QueueAsync(CommandContext ctx)
        {
            var track = LavalinkVoice.CurrentState.CurrentTrack;
            var queue = QueuedTracks.ToArray().Select((x, y) =>
                    $"{y + 1} - {Formatter.Bold(Formatter.Sanitize(x.Title))} | {Formatter.Bold(Formatter.Sanitize(x.Author))}")
                .Aggregate((x, y) => x + "\n" + y);

            await ctx.RespondAsync($"Сейчас играет: {Formatter.Bold(Formatter.Sanitize(track.Title))} | {Formatter.Bold(Formatter.Sanitize(track.Author))}.\n" + queue);
        }

        private async Task LavalinkVoice_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            await LavalinkVoice.PlayAsync(QueuedTracks.First());
            //QueuedTracks.RemoveFirst();
        }

        private async Task LavalinkVoice_PlaybackStarted(LavalinkGuildConnection sender, TrackStartEventArgs e)
        {
            DequeuedTracks.Push(e.Track);
            QueuedTracks.RemoveFirst();
            var track = e.Track;
            await ContextChannel.SendMessageAsync(
                $"Играет: {Formatter.Bold(Formatter.Sanitize(track.Title))} | {Formatter.Bold(Formatter.Sanitize(track.Author))}.");
        }
    }
}
