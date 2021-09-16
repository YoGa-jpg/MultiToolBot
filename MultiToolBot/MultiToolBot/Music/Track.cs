using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MultiToolBot.Music
{
    public class Track
    {
        private Guid _id = Guid.NewGuid();
        private YoutubeClient _client;
        private Video _video;
        public string Url { get; private set; }
        public string Title => _video.Title;
        public string Author => _video.Author.Title;
        public TimeSpan? Duration => _video.Duration;

        public Track(string url)
        {
            Url = url;
            _client = new YoutubeClient();
        }

        public async Task<string> DownloadAsync()
        {
            _video = await _client.Videos.GetAsync(Url);

            var streamManifest = await _client.Videos.Streams.GetManifestAsync(Url.Split('=')[1]);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var path = $"{_id}.{streamInfo.Container}";
            await _client.Videos.Streams.DownloadAsync(streamInfo, path);

            return path;
        } 
    }
}