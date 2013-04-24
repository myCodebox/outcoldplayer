// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ICachedAlbumArtsRepository
    {
        Task<CachedAlbumArt> FindAsync(Uri path);

        Task AddAsync(CachedAlbumArt cache);
    }

    public class CachedAlbumArtsRepository : RepositoryBase, ICachedAlbumArtsRepository
    {
        public Task<CachedAlbumArt> FindAsync(Uri path)
        {
            return this.Connection.FindAsync<CachedAlbumArt>(path);
        }

        public Task AddAsync(CachedAlbumArt cache)
        {
            return this.Connection.InsertAsync(cache);
        }
    }
}