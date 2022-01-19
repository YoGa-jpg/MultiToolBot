using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Common;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using DSharpPlus.Lavalink.EventArgs;
using Emzi0767;
using Microsoft.EntityFrameworkCore;
using MultiToolBot.Model;
using MultiToolBot.Model.MusicModel;
using MultiToolBot;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Search;

namespace MultiToolBot.Commands
{
    class VoiceCommands : BaseCommandModule
    {
        //private readonly GuildContext _context = new GuildContext();
        //private readonly GuildContext _context = new GuildContext();
        //private readonly DbSet<Track> _tracks;
        //private readonly DbSet<Guild> _guilds;
        private LinkedList<LavalinkTrack> _queue;
        private LinkedListNode<LavalinkTrack> _track;
        private DiscordChannel _channel;
        private LavalinkExtension _lavalink;
        private LavalinkNodeConnection _lavalinkNode;

        public VoiceCommands()
        {
            _queue = new LinkedList<LavalinkTrack>();
        }

        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {
            _channel = ctx.Channel;
            channel = ctx.Member.VoiceState.Channel;
            var users = channel.Users;

            _lavalink = ctx.Client.GetLavalink();
            _lavalinkNode = _lavalink.ConnectedNodes.Values.First();

            if (channel.Users.Any(user => user.IsCurrent))
            {
                return;
            }
            if (!_lavalink.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Внутренняя ошибка");
                return;
            }
            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Некорректный канал");
                return;
            }

            await _lavalinkNode.ConnectAsync(channel);

            _lavalinkNode.PlaybackStarted += LavalinkVoice_PlaybackStarted;
            _lavalinkNode.PlaybackFinished += LavalinkVoice_PlaybackFinished;

            await ctx.RespondAsync($"Зашел в {channel.Name}!");
        }

        [Command]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] string uri)
        {
            await Join(ctx);

            _lavalink = ctx.Client.GetLavalink();
            _lavalinkNode = _lavalink.ConnectedNodes.Values.First();

            LavalinkLoadResult search;

            if (!uri.Contains("https"))
            {
                uri = await GetUri(uri);
            }

            if (uri.Contains("soundcloud"))
                search = await _lavalinkNode.Rest.GetTracksAsync(uri, LavalinkSearchType.SoundCloud);
            else
                search = await _lavalinkNode.Rest.GetTracksAsync(uri);

            foreach (var lavalinkTrack in search.Tracks)
            {
                _queue.AddLast(lavalinkTrack);
            }

            if (_lavalinkNode.GetGuildConnection(ctx.Guild).CurrentState.CurrentTrack is null)
            {
                _track = _queue.First;
                await _lavalinkNode.GetGuildConnection(ctx.Guild)
                    .PlayAsync(_track.Value);
            }
        }

        [Command]
        public async Task StopAsync(CommandContext ctx)
        {
            _queue.Clear();
            await SkipAsync(ctx);
        }
        [Command]
        public async Task PauseAsync(CommandContext ctx)
        {
            _channel = ctx.Channel;
            await _lavalinkNode.GetGuildConnection(ctx.Guild).PauseAsync();
        }

        [Command]
        public async Task ResumeAsync(CommandContext ctx)
        {
            _channel = ctx.Channel;
            await _lavalinkNode.GetGuildConnection(ctx.Guild).ResumeAsync();
        }

        [Command]
        public async Task SkipAsync(CommandContext ctx)
        {
            if (_lavalinkNode == null)
                return;
            _channel = ctx.Channel;

            await _lavalinkNode.GetGuildConnection(ctx.Guild).StopAsync();
        }

        [Command]
        public async Task ShuffleAsync(CommandContext ctx)
        {
            _channel = ctx.Channel;
            var index = _queue.TakeWhile(track => track != _track.Value).Count();
            List<LavalinkTrack> toShuffle = _queue.SkipWhile(track => track != _track.Value).ToList();
            List<LavalinkTrack> toStay = _queue.Except(toShuffle).ToList();
            toShuffle.Shuffle();
            List<LavalinkTrack> shuffled = toStay.Union(toShuffle).ToList();
            _queue = new LinkedList<LavalinkTrack>(toStay.Union(shuffled));
            _track = _queue.First;
            for (int i = 0; i < index; i++)
                _track = _track.Next;
            await ctx.RespondAsync("Очередь перемешана");
        }

        [Command]
        public async Task PrevAsync(CommandContext ctx)
        {
            _channel = ctx.Channel;
            _track = _track.Previous?.Previous;

            await SkipAsync(ctx);
            //await _lavalinkNode.GetGuildConnection(ctx.Guild).PlayAsync(_track.Value);
        }

        [Command]
        public async Task QueueAsync(CommandContext ctx)
        {
            _channel = ctx.Channel;
            _lavalink = ctx.Client.GetLavalink();
            _lavalinkNode = _lavalink.ConnectedNodes.Values.First();
            var message = _queue.SkipWhile(track => track != _track.Value).Select((track, i) => i < 15 ? $"{i + 1}. {track.Title} | {track.Author}" : string.Empty)
                .Where(title => title != string.Empty)
                .Aggregate((q, w) => q + "\n" + w);
            //var lavalinkVoice = ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild);
            //var tracks = _tracks.ToList().Where(trc => trc.GuildId == ctx.Guild.Id).SkipWhile(trc => !trc.Pointer);
            //StringBuilder message = new StringBuilder(1500);
            //foreach (var track in tracks)
            //{
            //    var trackList = lavalinkVoice.GetTracksAsync(track.Uri).Result.Tracks;
            //    if (trackList == null | message.Length > 1500)
            //        break;
            //    var trackInfo = trackList.First();
            //    if (track.Pointer)
            //        message.AppendLine(
            //            $"Сейчас играет: {Formatter.Bold(Formatter.Sanitize(trackInfo.Title))} | {Formatter.Bold(Formatter.Sanitize(trackInfo.Author))}");
            //    else
            //        message.AppendLine(
            //            $"- {Formatter.Bold(Formatter.Sanitize(trackInfo.Title))} | {Formatter.Bold(Formatter.Sanitize(trackInfo.Author))}");
            //}
            await _channel.SendMessageAsync(message);
        }

        [Command]
        public async Task LeaveAsync(CommandContext ctx)
        {
            _lavalinkNode = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var guildConnection = _lavalinkNode.GetGuildConnection(ctx.Member.VoiceState.Guild);

            await guildConnection.DisconnectAsync();
            await StopAsync(ctx);

            await ctx.RespondAsync("До связи");
        }

        private async Task LavalinkVoice_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            _track = _track.Next;
            if (_track != null)
                await _lavalinkNode.ConnectedGuilds.First().Value.PlayAsync(_track.Value);
            else
                _queue.Clear();
        }

        private async Task LavalinkVoice_PlaybackStarted(LavalinkGuildConnection sender, TrackStartEventArgs e)
        {
            await _channel.SendMessageAsync(
                $"Играет: {Formatter.Bold(Formatter.Sanitize(e.Track.Title))} | {Formatter.Bold(Formatter.Sanitize(e.Track.Author))}.");
        }

        private async Task<string> GetUri(string title)
        {
            var youtube = new YoutubeClient();
            await foreach (var batch in youtube.Search.GetResultBatchesAsync(title))
            {
                foreach (var result in batch.Items)
                {
                    return result.Url;
                }
            }
            return string.Empty;
        }
    }
}
