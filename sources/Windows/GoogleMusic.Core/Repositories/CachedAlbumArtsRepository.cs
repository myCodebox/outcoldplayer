// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ICachedAlbumArtsRepository
    {
        Task<CachedAlbumArt> FindAsync(Uri path, uint size);

        Task AddAsync(CachedAlbumArt cache);

        Task<IList<CachedAlbumArt>> GetRemovedCachedItemsAsync();

        Task DeleteCachedItemsAsync(IEnumerable<CachedAlbumArt> cachedAlbumArts);

        Task ClearCacheAsync();

        Task DeleteBrokenLinkAsync(Uri url, uint size);
    }

    public class CachedAlbumArtsRepository : RepositoryBase, ICachedAlbumArtsRepository
    {
        private const string SqlRemovedCachedItems = @"select distinct c.* 
from CachedAlbumArt c
     left join Song s on s.AlbumArtUrl = c.AlbumArtUrl     
     left join Album a on a.ArtUrl = c.AlbumArtUrl
     left join Artist aa on aa.ArtUrl = c.AlbumArtUrl
     left join Radio r on r.ArtUrl = c.AlbumArtUrl or r.ArtUrl1 = c.AlbumArtUrl or r.ArtUrl2 = c.AlbumArtUrl or r.ArtUrl3 = c.AlbumArtUrl
 where s.[SongId] is null and a.[AlbumId] is null and aa.ArtistId is null and r.[RadioID] is null";

        public Task<CachedAlbumArt> FindAsync(Uri path, uint size)
        {
            return this.Connection.Table<CachedAlbumArt>().Where(c => c.Size == size && c.AlbumArtUrl == path).FirstOrDefaultAsync();
        }

        public Task AddAsync(CachedAlbumArt cache)
        {
            return this.Connection.InsertAsync(cache);
        }

        public async Task<IList<CachedAlbumArt>> GetRemovedCachedItemsAsync()
        {
            return await this.Connection.QueryAsync<CachedAlbumArt>(SqlRemovedCachedItems);
        }

        public Task DeleteCachedItemsAsync(IEnumerable<CachedAlbumArt> cachedAlbumArts)
        {
            return this.Connection.RunInTransactionAsync(
                c =>
                    {
                        foreach (var cachedAlbumArt in cachedAlbumArts)
                        {
                            c.Delete(cachedAlbumArt);
                        }
                    });
        }

        public Task ClearCacheAsync()
        {
            return this.Connection.RunInTransactionAsync(c => c.DeleteAll<CachedAlbumArt>());
        }


        public async Task DeleteBrokenLinkAsync(Uri url, uint size)
        {
            var brokenCache = await this.FindAsync(url, size);
            if (brokenCache != null)
            {
                await this.Connection.DeleteAsync(brokenCache);
            }
        }
    }
}