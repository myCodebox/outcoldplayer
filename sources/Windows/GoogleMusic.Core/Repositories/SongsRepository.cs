// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.UI.Xaml;

    public class SongsRepository : ISongsRepository
    {
        private readonly ISongWebService songWebService;
        private readonly ISongsCacheService songsCacheService;
        private readonly ILogger logger;

        private readonly Dictionary<Guid, Song> songs = new Dictionary<Guid, Song>();
        private DispatcherTimer dispatcherTimer;

        private DateTime? lastUpdate;

        public SongsRepository(
            ILogManager logManager,
            ISongWebService songWebService,
            IGoogleMusicSessionService googleMusicSessionService,
            ISongsCacheService songsCacheService)
        {
            this.songWebService = songWebService;
            this.songsCacheService = songsCacheService;
            this.logger = logManager.CreateLogger("SongsRepository");

            googleMusicSessionService.SessionCleared += async (sender, args) =>
                {
                    this.logger.Debug("Session cleared. Stopping the dispatcher and clearing the cache of songs.");

                    this.dispatcherTimer.Stop();
                    this.dispatcherTimer = null;
                    this.songs.Clear();

                    this.lastUpdate = null;

                    await this.songsCacheService.ClearCacheAsync();

                    this.RaiseUpdated();
                };
        }

        public event Action Updated;

        public async Task InitializeAsync(IProgress<int> progress)
        {
            this.logger.Debug("Initializing.");

            var cache = await this.songsCacheService.ReadFromFileAsync();
            if (cache != null)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Loaded {0} songs from cache. Last update: {1}.", cache.Songs.Length, cache.LastUpdate);
                }

                this.AddRange(cache.Songs);
                this.lastUpdate = cache.LastUpdate;

                progress.Report(cache.Songs.Length);

                await this.UpdateSongsAsync();
            }
            else
            {
                var updateStart = DateTime.UtcNow;
                this.AddRange((await this.songWebService.GetAllSongsAsync(progress)).Select(x => (SongMetadata)x));
                this.lastUpdate = updateStart;

                await this.SaveToCacheAsync();
            }

            this.logger.Debug("Initialized. Creating dispatcher timer.");

            this.dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
#if NETFX_CORE
            this.dispatcherTimer.Tick += async (sender, o) => await this.UpdateSongsAsync();
#endif
            this.dispatcherTimer.Start();
        }

        public void Clear()
        {
            lock (this.songs)
            {
                this.songs.Clear();
            }
        }

        public Song GetSong(Guid songId)
        {
            if (this.lastUpdate == null)
            {
                throw new NotSupportedException("Songs Repository is not initialized yet.");
            }

            lock (this.songs)
            {
                Song song;
                return this.songs.TryGetValue(songId, out song) ? song : null;
            }
        }

        public IEnumerable<Song> GetAll()
        {
            lock (this.songs)
            {
                return this.songs.Values.ToList();
            }
        }

        private void RaiseUpdated()
        {
            var handler = this.Updated;
            if (handler != null)
            {
                handler();
            }
        }

        private async Task UpdateSongsAsync()
        {
            var updateStart = DateTime.UtcNow;
            
            var updatedSongs = await this.songWebService.StreamingLoadAllTracksAsync(this.lastUpdate, null);
            if (updatedSongs.Count > 0)
            {
                bool updated = false;

                lock (this.songs)
                {
                    foreach (var songInfo in updatedSongs)
                    {
                        updated = true;

                        Song song;
                        bool containsSong = this.songs.TryGetValue(songInfo.Id, out song);
                        if (songInfo.Deleted)
                        {
                            if (containsSong)
                            {
                                this.songs.Remove(songInfo.Id);
                            }
                        }
                        else
                        {
                            if (containsSong)
                            {
                                song.Metadata = songInfo;
                            }
                            else
                            {
                                this.songs.Add(songInfo.Id, new Song(songInfo));
                            }
                        }
                    }

                    if (updated)
                    {
                        this.RaiseUpdated();
                    }
                }
            }
            
            this.lastUpdate = updateStart;

            if (updatedSongs.Count > 0)
            {
                await this.SaveToCacheAsync();
            }
            else
            {
                this.songsCacheService.UpdateCacheFreshness(this.lastUpdate.Value);
            }
        }

        private void AddRange(IEnumerable<SongMetadata> songInfos)
        {
            bool updated = false;

            lock (this.songs)
            {
                foreach (var songInfo in songInfos)
                {
                    updated = true;

                    Song song;
                    if (this.songs.TryGetValue(songInfo.Id, out song))
                    {
                        song.Metadata = songInfo;
                    }
                    else
                    {
                        this.songs.Add(songInfo.Id, new Song(songInfo));
                    }
                }
            }

            if (updated)
            {
                this.RaiseUpdated();
            }
        }

        private async Task SaveToCacheAsync()
        {
            if (this.lastUpdate != null)
            {
                await this.songsCacheService.SaveToFileAsync(
                    this.lastUpdate.Value, this.songs.Values.Select(x => x.Metadata).ToList());
            }
        }
    }
}