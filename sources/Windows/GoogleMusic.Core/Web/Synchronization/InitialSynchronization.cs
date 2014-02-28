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
    
    public interface IInitialSynchronization
    {
        Task InitializeAsync(IProgress<double> progress = null);
    }

    public class InitialSynchronization : IInitialSynchronization
    {
        private readonly ILogger logger;

        private readonly IGoogleMusicSynchronizationService synchronizationService;

        private readonly DbContext dbContext;

        public InitialSynchronization(
            ILogManager logManager,
            IGoogleMusicSynchronizationService synchronizationService)
        {
            this.dbContext = new DbContext();
            this.logger = logManager.CreateLogger("InitialSynchronization");
            this.synchronizationService = synchronizationService;
        }

        public async Task InitializeAsync(IProgress<double> progress)
        {
            await this.ClearLocalDatabaseAsync();
            await this.synchronizationService.Update(progress);
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
