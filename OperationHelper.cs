using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.Lavalink;
using YoutubeExplode;

namespace MultiToolBot
{
    public static class OperationHelper
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            int n = list.Count;
            while (n > 1)
            {
                byte[] box = new byte[1];
                do provider.GetBytes(box);
                while (!(box[0] < n * (Byte.MaxValue / n)));
                int k = (box[0] % n);
                n--;
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static async Task<IEnumerable<LavalinkTrack>> GetTracksAsync(this LavalinkRestClient client, string uri, bool byUri)
        {
            LavalinkLoadResult search;
            if (!byUri)
            {
                uri = await GetUri(uri);
                search = await client.GetTracksAsync(uri);
                return new Collection<LavalinkTrack>
                {
                    search.Tracks.First()
                };
            }
            search = await client.GetTracksAsync(new Uri(uri));
            return search.Tracks;
        }

        private static async Task<string> GetUri(string title)
        {
            var youtube = new YoutubeClient();
            await foreach (var batch in youtube.Search.GetResultBatchesAsync(title))
            {
                return batch.Items.First().Url;
            }
            return string.Empty;
        }
    }
}
