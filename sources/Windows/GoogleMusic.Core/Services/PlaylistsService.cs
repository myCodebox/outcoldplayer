// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.EventAggregator;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;

    public interface IPlaylistsService
    {
        IPlaylistRepository<TPlaylist> GetRepository<TPlaylist>() where TPlaylist : IPlaylist;

        Task<int> GetCountAsync(PlaylistType playlistType);

        Task<IList<Song>> GetSongsAsync(IPlaylist playlist);

        Task<IList<Song>> GetSongsAsync(PlaylistType playlistType, string id);

        Task<IPlaylist> GetAsync(PlaylistType playlistType, string id);

        Task<IEnumerable<IPlaylist>> GetAllAsync(PlaylistType playlistType, Order order, uint? take = null);

        Task<IEnumerable<IPlaylist>> SearchAsync(PlaylistType playlistType, string searchQuery, uint? take = null);

        Task GetArtUrisAsync(IMixedPlaylist playlist);
    }

    public class PlaylistsService : IPlaylistsService
    {
        private readonly IDependencyResolverContainer container;

        private readonly IRadioStationsService radioStationsService;

        private readonly IUserPlaylistsService userPlaylistsService;

        private readonly IApplicationResources applicationResources;

        private readonly ISettingsService settingsService;

        private readonly Dictionary<string, Task<Uri[]>> cachedUris = new Dictionary<string, Task<Uri[]>>();

        private readonly SemaphoreSlim limitSemaphore = new SemaphoreSlim(2);
        private readonly SemaphoreSlim dataSemaphore = new SemaphoreSlim(1);

        public PlaylistsService(
            IDependencyResolverContainer container,
            IRadioStationsService radioStationsService,
            IUserPlaylistsService userPlaylistsService,
            IApplicationResources applicationResources,
            ISettingsService settingsService,
            IEventAggregator eventAggregator)
        {
            this.container = container;
            this.radioStationsService = radioStationsService;
            this.userPlaylistsService = userPlaylistsService;
            this.applicationResources = applicationResources;
            this.settingsService = settingsService;

            eventAggregator.GetEvent<PlaylistsChangeEvent>()
                .Where(e => e.HasRemovedPlaylists() || e.HasUpdatedPlaylists())
                .Subscribe(
                    async (e) =>
                    {
                        var playlists = (e.UpdatedPlaylists ?? Enumerable.Empty<IPlaylist>()).Union(e.RemovedPlaylists)
                            .Where(x => x.PlaylistType == PlaylistType.UserPlaylist || x.PlaylistType == PlaylistType.Genre
                                    || x.PlaylistType == PlaylistType.Radio
                                    || x.PlaylistType == PlaylistType.SystemPlaylist);

                        await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                        try
                        {
                            foreach(var p in playlists)
                            {
                                this.cachedUris.Remove(p.Id);
                            }
                        }
                        finally
                        {
                            this.dataSemaphore.Release(1);
                        }
                    });
        }

        public IPlaylistRepository<TPlaylist> GetRepository<TPlaylist>() where TPlaylist : IPlaylist
        {
            return this.container.Resolve<IPlaylistRepository<TPlaylist>>();
        }

        public async Task<int> GetCountAsync(PlaylistType playlistType)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return await this.GetRepository<Album>().GetCountAsync();
                case PlaylistType.Artist:
                    return await this.GetRepository<Artist>().GetCountAsync();
                case PlaylistType.Genre:
                    return await this.GetRepository<Genre>().GetCountAsync();
                case PlaylistType.UserPlaylist:
                    return await this.GetRepository<UserPlaylist>().GetCountAsync();
                case PlaylistType.SystemPlaylist:
                    return await this.GetRepository<SystemPlaylist>().GetCountAsync();
                case PlaylistType.Radio:
                    return 1 + await this.GetRepository<Radio>().GetCountAsync();
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

        public async Task<IList<Song>> GetSongsAsync(PlaylistType playlistType, string id)
        {
            switch (playlistType)
            {
                case PlaylistType.Album:
                    return await this.GetRepository<Album>().GetSongsAsync(id);
                case PlaylistType.Artist:
                    return await this.GetRepository<Artist>().GetSongsAsync(id);
                case PlaylistType.Genre:
                    return await this.GetRepository<Genre>().GetSongsAsync(id);
                case PlaylistType.UserPlaylist:
                    var playlistRepository = this.GetRepository<UserPlaylist>();
                    var playlist = await playlistRepository.GetAsync(id);
                    if (playlist.IsShared)
                    {
                        return await this.userPlaylistsService.GetSharedPlaylistSongsAsync(playlist);
                    }
                    return await playlistRepository.GetSongsAsync(id);
                case PlaylistType.SystemPlaylist:
                    return await this.GetRepository<SystemPlaylist>().GetSongsAsync(id);
                case PlaylistType.Radio:
                    return await this.radioStationsService.GetRadioSongsAsync(id);
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
                case PlaylistType.Radio:
                    if (string.IsNullOrEmpty(id))
                    {
                        return this.GetLuckyRadio();
                    }
                    return await this.GetRepository<Radio>().GetAsync(id);
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
                    return (new [] { this.GetLuckyRadio() }).Union(await this.GetRepository<Radio>().GetAllAsync(order, take - 1));
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
                case PlaylistType.Radio:
                    return await this.GetRepository<Radio>().SearchAsync(searchQuery, take);
                default:
                    throw new ArgumentOutOfRangeException("playlistType");
            }
        }

        public async Task GetArtUrisAsync(IMixedPlaylist playlist)
        {
            await this.limitSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            try
            {
                Task<Uri[]> task;

                try
                {
                    await this.dataSemaphore.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                    if (!this.cachedUris.TryGetValue(playlist.Id, out task))
                    {
                        this.cachedUris.Add(playlist.Id, task = this.GetUrlsAsync(playlist));
                    }
                }
                finally
                {
                    this.dataSemaphore.Release(1);
                }

                playlist.ArtUrls = await task;
            }
            finally
            {
                this.limitSemaphore.Release(1);
            }
        }

        private async Task<Uri[]> GetUrlsAsync(IMixedPlaylist playlist)
        {
            switch (playlist.PlaylistType)
            {
                case PlaylistType.Genre:
                    return await ((IGenresRepository)this.GetRepository<Genre>()).GetUrisAsync(playlist.TitleNorm);
                case PlaylistType.UserPlaylist:
                    UserPlaylist userPlaylist = (UserPlaylist)playlist;
                    if (userPlaylist.IsShared)
                    {
                        Uri[] result = null;
                        var songs = await this.userPlaylistsService.GetSharedPlaylistSongsAsync(userPlaylist);
                        if (songs != null)
                        {
                            result = songs.OrderByDescending(x => x.Recent).Select(x => x.AlbumArtUrl).Distinct().Take(4).ToArray();
                        }

                        return result;
                    }
                    else
                    {
                        return await ((IUserPlaylistsRepository)this.GetRepository<UserPlaylist>()).GetUrisAsync(playlist.Id);
                    }
                case PlaylistType.SystemPlaylist:
                    return await ((ISystemPlaylistsRepository)this.GetRepository<SystemPlaylist>()).GetUrisAsync(((SystemPlaylist)playlist).SystemPlaylistType);
                default:
                    throw new ArgumentOutOfRangeException("playlist");
            }
        }

        private Radio GetLuckyRadio()
        {
            var luckyTitle = this.settingsService.GetIsAllAccessAvailable()
                ? this.applicationResources.GetString("Radio_Lucky")
                : this.applicationResources.GetString("InstantMix_Lucky");

            return new Radio()
            {
                SongId = string.Empty,
                Title = luckyTitle,
                TitleNorm = luckyTitle.Normalize()
            };
        }
    }
}
