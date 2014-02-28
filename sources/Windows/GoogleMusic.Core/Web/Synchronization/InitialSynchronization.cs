// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;

    public interface IInitialSynchronization
    {
        Task InitializeAsync(IProgress<double> progress = null);
    }

    public class InitialSynchronization : IInitialSynchronization
    {
        private readonly ILogger logger;

        private readonly IGoogleMusicSynchronizationService synchronizationService;

        private readonly ISongsCachingService songsCachingService;

        private readonly IAlbumArtCacheService albumArtCacheService;

        private readonly DbContext dbContext;

        public InitialSynchronization(
            ILogManager logManager,
            IGoogleMusicSynchronizationService synchronizationService,
            ISongsCachingService songsCachingService,
            IAlbumArtCacheService albumArtCacheService)
        {
            this.dbContext = new DbContext();
            this.logger = logManager.CreateLogger("InitialSynchronization");
            this.synchronizationService = synchronizationService;
            this.songsCachingService = songsCachingService;
            this.albumArtCacheService = albumArtCacheService;
        }

        public async Task InitializeAsync(IProgress<double> progress)
        {
            await this.ClearLocalDatabaseAsync();
            await this.albumArtCacheService.ClearCacheAsync();

            await this.synchronizationService.Update(progress);

            await this.songsCachingService.RestoreCacheAsync();
        }

        private async Task ClearLocalDatabaseAsync()
        {
            await this.dbContext.CreateConnection().RunInTransactionAsync(
                c =>
                {
                    c.DeleteAll<UserPlaylist>();
                    c.DeleteAll<UserPlaylistEntry>();
                    c.DeleteAll<Album>();
                    c.DeleteAll<Artist>();
                    c.DeleteAll<Genre>();
                    c.DeleteAll<Song>();
                    c.DeleteAll<CachedSong>();
                    c.DeleteAll<CachedAlbumArt>();
                });
        }
    }
}
