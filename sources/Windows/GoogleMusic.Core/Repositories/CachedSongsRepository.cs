// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ICachedSongsRepository
    {
        Task<IList<CachedSong>> GetDeadCacheAsync();

        Task AddAsync(CachedSong item);

        Task RemoveAsync(CachedSong item);

        Task UpdateAsync(CachedSong item);

        Task<CachedSong> GetNextAsync();

        Task<CachedSong> FindAsync(Song song);

        Task ClearCacheAsync();
    }

    public class CachedSongsRepository : RepositoryBase, ICachedSongsRepository
    {
        private const string SqlNextTask = @"
select t.*,
    s.[SongId] as [Song.SongId],
    s.[ProviderSongId] as [Song.ProviderSongId],
    s.[Title] as [Song.Title],
    s.[TitleNorm] as [Song.TitleNorm],
    s.[Duration] as [Song.Duration],
    s.[Composer] as [Song.Composer],
    s.[PlayCount] as [Song.PlayCount],
    s.[Rating] as [Song.Rating],
    s.[Disc] as [Song.Disc],
    s.[TotalDiscs] as [Song.TotalDiscs],
    s.[Track] as [Song.Track],
    s.[TotalTracks] as [Song.TotalTracks],
    s.[Year] as [Song.Year],
    s.[AlbumArtUrl] as [Song.AlbumArtUrl],
    s.[LastPlayed] as [Song.LastPlayed],
    s.[CreationDate] as [Song.CreationDate],
    s.[Comment] as [Song.Comment],
    s.[Bitrate] as [Song.Bitrate],
    s.[StreamType] as [Song.StreamType],
    s.[AlbumArtistTitle] as [Song.AlbumArtistTitle],
    s.[AlbumArtistTitleNorm] as [Song.AlbumArtistTitleNorm],
    s.[ArtistTitle] as [Song.ArtistTitle],
    s.[ArtistTitleNorm] as [Song.ArtistTitleNorm],
    s.[AlbumTitle] as [Song.AlbumTitle],
    s.[AlbumTitleNorm] as [Song.AlbumTitleNorm],
    s.[GenreTitle] as [Song.GenreTitle],
    s.[GenreTitleNorm] as [Song.GenreTitleNorm]
from [CachedSong] as t
     inner join [Song] s on s.SongId = t.SongId
where t.[FileName] is null
order by t.[TaskAdded]
limit 1
";

        private const string SqlAllDeadCacheTasks = @"
select t.*
from [CachedSong] as t
     left join [Song] s on s.SongId = t.SongId
where s.SongId is null
";

        public async Task<IList<CachedSong>> GetDeadCacheAsync()
        {
            return await this.Connection.QueryAsync<CachedSong>(SqlAllDeadCacheTasks);
        }

        public Task AddAsync(CachedSong item)
        {
            return this.Connection.RunInTransactionAsync(
                c =>
                    {
                        var cachedSong = c.Find<CachedSong>(item.SongId);
                        if (cachedSong == null)
                        {
                            c.Insert(item);
                        }
                    });
        }

        public Task RemoveAsync(CachedSong item)
        {
            return this.Connection.DeleteAsync(item);
        }

        public Task UpdateAsync(CachedSong item)
        {
            return this.Connection.UpdateAsync(item);
        }

        public async Task<CachedSong> GetNextAsync()
        {
            return (await this.Connection.QueryAsync<CachedSong>(SqlNextTask)).FirstOrDefault();
        }

        public async Task<CachedSong> FindAsync(Song song)
        {
            return await this.Connection.Table<CachedSong>().Where(c => song.SongId == c.SongId).FirstOrDefaultAsync();
        }

        public Task ClearCacheAsync()
        {
            return this.Connection.RunInTransactionAsync(c => c.DeleteAll<CachedSong>());
        }
    }
}