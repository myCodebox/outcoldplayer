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
    using OutcoldSolutions.GoogleMusic.Web.Synchronization;

    public interface IUserPlaylistsService
    {
        Task<UserPlaylist> CreateAsync(string name);

        Task<bool> DeleteAsync(UserPlaylist playlist);

        Task<bool> ChangeNameAsync(UserPlaylist playlist, string name);

        Task<bool> RemoveSongsAsync(UserPlaylist playlist, IEnumerable<Song> entry);

        Task<bool> AddSongsAsync(UserPlaylist playlist, IEnumerable<Song> song);
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
            if (resp.Success.HasValue && !resp.Success.Value)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Could not create playlist for name '{0}'.", name);
                }

                return null;
            }
            else
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Playlist was created on the server with id '{0}' for name '{1}'.", resp.Id, name);
                }

                var userPlaylist = new UserPlaylist
                {
                    PlaylistId = resp.Id,
                    Title = name,
                    TitleNorm = name.Normalize()
                };

                await this.repository.InsertAsync(new [] { userPlaylist });

                this.eventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.UserPlaylist).AddAddedPlaylists(userPlaylist));

                return userPlaylist;
            }
        }

        public async Task<bool> DeleteAsync(UserPlaylist playlist)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Deleting playlist '{0}'.", playlist.PlaylistId);
            }

            var resp = await this.webService.DeleteAsync(playlist.PlaylistId);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug(
                    "Playlist '{0}' was deleted from server with response '{1}'.",
                    playlist.PlaylistId,
                    resp);
            }

            if (resp)
            {
                await this.repository.DeleteAsync(new [] { playlist });
                this.eventAggregator.Publish(
                    PlaylistsChangeEvent.New(PlaylistType.UserPlaylist).AddRemovedPlaylists(playlist));
            }

            return resp;
        }

        public async Task<bool> ChangeNameAsync(UserPlaylist playlist, string name)
        {
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Changing name for playlist with Id '{0}' to '{1}'.", playlist.PlaylistId, name);
            }

            bool result = await this.webService.ChangeNameAsync(playlist.PlaylistId, name);

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("The result of name changing for playlist with id '{0}' is '{1}'.", playlist.PlaylistId, result);
            }

            if (result)
            {
                playlist.Title = name;
                playlist.TitleNorm = name.Normalize();
                await this.repository.UpdateAsync(new [] { playlist });
                this.eventAggregator.Publish(PlaylistsChangeEvent.New(PlaylistType.UserPlaylist).AddUpdatedPlaylists(playlist));
            }

            return result;
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

            List<Song> list = songs.ToList();
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Removing entries from playlist '{0}'.", playlist.PlaylistId);
            }

            string[] songIds = new string[list.Count];
            string[] entryIds = new string[list.Count];

            for (int index = 0; index < list.Count; index++)
            {
                var song = list[index];
                songIds[index] = song.SongId;

                if (song.UserPlaylistEntry == null)
                {
                    throw new ArgumentException("Songs should be collection of songs with playlist entries.", "songs");
                }

                entryIds[index] = song.UserPlaylistEntry.Id;
            }

            var result = await this.webService.RemoveSongsAsync(playlist.PlaylistId, songIds, entryIds);
            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Result of entry removing entries from playlist '{0}' is '{1}'.", playlist.PlaylistId, result);
            }

            if (result)
            {
                await this.repository.DeleteEntriesAsync(list.Select(s => s.UserPlaylistEntry));
            }

            return result;
        }

        public async Task<bool> AddSongsAsync(UserPlaylist playlist, IEnumerable<Song> songs)
        {
            if (songs == null)
            {
                throw new ArgumentNullException("songs");
            }

            List<Song> list = songs.ToList();

            if (this.logger.IsDebugEnabled)
            {
                this.logger.Debug("Adding {0} songs to playlist '{1}'.", list.Count, playlist.PlaylistId);
            }

            var result = await this.webService.AddSongsAsync(playlist.PlaylistId, list.Select(s => s.SongId).ToArray());
            if (result != null)
            {
                if (this.logger.IsDebugEnabled)
                {
                    this.logger.Debug(
                        "Successfully added '{0}' songs to playlist {1}.",
                        list.Count,
                        playlist.PlaylistId);
                }


                // TODO: Temporary solution
                await ApplicationBase.Container.Resolve<IGoogleMusicSynchronizationService>().Update();

                /*
                IList<UserPlaylistEntry> entries = new List<UserPlaylistEntry>();

                int index = 0;

                foreach (var songIdResp in result.SongIds)
                {
                    var storedSong = list.FirstOrDefault(x => string.Equals(x.SongId, songIdResp.SongId, StringComparison.OrdinalIgnoreCase));

                    // Don't support Playlist to Radio
                    if (storedSong.SongId <= 0)
                    {
                        await this.songsRepository.InsertAsync(new[] { storedSong });
                    }

                    entries.Add(new UserPlaylistEntry()
                    {
                        SongId = storedSong.SongId,
                        ProviderEntryId = songIdResp.PlaylistEntryId,
                        PlaylistOrder = playlist.SongsCount + index,
                        PlaylistId = playlist.PlaylistId
                    });

                    index++;
                }

                await this.repository.InsertEntriesAsync(entries);
                */

                return true;
            }

            return false;
        }
    }
}
