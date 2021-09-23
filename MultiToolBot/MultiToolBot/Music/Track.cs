using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MultiToolBot.Music
{
    public class Track
    {
        public Guid id = Guid.NewGuid();
        public string path;
        protected YoutubeClient _client;
        protected Video _video;
        public string _url;

        public Track(string url)
        {
            _url = url;
            _client = new YoutubeClient();
        }

        public Track() { }

        public async Task<string> DownloadAsync(string directory)
        {
            _video = await _client.Videos.GetAsync(_url);

            var streamManifest = await _client.Videos.Streams.GetManifestAsync(_url.Split('=')[1]);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();

            var path = $"{directory}/{id}.{streamInfo.Container}";
            await _client.Videos.Streams.DownloadAsync(streamInfo, path);

            return this.path = path;
        } 
    }
}