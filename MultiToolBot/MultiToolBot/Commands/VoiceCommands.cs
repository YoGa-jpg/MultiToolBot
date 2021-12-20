using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

namespace MultiToolBot.Commands
{
    class VoiceCommands : BaseCommandModule
    {
        private readonly GuildContext _context = new GuildContext();
        private readonly DbSet<Track> _tracks;
        private readonly DbSet<Guild> _guilds;

        public VoiceCommands()
        {
            _tracks = _context.Tracks;
            _guilds = _context.DiscordGuilds;
        }

        [Command]
        public async Task Join(CommandContext ctx, DiscordChannel channel = null)
        {
            channel = ctx.Member.VoiceState.Channel;
            var lava = ctx.Client.GetLavalink();
            var lavalink = lava.ConnectedNodes.Values.First(); // Lavalink
            var connection = lavalink.GetGuildConnection(channel.Guild) is null;


            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("Внутренняя ошибка");
                return;
            }
            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Некорректный канал");
                return;
            }

            await lavalink.ConnectAsync(channel);
            Guild guild = new Guild();
            try
            {
                guild = _guilds.Single(gui => gui.Id == ctx.Guild.Id);
            }
            catch (Exception e)
            {
                _guilds.Add(new Guild(channel.GuildId));
                await _context.SaveChangesAsync();
            }
            finally
            {
                guild = _guilds.Single(gui => gui.Id == ctx.Guild.Id);
            }

            if (guild.IsJoined & connection)
            {
                await StopAsync(ctx);
            }

            //if (!guild.IsConfigured)
            //{
            //    lavalink.PlaybackStarted += LavalinkVoice_PlaybackStarted;
            //    lavalink.PlaybackFinished += LavalinkVoice_PlaybackFinished;
            //    guild.IsConfigured = true;
            //}

            lavalink.PlaybackStarted -= LavalinkVoice_PlaybackStarted;
            lavalink.PlaybackFinished -= LavalinkVoice_PlaybackFinished;
            lavalink.PlaybackStarted += LavalinkVoice_PlaybackStarted;
            lavalink.PlaybackFinished += LavalinkVoice_PlaybackFinished;

            guild.IsConfigured = true;
            guild.IsJoined = true;
            await _context.SaveChangesAsync();

            await ctx.RespondAsync($"Зашел в {channel.Name}!");
        }

        [Command, DSharpPlus.CommandsNext.Attributes.Description("Добавление в очередь")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] Uri uri)
        {
            if (!(_guilds.Any(guild => guild.Id == ctx.Channel.GuildId) &
                  ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild) != null))
                await Join(ctx);

            var lavalink = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var guild = _guilds.Include(gui => gui.TextChannel).Single(guild => guild.Id == ctx.Channel.GuildId);

            if (guild.TextChannel is null)
            {
                _context.TextChannels.Add(new TextChannel(ctx.Channel.Id, ctx.Guild.Id));
            }
            else
            {
                if (guild.TextChannel.Id != ctx.Channel.Id)
                {
                    guild.TextChannel.Id = ctx.Channel.Id;

                }
            }

            foreach (var lavalinkTrack in lavalink.Rest.GetTracksAsync(uri).Result.Tracks)
            {
                _tracks.Add(new Track(ctx, lavalinkTrack.Uri));
            }

            try
            {
                if(_context.ChangeTracker.HasChanges())
                    _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            if (guild.IsActive)
                return;
            guild.IsActive = true;
            var track = _tracks.First(tr => tr.GuildId == ctx.Channel.GuildId);
            track.Pointer = true;
            await _context.SaveChangesAsync();
            var tracks = lavalink.Rest.GetTracksAsync(track.Uri).Result;
            await lavalink.GetGuildConnection(ctx.Guild)
                .PlayAsync(tracks.Tracks.First());
        }

        [Command]
        public async Task StopAsync(CommandContext ctx)
        {
            _tracks.RemoveRange(_tracks.Where(track => track.GuildId == ctx.Channel.GuildId));
            _guilds.Single(guild => guild.Id == ctx.Channel.GuildId).IsActive = false;
            await _context.SaveChangesAsync();
            await SkipAsync(ctx);
        }
        [Command]
        public async Task PauseAsync(CommandContext ctx)
        {
            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).PauseAsync();
        }

        [Command]
        public async Task ResumeAsync(CommandContext ctx)
        {
            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).ResumeAsync();
        }

        [Command]
        public async Task SkipAsync(CommandContext ctx)
        {
            var lavalinkVoice = ctx.Client.GetLavalink().ConnectedNodes.Values.First()
                .GetGuildConnection(ctx.Member.VoiceState.Guild);
            if (lavalinkVoice == null)
                return;

            await lavalinkVoice.StopAsync();
        }

        [Command]
        public async Task ShuffleAsync(CommandContext ctx)
        {
            var guild = _guilds.Single(gld => gld.Id == ctx.Guild.Id);
            var tracks = _tracks.ToList().SkipWhile(tra => !tra.Pointer).Skip(1).Where(tra => tra.GuildId == guild.Id);

            try
            {
                var enumerable = tracks as Track[] ?? tracks.ToArray();
                _tracks.RemoveRange(enumerable);
                _context.SaveChanges();

                enumerable = enumerable.OrderBy(tra => new SecureRandom().Next()).ToArray();
                foreach (var track in enumerable)
                {
                    _tracks.Add((Track)track.Clone());
                }
                _context.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            await ctx.RespondAsync("Очередь перемешана");
        }

        [Command]
        public async Task PrevAsync(CommandContext ctx)
        {
            var track = _tracks.ToList().TakeWhile(trc => !trc.Pointer & trc.GuildId == ctx.Guild.Id).TakeLast(2).First();
            _tracks.First(trc => trc.GuildId == ctx.Guild.Id & trc.Pointer).Pointer = false;
            track.Pointer = true;

            await _context.SaveChangesAsync();
            await ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild).StopAsync();
        }

        [Command]
        public async Task QueueAsync(CommandContext ctx)
        {
            var lavalinkVoice = ctx.Client.GetLavalink().GetGuildConnection(ctx.Guild);
            var tracks = _tracks.ToList().Where(trc => trc.GuildId == ctx.Guild.Id).SkipWhile(trc => !trc.Pointer);
            StringBuilder message = new StringBuilder(1500);
            foreach (var track in tracks)
            {
                var trackList = lavalinkVoice.GetTracksAsync(track.Uri).Result.Tracks;
                if (trackList == null | message.Length > 1500)
                    break;
                var trackInfo = trackList.First();
                if (track.Pointer)
                    message.AppendLine(
                        $"Сейчас играет: {Formatter.Bold(Formatter.Sanitize(trackInfo.Title))} | {Formatter.Bold(Formatter.Sanitize(trackInfo.Author))}");
                else
                    message.AppendLine(
                        $"- {Formatter.Bold(Formatter.Sanitize(trackInfo.Title))} | {Formatter.Bold(Formatter.Sanitize(trackInfo.Author))}");
            }
            await ctx.Channel.SendMessageAsync(message.ToString());
        }

        [Command, DSharpPlus.CommandsNext.Attributes.Description("Leaves a voice channel.")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            var lavalink = ctx.Client.GetLavalink().ConnectedNodes.Values.First();
            var lavalinkVoice = lavalink.GetGuildConnection(ctx.Member.VoiceState.Guild);
            var guild = _guilds.Single(guild => guild.Id == ctx.Channel.GuildId);

            if (lavalinkVoice.IsConnected)
            {
                await lavalinkVoice.DisconnectAsync().ConfigureAwait(false);
            }

            _tracks.RemoveRange(_tracks.Where(track => track.GuildId == ctx.Guild.Id));
            guild.IsActive = false;
            guild.IsJoined = false;

            await _context.SaveChangesAsync();
            await ctx.RespondAsync("До связи").ConfigureAwait(false);
        }

        private async Task LavalinkVoice_PlaybackFinished(LavalinkGuildConnection sender, TrackFinishEventArgs e)
        {
            var track = _tracks.ToList().Where(trc => trc.GuildId == sender.Channel.GuildId)
                .SkipWhile(trc => !trc.Pointer).Skip(1).First();
            track.Pointer = true;
            _tracks.First(trc => trc.GuildId == sender.Guild.Id & trc.Pointer).Pointer = false;
            await _context.SaveChangesAsync();
            await e.Player.PlayAsync(sender.GetTracksAsync(track.Uri).Result.Tracks.First());
        }

        private async Task LavalinkVoice_PlaybackStarted(LavalinkGuildConnection sender, TrackStartEventArgs e)
        {
            var channel = sender.Guild.GetChannel(_guilds.Include(guild => guild.TextChannel).Single(guild => guild.Id == sender.Guild.Id).TextChannel.Id);
            await channel.SendMessageAsync(
                $"Играет: {Formatter.Bold(Formatter.Sanitize(e.Track.Title))} | {Formatter.Bold(Formatter.Sanitize(e.Track.Author))}.");
        }
    }
}
