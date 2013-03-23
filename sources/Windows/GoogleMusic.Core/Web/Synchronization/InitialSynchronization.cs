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
            var song = googleMusicSong.ToSong();

            this.songEntities.Add(song.ProviderSongId, song);
        }

        public async Task CommitAsync(SQLiteAsyncConnection connection)
        {
            await connection.RunInTransactionAsync(
                        (c) =>
                        {
                            c.InsertAll(this.songEntities.Select(s =>
                                {
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
