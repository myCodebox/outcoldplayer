// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reactive.Linq;

    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.Models;

    public interface IRatingCacheService
    {
        void ClearCache();

        Tuple<DateTime, byte> GetCachedRating(Song song);
    }

    public class RatingCacheService : IRatingCacheService
    {
        private readonly ConcurrentDictionary<string, Tuple<DateTime, byte>> cache = new ConcurrentDictionary<string, Tuple<DateTime, byte>>();
        private DateTime lastCacheClear = DateTime.Now;

        public RatingCacheService(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<SongsUpdatedEvent>()
                .Where(e => e.UpdatedSongs != null)
                .Subscribe(this.UpdatedSongs);

        }

        public void ClearCache()
        {
            var now = DateTime.Now;
            if ((now - this.lastCacheClear).TotalMinutes > 5)
            {
                var oldCache = this.cache.Where(x => (now - x.Value.Item1).TotalMinutes > 5).ToList();
                foreach (var key in oldCache)
                {
                    Tuple<DateTime, byte> c;
                    this.cache.TryRemove(key.Key, out c);
                }

                this.lastCacheClear = now;
            }
        }

        public Tuple<DateTime, byte> GetCachedRating(Song song)
        {
            Tuple<DateTime, byte> value;
            if (this.cache.TryGetValue(song.SongId, out value))
            {
                return value;
            }

            return null;
        }

        private void UpdatedSongs(SongsUpdatedEvent e)
        {
            if (e.UpdatedSongs != null)
            {
                foreach (var unknownSong in e.UpdatedSongs.Where(x => x.UnknownSong))
                {
                    var song = unknownSong;
                    this.cache.AddOrUpdate(
                        song.SongId,
                        Tuple.Create(DateTime.UtcNow, song.Rating),
                        (s, tuple) => Tuple.Create(DateTime.UtcNow, song.Rating));
                }
            }
        }
    }
}
