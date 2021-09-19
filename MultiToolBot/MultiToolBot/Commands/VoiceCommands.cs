using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.VoiceNext;
using MultiToolBot.Music;
using Newtonsoft.Json;

namespace MultiToolBot.Commands
{
    class VoiceCommands : BaseCommandModule
    {
        private object locker1 = new object(), locker2 = new object();

        [Command("play"), Description("Plays an audio file.")]
        public async Task Play(CommandContext ctx, [RemainingText, Description("Ссылка на трэк")] string url)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("Внутренняя ошибка");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await Join(ctx);
                vnc = vnext.GetConnection(ctx.Guild);
            }

            var track = new QueuedTrack(url);
            List<QueuedTrack> queuedTracks = new List<QueuedTrack>();

            lock (locker1)
            {
                
                queuedTracks =
                    JsonConvert.DeserializeObject<List<QueuedTrack>>(
                        File.ReadAllText($"{ctx.Channel.ParentId}/queue.json"));

                if (queuedTracks.Count == 0)
                {
                    track.QueueNumber = 1;
                }
                else
                {
                    track.QueueNumber = queuedTracks.Count + 1;
                }

                queuedTracks.Add(track);
                var serialized = JsonConvert.SerializeObject(queuedTracks);

                File.WriteAllText($"{ctx.Channel.ParentId}/queue.json", serialized);
            }

            track.path = await track.DownloadAsync(ctx.Channel.ParentId.ToString());

            lock (locker2)
            {
                var que = JsonConvert.DeserializeObject<List<QueuedTrack>>(
                    File.ReadAllText($"{ctx.Channel.ParentId}/queue.json"));
                var current = que.Single(q => q.id == track.id);
                current.path = track.Path;
                //current = track;
                var serialized = JsonConvert.SerializeObject(que);
                File.WriteAllText($"{ctx.Channel.ParentId}/queue.json", serialized);
            }

            while (vnc.IsPlaying & track.id != queuedTracks.First(q => q.IsPlayed == false).id)
                await vnc.WaitForPlaybackFinishAsync();

            queuedTracks =
                JsonConvert.DeserializeObject<List<QueuedTrack>>(
                    File.ReadAllText($"{ctx.Channel.ParentId}/queue.json"));

            Exception exc = null;
            await ctx.Message.RespondAsync($"Играет `{track.Title} | {track.Author} | {track.Duration}`");

            try
            {
                await vnc.SendSpeakingAsync(true);

                var playingTrack = queuedTracks.First(q => q.IsPlayed == false);

                if (playingTrack.Path == null)
                    playingTrack.path = $"{ctx.Channel.ParentId}/{track.id}.mp4";

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg.exe",
                    Arguments = $@"-i ""{queuedTracks.First(q => q.IsPlayed == false).Path}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                    RedirectStandardOutput = true,
                    UseShellExecute = false
                };
                var ffmpeg = Process.Start(psi);
                var ffout = ffmpeg.StandardOutput.BaseStream;

                var txStream = vnc.GetTransmitSink();
                await ffout.CopyToAsync(txStream);
                await txStream.FlushAsync();


                var currentTrack = queuedTracks.First(q => q.IsPlayed == false);
                currentTrack.IsPlayed = true;
                File.WriteAllText($"{ctx.Channel.ParentId}/queue.json", JsonConvert.SerializeObject(queuedTracks));
            }
            catch (Exception ex) { exc = ex; }
            finally
            {
                await vnc.SendSpeakingAsync(false);
                await ctx.Message.RespondAsync($"Закончил игру `{track.Title}`");
            }

            if (exc != null)
                await ctx.RespondAsync($"Исключение во время воспроизведения: `{exc.GetType()}: {exc.Message}`");
        }

        [Command("join"), Description("Joins a voice channel.")]
        public async Task Join(CommandContext ctx, DiscordChannel chn = null)
        {

            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                // not enabled
                await ctx.RespondAsync("Внутренняя ошибка");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc != null)
            {
                await ctx.RespondAsync("Уже на канале");
                return;
            }

            var vstat = ctx.Member?.VoiceState;
            if (vstat?.Channel == null && chn == null)
            {
                // they did not specify a channel and are not in one
                await ctx.RespondAsync("Зайдите в канал");
                return;
            }

            if (chn == null)
                chn = vstat.Channel;
            Directory.Delete(ctx.Channel.ParentId.ToString(), true);
            Directory.CreateDirectory(ctx.Channel.ParentId.Value.ToString());
            if (!File.Exists($"{ctx.Channel.ParentId}/queue.json"))
            {
                File.WriteAllText($"{ctx.Channel.ParentId}/queue.json", JsonConvert.SerializeObject(new List<QueuedTrack>()));
            }

            // connect
            vnc = await vnext.ConnectAsync(chn);
            await ctx.RespondAsync($"Подключен к `{chn.Name}`");
        }

        [Command("leave"), Description("Leaves a voice channel.")]
        public async Task Leave(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            if (vnext == null)
            {
                await ctx.RespondAsync("Внутренняя ошибка");
                return;
            }

            var vnc = vnext.GetConnection(ctx.Guild);
            if (vnc == null)
            {
                await ctx.RespondAsync("Я никуда не подключен");
                return;
            }

            vnc.Disconnect();
            await ctx.RespondAsync("Улетучиваюсь");
        }

        [Command("skip")]
        public async Task Skip(CommandContext ctx)
        {
            var vnext = ctx.Client.GetVoiceNext();
            var vnc = vnext.GetConnection(ctx.Guild);

            var queuedTracks =
                JsonConvert.DeserializeObject<List<QueuedTrack>>(
                    File.ReadAllText($"{ctx.Channel.ParentId}/queue.json"));

            Exception exc = null;

            var playingTrack = queuedTracks.First(q => q.IsPlayed == false);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $@"-i ""{queuedTracks.First(q => q.IsPlayed == false).Path}"" -ac 2 -f s16le -ar 48000 pipe:1 -loglevel quiet",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            var ffmpeg = Process.Start(psi);
            var ffout = ffmpeg.StandardOutput.BaseStream;

            var txStream = vnc.GetTransmitSink();
            await ffout.CopyToAsync(txStream);
            await txStream.FlushAsync();


            var currentTrack = queuedTracks.First(q => q.IsPlayed == false);
            currentTrack.IsPlayed = true;
            File.WriteAllText($"{ctx.Channel.ParentId}/queue.json", JsonConvert.SerializeObject(queuedTracks));

            await vnc.SendSpeakingAsync(false);
            //await ctx.Message.RespondAsync($"Играет `{track.Title} | {track.Author} | {track.Duration}`");
        }
    }
}
