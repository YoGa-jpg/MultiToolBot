using System;

namespace MultiToolBot.Music
{
    public class QueuedTrack : Track
    {
        public int QueueNumber { get; set; }
        public bool IsPlayed { get; set; }
        public string Title
        {
            get
            {
                return _video == null ? "def" : _video.Title;
            }
        }
        public string Author
        {
            get
            {
                return _video == null ? "def" : _video.Author.Title;
            }
        }

        public TimeSpan? Duration
        {
            get
            {
                return _video == null ? default(TimeSpan) : _video.Duration;
            }
        }

        public string Path => path;
        public QueuedTrack(string url) : base(url) { }

        public QueuedTrack(string url, int queue) : base(url)
        {
            QueueNumber = queue;
            IsPlayed = false;
        }

        public QueuedTrack() : base() { }
    }
}