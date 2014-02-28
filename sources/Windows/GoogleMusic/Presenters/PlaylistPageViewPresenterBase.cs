// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.Diagnostics;
    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
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
        private readonly IApplicationResources resources;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;
        private readonly IRadioWebService radioWebService;
        private readonly INavigationService navigationService;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
        {
            this.playQueueService = container.Resolve<IPlayQueueService>();
            this.metadataEditService = container.Resolve<ISongsService>();
            this.playlistsService = container.Resolve<IPlaylistsService>();
            this.resources = container.Resolve<IApplicationResources>();
            this.cachingService = container.Resolve<ISongsCachingService>();
            this.stateService = container.Resolve<IApplicationStateService>();
            this.radioWebService = container.Resolve<IRadioWebService>();
            this.navigationService = container.Resolve<INavigationService>();

            this.QueueCommand = new DelegateCommand(this.Queue, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count > 0);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count > 0);
            this.DownloadCommand = new DelegateCommand(this.Download, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count > 0);
            this.UnPinCommand = new DelegateCommand(this.UnPin, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count > 0);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
            this.StartRadioCommand = new DelegateCommand(this.StartRadio, () => this.BindingModel != null && this.BindingModel.SongsBindingModel.SelectedItems.Count == 1);
        }

        public DelegateCommand QueueCommand { get; set; }

        public DelegateCommand DownloadCommand { get; set; }

        public DelegateCommand UnPinCommand { get; set; }

        public DelegateCommand AddToPlaylistCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public DelegateCommand StartRadioCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.SongsBindingModel.SetCollection(null);
            this.BindingModel.Playlist = default(TPlaylist);
            this.BindingModel.Type = null;
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
                        this.BindingModel.Type = this.resources.GetTitle(playlist.PlaylistType);

                        if (!string.IsNullOrEmpty(request.SongId))
                        {
                            var songBindingModel = this.BindingModel.SongsBindingModel.Songs.FirstOrDefault(s => s.Metadata.SongId == request.SongId);
                            if (songBindingModel != null)
                            {
                                this.BindingModel.SongsBindingModel.SelectedItems.Add(songBindingModel);
                            }
                        }
                    });
        }

        protected virtual IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.OpenWith, this.resources.GetString("Toolbar_QueueButton"), this.QueueCommand);
            if (this.stateService.IsOnline())
            {
                yield return new CommandMetadata(CommandIcon.Add, this.resources.GetString("Toolbar_PlaylistButton"), this.AddToPlaylistCommand);

                if (this.BindingModel.SongsBindingModel.SelectedItems.Count == 1)
                {
                    yield return new CommandMetadata(CommandIcon.MusicInfo, this.resources.GetString("Toolbar_StartRadio"), this.StartRadioCommand);
                }
            }

            if (this.BindingModel.SongsBindingModel.SelectedItems.Any(x => !x.IsCached))
            {
                if (this.stateService.IsOnline())
                {
                    yield return new CommandMetadata(CommandIcon.Pin, this.resources.GetString("Toolbar_KeepLocal"), this.DownloadCommand);
                }
            }
            else
            {
                yield return new CommandMetadata(CommandIcon.UnPin, this.resources.GetString("Toolbar_RemoveLocal"), this.UnPinCommand);
            }
        }

        private void Queue()
        {
            this.MainFrame.ShowPopup<IQueueActionsPopupView>(
                PopupRegion.AppToolBarLeft,
                new SelectedItems(this.BindingModel.SongsBindingModel.GetSelectedSongs().ToList())).Closed += this.QueueActionsPopupView_Closed;
        }

        private async void Download()
        {
            try
            {
                await this.cachingService.QueueForDownloadAsync(this.BindingModel.SongsBindingModel.GetSelectedSongs());
                this.BindingModel.SongsBindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot add songs to download queue");
            }
        }

        private async void UnPin()
        {
            try
            {
                await this.cachingService.ClearCachedAsync(this.BindingModel.SongsBindingModel.GetSelectedSongs());
                this.BindingModel.SongsBindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot clear cache for selected songs.");
            }
        }

        private void QueueActionsPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.QueueActionsPopupView_Closed;
            if (eventArgs is QueueActionsCompletedEventArgs)
            {
                this.BindingModel.SongsBindingModel.ClearSelectedItems();
            }
        }

        private void AddToPlaylist()
        {
            var selectedSongs = this.BindingModel.SongsBindingModel.GetSelectedSongs().ToList();
            if (selectedSongs.Count > 0)
            {
                this.MainFrame.ShowPopup<IAddToPlaylistPopupView>(
                    PopupRegion.AppToolBarLeft,
                    selectedSongs).Closed += this.AddToPlaylistPopupView_Closed;
            }
        }

        private void AddToPlaylistPopupView_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.AddToPlaylistPopupView_Closed;
            if (eventArgs is AddToPlaylistCompletedEventArgs)
            {
                this.BindingModel.SongsBindingModel.ClearSelectedItems();
            }
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null && this.stateService.IsOnline())
            {
                this.Logger.LogTask(this.metadataEditService.UpdateRatingAsync(
                        ((SongBindingModel)ratingEventArgs.CommandParameter).Metadata, (byte)ratingEventArgs.Value));
            }
        }

        private async void StartRadio()
        {
            if (this.StartRadioCommand.CanExecute())
            {
                var songBindingModel = this.BindingModel.SongsBindingModel.SelectedItems.FirstOrDefault();
                if (songBindingModel != null)
                {
                    try
                    {
                        this.IsDataLoading = true;

                        var radioResp = await this.radioWebService.CreateStationAsync(songBindingModel.Metadata);

                        if (radioResp != null)
                        {
                            await this.playQueueService.PlayAsync(radioResp.Item1, radioResp.Item2, -1);
                            this.IsDataLoading = false;
                            this.navigationService.NavigateTo<ICurrentPlaylistPageView>();
                        }
                    }
                    catch (Exception e)
                    {
                        this.Logger.Error(e, "Cannot start radio");
                    }
                    finally
                    {
                        this.IsDataLoading = false;
                    }
                }
            }
        }

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.Dispatcher.RunAsync(
                () =>
                    {
                        this.QueueCommand.RaiseCanExecuteChanged();
                        this.AddToPlaylistCommand.RaiseCanExecuteChanged();
                        this.DownloadCommand.RaiseCanExecuteChanged();
                        this.StartRadioCommand.RaiseCanExecuteChanged();

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