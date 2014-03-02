// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Threading.Tasks;

    using Windows.Storage;

    public interface IAlbumArtCacheService
    {
        Task<string> GetCachedImageAsync(Uri url, uint size);

        Task DeleteBrokenLinkAsync(Uri url, uint size);

        Task<StorageFolder> GetCacheFolderAsync();

        Task ClearCacheAsync();
    }
}