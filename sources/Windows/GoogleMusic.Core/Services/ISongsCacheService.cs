// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISongsCacheService
    {
        Task SaveToFileAsync(DateTime lastUpdate, IEnumerable<SongMetadata> songs);

        Task<SongsCache> ReadFromFileAsync();

        Task ClearCacheAsync();

        void UpdateCacheFreshness(DateTime lastUpdate);

        Task UpdateSongMedatadaAsync(SongMetadata metadata);
    }
}