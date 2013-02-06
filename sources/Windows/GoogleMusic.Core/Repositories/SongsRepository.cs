// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using Windows.UI.Xaml;

    public class SongsRepository : ISongsRepository
    {
        private readonly ISongWebService songWebService;

        private readonly ILogger logger;

        private readonly Dictionary<Guid, Song> songs = new Dictionary<Guid, Song>();
        private DispatcherTimer dispatcherTimer;

        public SongsRepository(
            ILogManager logManager,
            ISongWebService songWebService,
            IGoogleMusicSessionService googleMusicSessionService)
        {
            this.songWebService = songWebService;
            this.logger = logManager.CreateLogger("SongsRepository");

            googleMusicSessionService.SessionCleared += (sender, args) =>
                {
                    this.logger.Debug("Session cleared. Stopping the dispatcher and clearing the cache of songs.");

                    this.dispatcherTimer.Stop();
                    this.dispatcherTimer = null;
                    this.songs.Clear();

                    this.RaiseUpdated();
                };
        }

        public event Action Updated;

        public async Task InitializeAsync(IProgress<int> progress)
        {
            this.logger.Debug("Initializing.");

            this.AddRange(await this.songWebService.GetAllSongsAsync(progress));

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

        public Song AddOrUpdate(GoogleMusicSong songInfo)
        {
            lock (this.songs)
            {
                Song song;
                if (this.songs.TryGetValue(songInfo.Id, out song))
                {
                    song.GoogleMusicMetadata = songInfo;
                }
                else
                {
                    this.songs.Add(songInfo.Id, song = new Song(songInfo));
                }

                return song;
            }
        }

        public void AddRange(IEnumerable<GoogleMusicSong> songInfos)
        {
            bool updated = false;

            lock (this.songs)
            {
                foreach (var songInfo in songInfos)
                {
                    updated = true;

                    this.AddOrUpdate(songInfo);
                }
            }

            if (updated)
            {
                this.RaiseUpdated();
            }
        }

        public void Remove(Guid id)
        {
            lock (this.songs)
            {
                Song song;
                if (this.songs.TryGetValue(id, out song))
                {
                    this.songs.Remove(id);
                }
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
            var updatedSongs = await this.songWebService.StreamingLoadAllTracksAsync(null);
            if (updatedSongs.Count > 0)
            {
                foreach (var metadata in updatedSongs.Where(m => m.Deleted))
                {
                    this.Remove(metadata.Id);
                }

                this.AddRange(updatedSongs.Where(m => !m.Deleted));
            }
        }
    }
}