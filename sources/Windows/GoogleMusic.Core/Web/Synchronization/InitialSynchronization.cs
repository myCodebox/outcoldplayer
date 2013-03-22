// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web.Synchronization
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.Models;

    using SQLite;

    internal class InitialSynchronization
    {
        private readonly Dictionary<string, ArtistContainer> artists =
            new Dictionary<string, ArtistContainer>();

        private readonly Dictionary<string, Genre> genres = 
            new Dictionary<string, Genre>();

        private readonly Dictionary<string, Song> songEntities =
            new Dictionary<string, Song>();

        private readonly List<UserPlaylistContainer> playlists =
            new List<UserPlaylistContainer>();

        public void AddRange(IList<GoogleMusicSong> googleMusicSongs)
        {
            foreach (var googleSong in googleMusicSongs)
            {
                this.AddSong(googleSong);
            }
        }

        public void AddPlaylists(IList<GoogleMusicPlaylist> googleMusicPlaylists)
        {
            for (int i = 0; i < googleMusicPlaylists.Count; i++)
            {
                var googleUserPlaylist = googleMusicPlaylists[i];

                var userPlaylist = new UserPlaylistContainer(new UserPlaylist()
                                             {
                                                 ProviderPlaylistId = googleUserPlaylist.PlaylistId,
                                                 Title = googleUserPlaylist.Title,
                                                 TitleNorm = googleUserPlaylist.Title.Normalize()
                                             });

                if (googleUserPlaylist.Playlist != null)
                {
                    for (int index = 0; index < googleUserPlaylist.Playlist.Count; index++)
                    {
                        GoogleMusicSong googleSong = googleUserPlaylist.Playlist[index];

                        Song song;
                        if (!this.songEntities.TryGetValue(googleSong.Id, out song))
                        {
                            continue;
                        }

                        var entry = new UserPlaylistEntry()
                                        {
                                            ProviderEntryId = googleSong.PlaylistEntryId,
                                            PlaylistOrder = index,
                                            Song = song
                                        };

                        userPlaylist.Entries.Add(entry);

                        this.UpdateContainer(userPlaylist.Playlist, song);
                    }
                }

                this.playlists.Add(userPlaylist);
            }
        }

        public void AddSong(GoogleMusicSong googleMusicSong)
        {
            Genre genre = this.GetGenre(googleMusicSong.Genre);
            ArtistContainer songArtist = this.GetArtist(googleMusicSong.Artist);
            ArtistContainer albumArtist = string.IsNullOrEmpty(googleMusicSong.AlbumArtist)
                                              ? songArtist
                                              : this.GetArtist(googleMusicSong.AlbumArtist);

            Album album = albumArtist.GetAlbum(googleMusicSong.Album);
            if (googleMusicSong.Year.HasValue && googleMusicSong.Year.Value > 0 && !album.Year.HasValue)
            {
                album.Year = googleMusicSong.Year;
            }

            var song = googleMusicSong.ToSong();
            song.Genre = genre;
            song.Artist = songArtist.Artist;
            song.Album = album;

            this.UpdateContainer(genre, song);
            this.UpdateContainer(album, song);
            this.UpdateContainer(albumArtist.Artist, song);
            if (albumArtist != songArtist)
            {
                this.UpdateContainer(albumArtist.Artist, song);
            }

            this.songEntities.Add(song.ProviderSongId, song);
        }

        public async Task CommitAsync(SQLiteAsyncConnection connection)
        {
            await connection.RunInTransactionAsync(
                        (c) =>
                        {
                            c.InsertAll(this.genres.Select(x => x.Value));
                            c.InsertAll(this.artists.Select(x => x.Value.Artist));
                            c.InsertAll(this.artists.SelectMany(
                                x =>
                                {
                                    foreach (Album album in x.Value.Albums.Values)
                                    {
                                        album.ArtistId = x.Value.Artist.Id;
                                    }

                                    return x.Value.Albums.Values;
                                }));

                            c.InsertAll(this.songEntities.Select(s =>
                                {
                                    s.Value.AlbumId = s.Value.Album.Id;
                                    s.Value.GenreId = s.Value.Genre.Id;
                                    s.Value.ArtistId = s.Value.Artist.Id;
                                    return s.Value;
                                }));

                            c.InsertAll(this.playlists.Select(x => x.Playlist));
                            c.InsertAll(this.playlists.SelectMany(x =>
                                {
                                    foreach (var e in x.Entries)
                                    {
                                        e.PlaylistId = x.Playlist.Id;
                                        e.SongId = e.Song.SongId;
                                    }

                                    return x.Entries;
                                }));
                        });
        }

        private void UpdateContainer(IPlaylist entity, Song song)
        {
            if (entity.ArtUrl == null && song.AlbumArtUrl != null)
            {
                entity.ArtUrl = song.AlbumArtUrl;
            }

            if (entity.LastPlayed < song.LastPlayed)
            {
                entity.LastPlayed = song.LastPlayed;
            }

            entity.SongsCount++;
            entity.Duration += song.Duration;
        }

        private Genre GetGenre(string genreTitle)
        {
            string genreNormalized = genreTitle.Normalize();
            Genre genre;
            if (!this.genres.TryGetValue(genreNormalized, out genre))
            {
                genre = new Genre { Title = genreTitle, TitleNorm = genreNormalized };
                this.genres.Add(genreNormalized, genre);
            }

            return genre;
        }

        private ArtistContainer GetArtist(string artistTitle)
        {
            string artistNormalized = artistTitle.Normalize();
            ArtistContainer artist = null;
            if (!this.artists.TryGetValue(artistNormalized, out artist))
            {
                artist = new ArtistContainer(new Artist() { Title = artistTitle, TitleNorm = artistNormalized });
                this.artists.Add(artistNormalized, artist);
            }

            return artist;
        }

        private class ArtistContainer
        {
            public ArtistContainer(Artist artist)
            {
                this.Artist = artist;
                this.Albums = new Dictionary<string, Album>();
            }

            public Artist Artist { get; private set; }

            public Dictionary<string, Album> Albums { get; private set; }

            public Album GetAlbum(string albumTitle)
            {
                string albumNormalized = albumTitle.Normalize();
                Album album = null;
                if (!this.Albums.TryGetValue(albumNormalized, out album))
                {
                    album = new Album()
                                {
                                    Title = albumTitle, 
                                    TitleNorm = albumNormalized, 
                                    Artist = this.Artist
                                };
                    this.Albums.Add(albumNormalized, album);
                    this.Artist.AlbumsCount++;
                }

                return album;
            }
        }

        private class UserPlaylistContainer 
        {
            public UserPlaylistContainer(UserPlaylist userPlaylist)
            {
                this.Playlist = userPlaylist;
                this.Entries = new List<UserPlaylistEntry>();
            }

            public UserPlaylist Playlist { get; private set; }

            public List<UserPlaylistEntry> Entries { get; private set; }
        }
    }
}
