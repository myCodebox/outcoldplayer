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
    using OutcoldSolutions.GoogleMusic.Repositories.DbModels;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.Models;

    using SQLite;

    internal class SongsSyncContainer
    {
        private readonly Dictionary<string, Tuple<ArtistEntity, Dictionary<string, AlbumEntity>>> artists = 
            new Dictionary<string, Tuple<ArtistEntity, Dictionary<string, AlbumEntity>>>();

        private readonly Dictionary<string, GenreEntity> genres = 
            new Dictionary<string, GenreEntity>();

        private readonly Dictionary<string, SongEntity> songEntities =
            new Dictionary<string, SongEntity>();

        private readonly List<Tuple<UserPlaylistEntity, List<Tuple<UserPlaylistEntryEntity, SongEntity>>>> playlistEntities = 
            new List<Tuple<UserPlaylistEntity, List<Tuple<UserPlaylistEntryEntity, SongEntity>>>>();

        public void AddRange(IList<GoogleMusicSong> googleMusicSongs)
        {
            foreach (var googleSong in googleMusicSongs)
            {
                this.AddSong(googleSong);
            }
        }

        public void AddPlaylists(IList<GoogleMusicPlaylist> playlists)
        {
            for (int i = 0; i < playlists.Count; i++)
            {
                var googleUserPlaylist = playlists[i];

                var userPlaylistEntity = new UserPlaylistEntity()
                                             {
                                                 ProviderPlaylistId = googleUserPlaylist.PlaylistId,
                                                 Title = googleUserPlaylist.Title,
                                                 TitleNorm = googleUserPlaylist.Title.Normalize()
                                             };

                var entries = new List<Tuple<UserPlaylistEntryEntity, SongEntity>>();

                for (int index = 0; index < googleUserPlaylist.Playlist.Count; index++)
                {
                    GoogleMusicSong googleSong = googleUserPlaylist.Playlist[index];
                    var song = this.songEntities[googleSong.Id];
                    var entry = new UserPlaylistEntryEntity()
                                    {
                                        ProviderEntryId = googleSong.PlaylistEntryId,
                                        SongId = song.SongId,
                                        PlaylistOrder = index
                                    };

                    entries.Add(Tuple.Create(entry, song));

                    this.UpdateContainer(userPlaylistEntity, song);
                }

                this.playlistEntities.Add(Tuple.Create(userPlaylistEntity, entries));
            }
        }

        public void AddSong(GoogleMusicSong googleMusicSong)
        {
            string genreNormalized = googleMusicSong.Genre.Normalize();
            GenreEntity genre;
            if (!this.genres.TryGetValue(genreNormalized, out genre))
            {
                this.genres.Add(genreNormalized, genre = new GenreEntity { Title = googleMusicSong.Genre, TitleNorm = genreNormalized });
            }

            string artistNormalized = googleMusicSong.Artist.Normalize();
            Tuple<ArtistEntity, Dictionary<string, AlbumEntity>> artistT;
            if (!this.artists.TryGetValue(artistNormalized, out artistT))
            {
                this.artists.Add(artistNormalized, artistT = Tuple.Create(new ArtistEntity() { Title = googleMusicSong.Artist, TitleNorm = artistNormalized }, new Dictionary<string, AlbumEntity>()));
            }

            string albumArtistNormalized = googleMusicSong.AlbumArtist.Normalize();
            Tuple<ArtistEntity, Dictionary<string, AlbumEntity>> albumArtistT;
            if (!this.artists.TryGetValue(albumArtistNormalized, out albumArtistT))
            {
                this.artists.Add(albumArtistNormalized, albumArtistT = Tuple.Create(new ArtistEntity() { Title = googleMusicSong.AlbumArtist, TitleNorm = albumArtistNormalized }, new Dictionary<string, AlbumEntity>()));
            }

            string albumNormalized = googleMusicSong.Album.Normalize();
            AlbumEntity album;
            if (!albumArtistT.Item2.TryGetValue(albumNormalized, out album))
            {
                album = new AlbumEntity
                            {
                                Title = googleMusicSong.Album,
                                TitleNorm = albumNormalized,
                                Year = googleMusicSong.Year,
                                Artist = albumArtistT.Item1
                            };

                albumArtistT.Item2.Add(albumNormalized, album);
            }

            var song = new SongEntity()
            {
                AlbumArtist = googleMusicSong.AlbumArtist,
                AlbumArtUrl = string.IsNullOrEmpty(googleMusicSong.AlbumArtUrl) ? null : new Uri("http:" + googleMusicSong.AlbumArtUrl),
                Composer = googleMusicSong.Composer,
                Disc = googleMusicSong.Disc,
                TotalDiscs = googleMusicSong.TotalDiscs,
                Duration = TimeSpan.FromMilliseconds(googleMusicSong.DurationMillis),
                ProviderSongId = googleMusicSong.Id,
                LastPlayed = DateTimeExtensions.FromUnixFileTime(googleMusicSong.LastPlayed / 1000),
                CreationDate = DateTimeExtensions.FromUnixFileTime(googleMusicSong.CreationDate / 1000),
                PlayCount = googleMusicSong.PlayCount,
                Rating = googleMusicSong.Rating,
                Title = googleMusicSong.Title,
                TitleNorm = googleMusicSong.Title.Normalize(),
                Track = googleMusicSong.Track,
                TotalTracks = googleMusicSong.TotalTracks,
                Year = googleMusicSong.Year,
                Comment = googleMusicSong.Comment,
                Bitrate = googleMusicSong.Bitrate,
                StreamType = StreamType.GoogleMusic,
                GenreTitle = googleMusicSong.Genre,
                ArtistTitle = googleMusicSong.Artist,
                AlbumTitle = googleMusicSong.Album,
                Genre = genre,
                Artist = artistT.Item1,
                Album = album
            };

            this.UpdateContainer(genre, song);
            this.UpdateContainer(album, song);
            this.UpdateContainer(artistT.Item1, song);

            this.songEntities.Add(song.ProviderSongId, song);
        }

        public async Task SaveAsync(SQLiteAsyncConnection connection)
        {
            await connection.RunInTransactionAsync(
                        (c) =>
                        {
                            c.InsertAll(genres.Select(x => x.Value));
                            c.InsertAll(artists.Select(x =>
                                {
                                    x.Value.Item1.AlbumsCount = x.Value.Item2.Count;
                                    return x.Value.Item1;
                                }));
                            c.InsertAll(artists.SelectMany(
                                x =>
                                {
                                    var albums = x.Value.Item2.Select(a => a.Value).ToList();
                                    foreach (AlbumEntity album in albums)
                                    {
                                        album.ArtistId = x.Value.Item1.ArtistId;
                                    }

                                    return albums;
                                }));

                            c.InsertAll(songEntities.Select(s =>
                                {
                                    s.Value.AlbumId = s.Value.Album.AlbumId;
                                    s.Value.GenreId = s.Value.Genre.GenreId;
                                    s.Value.ArtistId = s.Value.Artist.ArtistId;
                                    return s.Value;
                                }));

                            c.InsertAll(this.playlistEntities.Select(x => x.Item1));
                            c.InsertAll(this.playlistEntities.SelectMany(x =>
                                {
                                    var entities = new List<UserPlaylistEntryEntity>();

                                    foreach (var e in x.Item2)
                                    {
                                        e.Item1.PlaylistId = x.Item1.PlaylistId;
                                        e.Item1.SongId = e.Item2.SongId;
                                        entities.Add(e.Item1);
                                    }

                                    return entities;
                                }));
                        });
        }

        private void UpdateContainer(ISongsContainerEntity entity, SongEntity song)
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
    }
}
