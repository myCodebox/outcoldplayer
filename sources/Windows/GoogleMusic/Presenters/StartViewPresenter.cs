﻿// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class StartViewPresenter : PlaylistsViewPresenterBase<IStartView>
    {
        private const int MaxItems = 12;

        private readonly IPlaylistCollectionsService collectionsService;

        public StartViewPresenter(
            IDependencyResolverContainer container, 
            IStartView view,
            IPlaylistCollectionsService collectionsService)
            : base(container, view)
        {
            this.collectionsService = collectionsService;

            this.BindingModel = new StartViewBindingModel();
        }

        public StartViewBindingModel BindingModel { get; private set; }

        public override void OnNavigatedTo(NavigatedToEventArgs eventArgs)
        {
            base.OnNavigatedTo(eventArgs);

            this.View.SetGroups(null);
            this.BindingModel.IsLoading = true;

            this.Logger.Debug("Loading playlists.");
            this.GetGroupsAsync().ContinueWith(
                task =>
                    {
                        if (task.IsCompleted && !task.IsFaulted)
                        {
                            this.View.SetGroups(task.Result);
                        }

                        this.BindingModel.IsLoading = false;
                    },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        public async Task<List<PlaylistsGroupBindingModel>> GetGroupsAsync()
        {
            var groups = new List<PlaylistsGroupBindingModel>();

            // TODO: PlaylistsRequest should be null
            groups.Add(new PlaylistsGroupBindingModel(
                null,
                await this.collectionsService.GetCollection<SystemPlaylist>().CountAsync(),
                (await this.collectionsService.GetCollection<SystemPlaylist>().GetAllAsync(Order.None)).Select(x => new PlaylistBindingModel(x)),
                PlaylistsRequest.Albums));
            groups.Add(await this.GetGroupAsync<MusicPlaylist>("Playlists", PlaylistsRequest.Playlists));
            groups.Add(await this.GetGroupAsync<Artist>("Artists", PlaylistsRequest.Artists));
            groups.Add(await this.GetGroupAsync<Album>("Albums", PlaylistsRequest.Albums));
            groups.Add(await this.GetGroupAsync<Genre>("Genres", PlaylistsRequest.Genres));

            return groups;
        }

        private async Task<PlaylistsGroupBindingModel> GetGroupAsync<TPlaylist>(string title, PlaylistsRequest playlistsRequest) where TPlaylist : Playlist
        {
            var collection = this.collectionsService.GetCollection<TPlaylist>();
            var playlists = (await collection.GetAllAsync(Order.LastPlayed, MaxItems)).ToList();
            return new PlaylistsGroupBindingModel(
                title,
                await collection.CountAsync(),
                playlists.Select(x => new PlaylistBindingModel(x)),
                playlistsRequest);
        }
    }
}