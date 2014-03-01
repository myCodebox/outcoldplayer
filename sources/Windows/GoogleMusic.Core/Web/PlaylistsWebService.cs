// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Web
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Newtonsoft.Json;

    using OutcoldSolutions.GoogleMusic.Web.Models;

    public interface IPlaylistsWebService
    {
        Task<IList<GoogleMusicPlaylist>> GetAllAsync(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylist>, Task> chunkHandler = null);

        Task<IList<GoogleMusicPlaylistEntry>> GetAllPlaylistEntries(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylistEntry>, Task> chunkHandler = null);

        Task<AddPlaylistResp> CreateAsync(string name);

        Task<bool> DeleteAsync(string id);

        Task<bool> ChangeNameAsync(string id, string name);

        Task<AddSongResp> AddSongsAsync(string playlistId, string[] songIds);

        Task<bool> RemoveSongsAsync(string playlistId, string[] songId, string[] entryId);
    }

    public class PlaylistsWebService : IPlaylistsWebService
    {
        private const string PlaylistFeed = "playlistfeed";
        private const string PlEntryFeed = "plentryfeed";
        private const string AddPlaylistUrl = "services/createplaylist?format=json";
        private const string DeletePlaylistUrl = "services/deleteplaylist";
        private const string ChangePlaylistNameUrl = "services/modifyplaylist";
        private const string AddToPlaylistUrl = "services/addtoplaylist";
        private const string DeleteSongUrl = "services/deletesong";

        private readonly IGoogleMusicWebService googleMusicWebService;
        private readonly IGoogleMusicApisService googleMusicApisService;

        public PlaylistsWebService(
            IGoogleMusicWebService googleMusicWebService,
            IGoogleMusicApisService googleMusicApisService)
        {
            this.googleMusicWebService = googleMusicWebService;
            this.googleMusicApisService = googleMusicApisService;
        }

        public Task<IList<GoogleMusicPlaylist>> GetAllAsync(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylist>, Task> chunkHandler = null)
        {
            return this.googleMusicApisService.DownloadList(PlaylistFeed, lastUpdate, progress, chunkHandler);
        }

        public Task<IList<GoogleMusicPlaylistEntry>> GetAllPlaylistEntries(DateTime? lastUpdate, IProgress<int> progress = null, Func<IList<GoogleMusicPlaylistEntry>, Task> chunkHandler = null)
        {
            return this.googleMusicApisService.DownloadList(PlEntryFeed, lastUpdate, progress, chunkHandler);
        }

        public async Task<AddPlaylistResp> CreateAsync(string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "name", JsonConvert.ToString(name) },
                                            { "type", JsonConvert.ToString("USER_GENERATED") },
                                            { "track", JsonConvert.SerializeObject(new string[] { }) },
                                            { "public", JsonConvert.ToString(false) },
                                        };

            return await this.googleMusicWebService.PostAsync<AddPlaylistResp>(AddPlaylistUrl, jsonProperties: jsonProperties);
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "id", JsonConvert.ToString(id) },
                                            { "requestType", JsonConvert.ToString(1) },
                                            { "requestCause", JsonConvert.ToString(1) }
                                        };

            var deletePlaylistResp = await this.googleMusicWebService.PostAsync<DeletePlaylistResp>(DeletePlaylistUrl, jsonProperties: jsonProperties);

            return deletePlaylistResp.DeleteId == id;
        }

        public async Task<bool> ChangeNameAsync(string id, string name)
        {
            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "playlistId", JsonConvert.ToString(id) },
                                            { "playlistName", JsonConvert.ToString(name) }
                                        };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(ChangePlaylistNameUrl, jsonProperties: jsonProperties);
            return !response.Success.HasValue || response.Success.Value;
        }

        public async Task<AddSongResp> AddSongsAsync(string playlistId, string[] songIds)
        {
            if (songIds == null)
            {
                throw new ArgumentNullException("songIds");
            }

            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "playlistId", JsonConvert.ToString(playlistId) },
                                            { "songRefs", JsonConvert.SerializeObject(songIds.Select(x => new { id = x, type = 1 }).ToArray()) }
                                        };

            return await this.googleMusicWebService.PostAsync<AddSongResp>(AddToPlaylistUrl, jsonProperties: jsonProperties);
        }

        public async Task<bool> RemoveSongsAsync(string playlistId, string[] songIds, string[] entryIds)
        {
            if (songIds == null)
            {
                throw new ArgumentNullException("songIds");
            }

            if (entryIds == null)
            {
                throw new ArgumentNullException("entryIds");
            }

            if (songIds.Length != entryIds.Length)
            {
                throw new ArgumentException("Different lengths of collections: songIds and entries Ids.", "entryIds");
            }

            var jsonProperties = new Dictionary<string, string>
                                        {
                                            { "listId", JsonConvert.ToString(playlistId) },
                                            { "songIds", JsonConvert.SerializeObject(songIds) },
                                            { "entryIds", JsonConvert.SerializeObject(entryIds) }
                                        };

            var response = await this.googleMusicWebService.PostAsync<CommonResponse>(DeleteSongUrl, jsonProperties: jsonProperties);
            return !response.Success.HasValue || response.Success.Value;
        }
    }
}