// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;

    public class SongsRepository : RepositoryBase, ISongsRepository
    {
        private const string SqlAlbumsSongs = @"
select * ,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistId] as [Album.ArtistId],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],       
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Album a on s.AlbumId = a.AlbumId
     inner join Artist ta on ta.ArtistId = s.ArtistId 
where s.AlbumId = ?1
order by coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private const string SqlGenreSongs = @"
select s.* ,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistId] as [Album.ArtistId],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],  
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Album a on s.AlbumId = a.AlbumId     
     inner join Artist ar on a.ArtistId = ar.ArtistId
     inner join Artist ta on ta.ArtistId = s.ArtistId 
where s.GenreId = ?1
order by ar.TitleNorm, a.TitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private const string SqlUserPlaylistSongs = @"
select s.*,
       e.[Id] as [UserPlaylistEntry.Id],
       e.[PlaylistId] as [UserPlaylistEntry.PlaylistId], 
       e.[SongId] as [UserPlaylistEntry.SongId],
       e.[PlaylistOrder] as [UserPlaylistEntry.PlaylistOrder],
       e.[ProviderEntryId] as [UserPlaylistEntry.ProviderEntryId],
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistId] as [Album.ArtistId],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],  
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join UserPlaylistEntry e on e.SongId = s.SongId
     inner join Album a on s.AlbumId = a.AlbumId
     inner join Artist ta on ta.ArtistId = s.ArtistId 
where e.[PlaylistId] = ?1
order by e.[PlaylistOrder]
";

        private const string SqlArtistSongs = @"
select s.*,
       a.[AlbumId] as [Album.AlbumId],
       a.[Title] as [Album.Title],  
       a.[TitleNorm] as [Album.TitleNorm],
       a.[ArtistId] as [Album.ArtistId],
       a.[SongsCount] as [Album.SongsCount], 
       a.[Year] as [Album.Year],    
       a.[Duration] as [Album.Duration],       
       a.[ArtUrl] as [Album.ArtUrl],    
       a.[LastPlayed] as [Album.LastPlayed],  
       ta.[ArtistId] as [Artist.ArtistId],
       ta.[Title] as [Artist.Title],
       ta.[TitleNorm] as [Artist.TitleNorm],
       ta.[AlbumsCount] as [Artist.AlbumsCount],
       ta.[SongsCount] as [Artist.SongsCount],
       ta.[Duration] as [Artist.Duration],
       ta.[ArtUrl] as [Artist.ArtUrl],
       ta.[LastPlayed]  as [Artist.LastPlayed]
from [Song] as s
     inner join Album a on s.AlbumId = a.AlbumId
     inner join Artist ta on ta.ArtistId = s.ArtistId 
where a.ArtistId = ?1
order by a.[Year], a.TitleNorm, coalesce(nullif(s.Disc, 0), 1), s.Track
";

        private readonly ILogger logger;

        public SongsRepository(
            ILogManager logManager)
        {
            this.logger = logManager.CreateLogger("SongsRepository");
        }

        public async Task<SongBindingModel> GetSongAsync(string songId)
        {
            return new SongBindingModel(await this.Connection.GetAsync<Song>(songId));
        }

        public async Task<IList<SongBindingModel>> GetAllAsync()
        {
            return (await this.Connection.Table<Song>().ToListAsync()).Select(x => new SongBindingModel(x)).ToList();
        }

        public async Task<IList<Song>> GetArtistSongsAsync(Artist artist)
        {
            if (artist == null)
            {
                throw new ArgumentNullException("artist");
            }

            return await this.Connection.QueryAsync<Song>(SqlArtistSongs, artist.ArtistId);
        }

        public async Task<IList<Song>> GetAlbumSongsAsync(Album album)
        {
            if (album == null)
            {
                throw new ArgumentNullException("album");
            }

            return await this.Connection.QueryAsync<Song>(SqlAlbumsSongs, album.AlbumId);
        }

        public async Task<IList<Song>> GetGenreSongsAsync(Genre genre)
        {
            if (genre == null)
            {
                throw new ArgumentNullException("genre");
            }

            return await this.Connection.QueryAsync<Song>(SqlGenreSongs, genre.GenreId);
        }

        public async Task<IList<Song>> GetUserPlaylistSongsAsync(UserPlaylist userPlaylist)
        {
            if (userPlaylist == null)
            {
                throw new ArgumentNullException("userPlaylist");
            }

            return await this.Connection.QueryAsync<Song>(SqlUserPlaylistSongs, userPlaylist.PlaylistId);
        }
    }
}