// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------

namespace OutcoldSolutions.GoogleMusic.Services.Actions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Presenters;
    using OutcoldSolutions.GoogleMusic.Views;

    public class RemoveSelectedSongAction: ISelectedObjectAction
    {
        private readonly INavigationService navigationService;
        private readonly IPlayQueueService playQueueService;

        public RemoveSelectedSongAction(
            INavigationService navigationService,
            IPlayQueueService playQueueService)
        {
            this.navigationService = navigationService;
            this.playQueueService = playQueueService;
        }

        public string Icon
        {
            get
            {
                return CommandIcon.Remove;
            }
        }

        public string Title
        {
            get
            {
                return "Remove from queue";
            }
        }

        public ActionGroup Group
        {
            get
            {
                return ActionGroup.Navigation;
            }
        }


        public int Priority
        {
            get
            {
                return 50;
            }
        }

        public bool CanExecute(IList<object> selectedObjects)
        {
            IPageView currentView = this.navigationService.GetCurrentView();

            if (currentView is ICurrentPlaylistPageView)
            {
                return true;
            }

            if (currentView is IPlaylistPageView)
            {
                var presenter = currentView.GetPresenter<BindingModelBase>();
                var playlistPageViewPresenter = presenter as PlaylistPageViewPresenter;
                if (playlistPageViewPresenter == null)
                {
                    return false;
                }

                PlaylistPageViewBindingModel bindingModel = playlistPageViewPresenter.BindingModel;
                IPlaylist currentPlaylist = this.playQueueService.CurrentPlaylist;
                if (currentPlaylist != null && 
                    currentPlaylist.PlaylistType != PlaylistType.Radio && 
                    string.Equals(bindingModel.Playlist.Id, currentPlaylist.Id, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool?> Execute(IList<object> selectedObjects)
        {
            if (!this.CanExecute(selectedObjects))
            {
                return null;
            }

            ICurrentPlaylistPageView currentView = this.navigationService.GetCurrentView() as ICurrentPlaylistPageView;

            SongsListViewPresenter songsListViewPresenter = null;
            if (currentView != null)
            {
                songsListViewPresenter = currentView.GetSongsListView().GetPresenter<SongsListViewPresenter>();
            }

            IPlaylistPageView playlistPageView = this.navigationService.GetCurrentView() as IPlaylistPageView;
            if (playlistPageView != null)
            {
                songsListViewPresenter = playlistPageView.GetSongsListView().GetPresenter<SongsListViewPresenter>();
            }

            if (songsListViewPresenter != null)
            {
                await this.playQueueService.RemoveAsync(songsListViewPresenter.GetSelectedIndexes().ToList());
            }

            if (playlistPageView != null)
            {
                this.navigationService.NavigateTo<ICurrentPlaylistPageView>(keepInHistory: false);
            }

            return null;
        }
    }
}
