// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class PlaylistPageViewPresenterBase<TView, TPlaylist> 
        : PagePresenterBase<TView, PlaylistPageViewBindingModel<TPlaylist>>
        where TPlaylist : class, IPlaylist 
        where TView : IPageView
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsService metadataEditService;
        private readonly IPlaylistsService playlistsService;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
        {
            this.playQueueService = container.Resolve<IPlayQueueService>();
            this.metadataEditService = container.Resolve<ISongsService>();
            this.playlistsService = container.Resolve<IPlaylistsService>();

            this.PlaySongCommand = new DelegateCommand(this.Play, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count > 0);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count > 0);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
        }

        public DelegateCommand PlaySongCommand { get; set; }

        public DelegateCommand AddToPlaylistCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.SongsBindingModel.SetCollection(null);
            this.BindingModel.Playlist = default(TPlaylist);
        }

        public void PlaySong(SongBindingModel songBindingModel)
        {
            if (songBindingModel != null)
            {
                int songIndex = this.BindingModel.SongsBindingModel.Songs.IndexOf(songBindingModel);
                this.Logger.LogTask(this.playQueueService.PlayAsync(this.BindingModel.Playlist, this.BindingModel.SongsBindingModel.Songs.Select(s => s.Metadata), songIndex));
                this.MainFrame.IsBottomAppBarOpen = true;
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SongsBindingModel.SelectedItems.CollectionChanged += this.SelectedItemsOnCollectionChanged;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var request = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;
            if (request == null)
            {
                throw new NotSupportedException("Request parameter should be PlaylistNavigationRequest.");
            }

            var songs = await this.playlistsService.GetSongsAsync(request.PlaylistType, request.PlaylistId);
            var playlist = await this.playlistsService.GetAsync(request.PlaylistType, request.PlaylistId);

            await this.Dispatcher.RunAsync(
                () =>
                    {
                        this.BindingModel.SongsBindingModel.SetCollection(songs);
                        this.BindingModel.Playlist = (TPlaylist)playlist;
                    });
        }

        protected virtual IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.Play, "Play", this.PlaySongCommand);
            yield return new CommandMetadata(CommandIcon.Add, "Playlist", this.AddToPlaylistCommand);
        }

        private void Play()
        {
            var selectedIndexes = this.BindingModel.SongsBindingModel.GetSelectedIndexes().ToList();
            if (selectedIndexes.Count > 0)
            {
                this.playQueueService.PlayAsync(this.BindingModel.Playlist, this.BindingModel.SongsBindingModel.Songs.Select(s => s.Metadata), selectedIndexes[0]);
                this.MainFrame.IsBottomAppBarOpen = true;
            }
        }

        private void AddToPlaylist()
        {
            var selectedSongs = this.BindingModel.SongsBindingModel.GetSelectedSongs().ToList();
            if (selectedSongs.Count > 0)
            {
                this.MainFrame.ShowPopup<IAddToPlaylistPopupView>(PopupRegion.AppToolBarLeft, selectedSongs);
            }
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null)
            {
                this.Logger.LogTask(this.metadataEditService.UpdateRatingAsync(
                        ((SongBindingModel)ratingEventArgs.CommandParameter).Metadata, (byte)ratingEventArgs.Value));
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.PlaySongCommand.RaiseCanExecuteChanged();
                        this.AddToPlaylistCommand.RaiseCanExecuteChanged();

                        if (this.BindingModel.SongsBindingModel.SelectedItems.Count > 0)
                        {
                            this.MainFrame.SetContextCommands(this.GetContextCommands());
                        }
                        else
                        {
                            this.MainFrame.ClearContextCommands();
                        }
                    });
        }
    }
}