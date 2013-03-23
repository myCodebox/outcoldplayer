// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;

    public interface ISongsRepository
    {
        Task<Song> GetSongAsync(int songId);

        Task<IList<Song>> SearchAsync(string searchQuery, uint? take = null);

        Task UpdateAsync(IEnumerable<Song> songs);

        Task InsertAsync(IEnumerable<Song> songs);

        Task DeleteAsync(IEnumerable<Song> songs);
    }

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private const string SqlSearchSongs = @"
select s.* ,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistTitleNorm] as [Album.ArtistTitleNorm],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],
       a.[GenreTitleNorm] as [Album.GenreTitleNorm],
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Album a on s.[AlbumTitleNorm] = a.[TitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]     
     inner join Artist ta on ta.[TitleNorm] = a.[ArtistTitleNorm] 
where s.[TitleNorm] like ?1
order by s.[TitleNorm]
";

        private const string SqlSong = @"
select s.* ,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistTitleNorm] as [Album.ArtistTitleNorm],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],
       a.[GenreTitleNorm] as [Album.GenreTitleNorm],
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Album a on s.[AlbumTitleNorm] = a.[TitleNorm] and coalesce(nullif(s.AlbumArtistTitleNorm, ''), s.[ArtistTitleNorm]) = a.[ArtistTitleNorm]    
     inner join Artist ta on ta.[TitleNorm] = a.[ArtistTitleNorm]  
where s.[SongId] = ?1
";

        public async Task<IList<Song>> SearchAsync(string searchQuery, uint? take = null)
        {
            var searchQueryNorm = searchQuery.Normalize() ?? string.Empty;

            var sql = new StringBuilder(SqlSearchSongs);

            if (take.HasValue)
            {
                sql.AppendFormat(" limit {0}", take.Value);
            }

            return await this.Connection.QueryAsync<Song>(sql.ToString(), string.Format("%{0}%", searchQueryNorm));
        }

        public Task UpdateAsync(IEnumerable<Song> songs)
        {
            return this.Connection.RunInTransactionAsync((c) => c.UpdateAll(songs));
        }

        public Task InsertAsync(IEnumerable<Song> songs)
        {
            return this.Connection.RunInTransactionAsync((c) => c.InsertAll(songs));
        }

        public Task DeleteAsync(IEnumerable<Song> songs)
        {
            return this.Connection.RunInTransactionAsync((c) =>
                {
                    foreach (var song in songs)
                    {
                        c.Delete(song);
                    }
                });
        }

        public async Task<Song> GetSongAsync(int songId)
        {
            return (await this.Connection.QueryAsync<Song>(SqlSong, songId)).FirstOrDefault();
        }
    }
}