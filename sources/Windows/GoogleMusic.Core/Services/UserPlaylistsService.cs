// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.GoogleMusic.Web.Models;
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IUserPlaylistsService
    {
        Task<UserPlaylist> CreateAsync(string name);

        Task<bool> DeleteAsync(IList<UserPlaylist> playlists);

        Task<bool> ChangeNameAsync(UserPlaylist playlist, string name);

        Task<bool> RemoveSongsAsync(UserPlaylist playlist, IEnumerable<Song> entry);

        Task<bool> AddSongsAsync(UserPlaylist playlist, IEnumerable<Song> song);

        Task<IList<Song>> GetSharedPlaylistSongsAsync(UserPlaylist userPlaylist);
    }

    public class UserPlaylistsService : IUserPlaylistsService
    {
        private readonly ILogger logger;
        private readonly IPlaylistsWebService webService;
        private readonly IUserPlaylistsRepository repository;
        private readonly ISongsRepository songsRepository;
        private readonly IEventAggregator eventAggregator;

        public UserPlaylistsService(
            ILogManager logManager,
            IPlaylistsWebService webService,
            IUserPlaylistsRepository repository,
            ISongsRepository songsRepository,
            IEventAggregator eventAggregator)
        {
            this.logger = logManager.CreateLogger("UserPlaylistsService");
            this.webService = webService;
            this.repository = repository;
            this.songsRepository = songsRepository;
            this.eventAggregator = eventAggregator;
        }

        public async Task<UserPlaylist> CreateAsync(string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Creating playlist '{0}'.", name);
            }

            var resp = await this.webService.CreateAsync(name);

            if (resp != null && resp.MutateResponse != null && resp.MutateResponse.Length == 1 && string.Equals(resp.MutateResponse[0].ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
            {
                var mutation = resp.MutateResponse[0];

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Playlist was created on the server with id '{0}' for name '{1}'.", mutation.Id, name);
                }

                var userPlaylist = new UserPlaylist { PlaylistId = mutation.Id, Title = name, TitleNorm = name.Normalize() };

                await this.repository.InsertAsync(new[] { userPlaylist });

                this.eventAggregator.Publish(
                    PlaylistsChangeEvent.New(PlaylistType.UserPlaylist).AddAddedPlaylists(userPlaylist));

                return userPlaylist;
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Could not create playlist for name '{0}'.", name);
                }

                return null;
            }
        }

        public async Task<bool> DeleteAsync(IList<UserPlaylist> playlists)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlists.");
            }

            List<UserPlaylist> toDelete = new List<UserPlaylist>();

            var resp = await this.webService.DeleteAsync(playlists);
            foreach (var mutation in resp.MutateResponse)
            {
                if (string.Equals(mutation.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    var playlist =
                        playlists.FirstOrDefault(
                            x => string.Equals(x.Id, mutation.Id, StringComparison.OrdinalIgnoreCase));
                    if (playlist != null)
                    {
                        toDelete.Add(playlist);
                    }
                }
                else
                {
                    this.logger.Debug(
                        "Playlist '{0}' was not deleted from server with response '{1}'.",
                        mutation.Id,
                        mutation.ResponseCode);
                }
            }

            if (toDelete.Count > 0)
            {
                await this.repository.DeleteAsync(toDelete);
                this.eventAggregator.Publish(
                    PlaylistsChangeEvent.New(PlaylistType.UserPlaylist).AddRemovedPlaylists(toDelete.Cast<IPlaylist>().ToArray()));
            }

            return toDelete.Count > 0;
        }

        public async Task<bool> ChangeNameAsync(UserPlaylist playlist, string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Changing name for playlist with Id '{0}' to '{1}'.", playlist.PlaylistId, name);
            }

            var resp = await this.webService.ChangeNameAsync(playlist.PlaylistId, name);

            if (resp != null && resp.MutateResponse != null && resp.MutateResponse.Length == 1 && string.Equals(resp.MutateResponse[0].ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
            {
                var mutation = resp.MutateResponse[0];

                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("The result of name changing for playlist with id '{0}' is '{1}'", mutation.Id, name);
                }

                playlist.Title = name;
                playlist.TitleNorm = name.Normalize();

                await this.repository.UpdateAsync(new[] { playlist });
                this.eventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.UserPlaylist).AddUpdatedPlaylists(playlist));

                return true;
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug("Could not change name for playlist '{0}'.", playlist.Id);
                }

                return false;
            }
        }

        public async Task<bool> RemoveSongsAsync(UserPlaylist playlist, IEnumerable<Song> songs)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            List<UserPlaylistEntry> list = songs.Select(x => x.UserPlaylistEntry).ToList();
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entries from playlist '{0}'.", playlist.PlaylistId);
            }

            var resp = await this.webService.RemoveSongsAsync(playlist, list);

            List<UserPlaylistEntry> toDelete = new List<UserPlaylistEntry>();

            foreach (var mutation in resp.MutateResponse)
            {
                if (string.Equals(mutation.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    var entry =
                        list.FirstOrDefault(x => string.Equals(x.Id, mutation.Id, StringComparison.OrdinalIgnoreCase));
                    if (entry != null)
                    {
                        toDelete.Add(entry);
                    }
                }
                else
                {
                    this.logger.Debug(
                        "Entry '{0}' was NOT deleted from server with response '{1}'.",
                        mutation.Id,
                        mutation.ResponseCode);
                }
            }

            if (toDelete.Count > 0)
            {
                await this.repository.DeleteEntriesAsync(toDelete);
            }

            return toDelete.Count > 0;
        }

        public async Task<bool> AddSongsAsync(UserPlaylist playlist, IEnumerable<Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            var dictionary = songs.ToDictionary(x => Guid.NewGuid().ToString().ToLowerInvariant(), x => x);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Adding {0} songs to playlist '{1}'.", dictionary.Count, playlist.PlaylistId);
            }

            List<UserPlaylistEntry> toInsert = new List<UserPlaylistEntry>();

            var result = await this.webService.AddSongsAsync(playlist, dictionary);
            if (result != null)
            {
                for (int index = 0; index < result.MutateResponse.Length; index++)
                {
                    var mutation = result.MutateResponse[index];
                    if (string.Equals(mutation.ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
                    {
                        var song = dictionary[mutation.ClientId];
                        toInsert.Add(
                            new UserPlaylistEntry()
                            {
                                Id = mutation.Id,
                                CliendId = mutation.ClientId,
                                SongId = song.SongId,
                                CreationDate = DateTime.UtcNow,
                                LastModified = DateTime.UtcNow,
                                Source = song.TrackType == StreamType.EphemeralSubscription ? 2 : 1,
                                PlaylistOrder = ((1729000000000000000L) + DateTime.UtcNow.Millisecond * 1000L + index).ToString("G"),
                                PlaylistId = playlist.PlaylistId
                            });
                    }
                    else
                    {
                        this.logger.Debug(
                            "Could not add song to playlist {1} because {2}.",
                            playlist.Id,
                            mutation.ResponseCode);
                    }
                }
            }

            if (toInsert.Count > 0)
            {
                await this.repository.InsertEntriesAsync(toInsert);
            }

            return toInsert.Count > 0;
        }

        public async Task<IList<Song>> GetSharedPlaylistSongsAsync(UserPlaylist userPlaylist)
        {
            List<Song> songs = new List<Song>();

            var resp = await this.webService.GetAllPlaylistEntriesSharedAsync(new UserPlaylist[] { userPlaylist });
            if (resp.Entries.Length == 1
                && string.Equals(resp.Entries[0].ResponseCode, "OK", StringComparison.OrdinalIgnoreCase))
            {
                GoogleMusicSharedPlaylist googleMusicSharedPlaylist = resp.Entries[0];
                if (string.Equals(
                    googleMusicSharedPlaylist.ShareToken,
                    userPlaylist.ShareToken,
                    StringComparison.OrdinalIgnoreCase) && googleMusicSharedPlaylist.PlaylistEntry != null)
                {
                    foreach (var entry in googleMusicSharedPlaylist.PlaylistEntry)
                    {
                        var song = await this.songsRepository.FindSongAsync(entry.TrackId);

                        if (song == null)
                        {
                            song = entry.Track.ToSong();
                            song.IsLibrary = false;
                            song.UnknownSong = true;
                        }

                        songs.Add(song);
                    }
                }
            }

            if (songs.Count > 0)
            {
                userPlaylist.ArtUrl = songs[0].AlbumArtUrl;
                await this.repository.UpdateAsync(new[] { userPlaylist });
            }

            return songs;
        }
    }
}
