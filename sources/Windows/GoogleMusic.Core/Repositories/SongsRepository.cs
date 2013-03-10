// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Web;

    using Windows.UI.Xaml;

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private const string LastUpdateKey = "SongsCacheService_CacheFreshnessDate";

        private readonly ISongWebService songWebService;
        private readonly ISettingsService settingsService;
        private readonly ILogger logger;

        private DispatcherTimer dispatcherTimer;

        private DateTime? lastUpdate;

        public SongsRepository(
            ILogManager logManager,
            ISongWebService songWebService,
            IGoogleMusicSessionService googleMusicSessionService,
            ISettingsService settingsService)
        {
            this.songWebService = songWebService;
            this.settingsService = settingsService;
            this.logger = logManager.CreateLogger("SongsRepository");

            googleMusicSessionService.SessionCleared += async (sender, args) =>
                {
                    this.logger.Debug("Session cleared. Stopping the dispatcher and clearing the cache of songs.");
                    await this.ClearRepositoryAsync();
                };
        }

        public event Action Updated;

        public async Task InitializeAsync(IProgress<int> progress)
        {
            this.logger.Debug("Initializing.");


            this.lastUpdate = this.settingsService.GetValue(LastUpdateKey, DateTime.MinValue);
            if (this.lastUpdate == DateTime.MinValue)
            {
                var updateStart = DateTime.UtcNow;
                var loaddedSongs = (await this.songWebService.GetAllSongsAsync(progress)).Select(x => (SongMetadata)x);
                await this.Connection.InsertAllAsync(loaddedSongs);
                this.lastUpdate = updateStart;
            }
            else
            {
                progress.Report(await this.Connection.Table<SongMetadata>().CountAsync());

                await this.UpdateSongsAsync();
            }

            this.logger.Debug("Initialized. Creating dispatcher timer.");

            this.dispatcherTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
#if NETFX_CORE
            this.dispatcherTimer.Tick += async (sender, o) => await this.UpdateSongsAsync();
#endif
            this.dispatcherTimer.Start();
        }

        public async Task<Song> GetSongAsync(string songId)
        {
            if (this.lastUpdate == null)
            {
                throw new NotSupportedException("Songs Repository is not initialized yet.");
            }

            return new Song(await this.Connection.GetAsync<SongMetadata>(songId));
        }

        public async Task<IEnumerable<Song>> GetAllAsync()
        {
            return (await this.Connection.Table<SongMetadata>().ToListAsync()).Select(x => new Song(x));
        }

        public async Task ClearRepositoryAsync()
        {
            this.dispatcherTimer.Stop();
            this.dispatcherTimer = null;
            await this.Connection.ExecuteAsync("delete from SongMetadata");

            this.lastUpdate = null;

            this.RaiseUpdated();
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
                await this.Connection.RunInTransactionAsync(connection =>
                    {
                        foreach (var song in updatedSongs)
                        {
                            if (song.Deleted)
                            {
                                connection.Delete(song.Id);
                            }
                            else
                            {
                                connection.Update((SongMetadata)song);
                            }
                        }
                    });

                this.RaiseUpdated();
            }
            
            this.lastUpdate = updateStart;
            this.settingsService.SetValue(LastUpdateKey, this.lastUpdate.Value);
        }

        private Song CreateSong(SongMetadata metadata)
        {
            var song = new Song(metadata);
            song.PropertyChanged += this.SongOnPropertyChanged;
            return song;
        }

        private async void SongOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var song = sender as Song;
            if (song != null)
            {
                if (!string.Equals(
                    propertyChangedEventArgs.PropertyName,
                    PropertyNameExtractor.GetPropertyName(() => song.State),
                    StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(
                    propertyChangedEventArgs.PropertyName,
                    PropertyNameExtractor.GetPropertyName(() => song.IsPlaying),
                    StringComparison.OrdinalIgnoreCase))
                {
                    await this.Connection.UpdateAsync(((Song)sender).Metadata);
                }
            }
        }
    }
}