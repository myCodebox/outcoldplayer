// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Web;

    public interface IPlaylistsService
    {
        IPlaylistRepository<TPlaylist> GetRepository<TPlaylist>() where TPlaylist : IPlaylist;

        Task<int> GetCountAsync(PlaylistType playlistType);

        Task<IList<Song>> GetSongsAsync(IPlaylist playlist);

        Task<IList<Song>> GetSongsAsync(PlaylistType playlistType, string id);

        Task<IPlaylist> GetAsync(PlaylistType playlistType, string id);

        Task<IEnumerable<IPlaylist>> GetAllAsync(PlaylistType playlistType, Order order, uint? take = null);

        Task<IEnumerable<IPlaylist>> SearchAsync(PlaylistType playlistType, string searchQuery, uint? take = null);
    }

    public class PlaylistsService : IPlaylistsService
    {
        private readonly IDependencyResolverContainer container;

        private readonly IRadioWebService radioWebService;

        public PlaylistsService(
            IDependencyResolverContainer container,
            IRadioWebService radioWebService)
        {
            this.container = container;
            this.radioWebService = radioWebService;
        }

        public IPlaylistRepository<TPlaylist> GetRepository<TPlaylist>() where TPlaylist : IPlaylist
        {
            return this.container.Resolve<IPlaylistRepository<TPlaylist>>();
        }

        public Task<int> GetCountAsync(PlaylistType playlistType)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return this.GetRepository<Album>().GetCountAsync();
                case PlaylistType.Artist:
                    return this.GetRepository<Artist>().GetCountAsync();
                case PlaylistType.Genre:
                    return this.GetRepository<Genre>().GetCountAsync();
                case PlaylistType.UserPlaylist:
                    return this.GetRepository<UserPlaylist>().GetCountAsync();
                case PlaylistType.SystemPlaylist:
                    return this.GetRepository<SystemPlaylist>().GetCountAsync();
                default:
                    throw new ArgumentOutOfRangeException("playlistType");
            }
        }

        public Task<IList<Song>> GetSongsAsync(IPlaylist playlist)
        {
            if (playlist == null)
            {
                throw new ArgumentNullException("playlist");
            }

            return this.GetSongsAsync(playlist.PlaylistType, playlist.Id);
        }

        public Task<IList<Song>> GetSongsAsync(PlaylistType playlistType, string id)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return this.GetRepository<Album>().GetSongsAsync(id);
                case PlaylistType.Artist:
                    return this.GetRepository<Artist>().GetSongsAsync(id);
                case PlaylistType.Genre:
                    return this.GetRepository<Genre>().GetSongsAsync(id);
                case PlaylistType.UserPlaylist:
                    return this.GetRepository<UserPlaylist>().GetSongsAsync(id);
                case PlaylistType.SystemPlaylist:
                    return this.GetRepository<SystemPlaylist>().GetSongsAsync(id);
                case PlaylistType.Radio:
                    return this.radioWebService.GetRadioSongsAsync(id);
                default:
                    throw new ArgumentOutOfRangeException("playlistType");
            }
        }

        public async Task<IPlaylist> GetAsync(PlaylistType playlistType, string id)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return await this.GetRepository<Album>().GetAsync(id);
                case PlaylistType.Artist:
                    return await this.GetRepository<Artist>().GetAsync(id);
                case PlaylistType.Genre:
                    return await this.GetRepository<Genre>().GetAsync(id);
                case PlaylistType.UserPlaylist:
                    return await this.GetRepository<UserPlaylist>().GetAsync(id);
                case PlaylistType.SystemPlaylist:
                    return await this.GetRepository<SystemPlaylist>().GetAsync(id);
                default:
                    throw new ArgumentOutOfRangeException("playlistType");
            }
        }

        public async Task<IEnumerable<IPlaylist>> GetAllAsync(PlaylistType playlistType, Order order, uint? take = null)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return await this.GetRepository<Album>().GetAllAsync(order, take);
                case PlaylistType.Artist:
                    return await this.GetRepository<Artist>().GetAllAsync(order, take);
                case PlaylistType.Genre:
                    return await this.GetRepository<Genre>().GetAllAsync(order, take);
                case PlaylistType.UserPlaylist:
                    return await this.GetRepository<UserPlaylist>().GetAllAsync(order, take);
                case PlaylistType.SystemPlaylist:
                    return await this.GetRepository<SystemPlaylist>().GetAllAsync(order, take);
                case PlaylistType.Radio:
                    return await this.radioWebService.GetAllAsync();
                default:
                    throw new ArgumentOutOfRangeException("playlistType");
            }
        }

        public async Task<IEnumerable<IPlaylist>> SearchAsync(PlaylistType playlistType, string searchQuery, uint? take = null)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return await this.GetRepository<Album>().SearchAsync(searchQuery, take);
                case PlaylistType.Artist:
                    return await this.GetRepository<Artist>().SearchAsync(searchQuery, take);
                case PlaylistType.Genre:
                    return await this.GetRepository<Genre>().SearchAsync(searchQuery, take);
                case PlaylistType.UserPlaylist:
                    return await this.GetRepository<UserPlaylist>().SearchAsync(searchQuery, take);
                case PlaylistType.SystemPlaylist:
                    return await this.GetRepository<SystemPlaylist>().SearchAsync(searchQuery, take);
                default:
                    throw new ArgumentOutOfRangeException("playlistType");
            }
        }
    }
}
