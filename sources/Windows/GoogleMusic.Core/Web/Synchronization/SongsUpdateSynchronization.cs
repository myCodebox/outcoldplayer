// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web.Models;

    using SQLite;

    public class SongsUpdateSynchronization
    {
        public async Task UpdateSongsAsync(IEnumerable<GoogleMusicSong> googleMusicSongs)
        {
            if (googleMusicSongs == null)
            {
                throw new ArgumentNullException("googleMusicSongs");
            }

            var dbContext = new DbContext();
            var asyncConnection = dbContext.CreateConnection();

            await asyncConnection.RunInTransactionAsync((connection) => this.UpdateSongs(googleMusicSongs, connection));
        }

        private void UpdateSongs(IEnumerable<GoogleMusicSong> googleMusicSongs, SQLiteConnection connection)
        {
            foreach (var song in googleMusicSongs)
            {
                var songId = song.Id;
                var storedSong = connection.Find<Song>(x => x.ProviderSongId == songId);

                if (song.Deleted)
                {
                    this.DeleteSong(connection, storedSong);
                }
                else
                {
                    if (storedSong != null)
                    {
                        this.UpdateSong(connection, storedSong, song.ToSong());
                    }
                    else
                    {
                        //connection.Insert((Song)song);
                    }
                }
            }
        }

        private void DeleteSong(SQLiteConnection connection, Song song)
        {
            this.UpdatePlaylistOnSongDeleted<Genre>(connection, song, song.GenreId);
            var album = this.UpdatePlaylistOnSongDeleted<Album>(connection, song, song.AlbumId);
            var albumArtist = this.UpdatePlaylistOnSongDeleted<Artist>(connection, song, album.ArtistId);
            if (song.ArtistId != album.ArtistId)
            {
                var songArtist = this.UpdatePlaylistOnSongDeleted<Artist>(connection, song, song.ArtistId);
                if (songArtist.AlbumsCount == 0 && songArtist.SongsCount == 0)
                {
                    connection.Delete(songArtist);
                }
            }

            if (album.SongsCount == 0)
            {
                albumArtist.AlbumsCount--;
                if (albumArtist.AlbumsCount == 0 && albumArtist.SongsCount == 0)
                {
                    connection.Delete(albumArtist);
                }
            }
        }

        private TPlaylist UpdatePlaylistOnSongDeleted<TPlaylist>(SQLiteConnection connection, Song song, int playlistId) 
            where TPlaylist : class, IPlaylist, new()
        {
            var playlist = connection.Find<TPlaylist>(a => a.Id == playlistId);
            if (playlist != null)
            {
                playlist.SongsCount--;
                playlist.Duration -= song.Duration;

                if (playlist.SongsCount == 0)
                {
                    connection.Delete(playlist);
                }
                else
                {
                    if (playlist.ArtUrl == song.AlbumArtUrl)
                    {
                        Song updateAlbumArt = connection.Find<Song>(s => playlistId == playlist.Id && s.AlbumArtUrl != null);
                        if (updateAlbumArt != null)
                        {
                            playlist.ArtUrl = updateAlbumArt.AlbumArtUrl;
                        }
                    }

                    if (playlist.LastPlayed == song.LastPlayed)
                    {
                        var lastPlayedSong = connection.Table<Song>()
                                  .Where(s => s.AlbumId == playlist.Id)
                                  .OrderByDescending(s => s.LastPlayed)
                                  .FirstOrDefault();

                        playlist.LastPlayed = lastPlayedSong.LastPlayed;
                    }

                    connection.Update(playlist);
                }
            }

            return playlist;
        }

        private void UpdateSong(SQLiteConnection connection, Song storedSong, Song newMetadata)
        {
            
        }
    }
}
