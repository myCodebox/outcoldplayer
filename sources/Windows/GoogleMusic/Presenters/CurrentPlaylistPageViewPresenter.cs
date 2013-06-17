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
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.GoogleMusic.Views.Popups;
    using OutcoldSolutions.GoogleMusic.Web;
    using OutcoldSolutions.Presenters;
    using OutcoldSolutions.Views;

    public class CurrentPlaylistPageViewPresenter : PagePresenterBase<ICurrentPlaylistPageView>
    {
        private readonly IApplicationResources resources;
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsService metadataEditService;
        private readonly ISongsCachingService cachingService;
        private readonly IApplicationStateService stateService;
        private readonly INavigationService navigationService;
        private readonly IRadioWebService radioWebService;

        internal CurrentPlaylistPageViewPresenter(
            IApplicationResources resources,
            IPlayQueueService playQueueService,
            ISongsService metadataEditService,
            ISongsCachingService cachingService,
            IApplicationStateService stateService,
            INavigationService navigationService,
            IRadioWebService radioWebService,
            SongsBindingModel songsBindingModel)
        {
            this.resources = resources;
            this.playQueueService = playQueueService;
            this.metadataEditService = metadataEditService;
            this.cachingService = cachingService;
            this.stateService = stateService;
            this.navigationService = navigationService;
            this.radioWebService = radioWebService;
            this.BindingModel = songsBindingModel;

            this.playQueueService.QueueChanged += async (sender, args) => await this.Dispatcher.RunAsync(this.UpdateSongs);

            //this.SaveAsPlaylistCommand = new DelegateCommand(this.SaveAsPlaylist, () => this.BindingModel.Songs.Count > 0);
            this.RemoveSelectedSongCommand = new DelegateCommand(this.RemoveSelectedSong, () => this.BindingModel.SelectedItems.Count > 0);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist, () => this.BindingModel.SelectedItems.Count > 0);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
            this.DownloadCommand = new DelegateCommand(this.Download, () => this.BindingModel.SelectedItems.Count  > 0);
            this.UnPinCommand = new DelegateCommand(this.UnPin, () => this.BindingModel.SelectedItems.Count  > 0);
            this.StartRadioCommand = new DelegateCommand(this.StartRadio, () => this.BindingModel != null && this.BindingModel.SelectedItems.Count == 1);

            this.playQueueService.StateChanged += async (sender, args) => await this.Dispatcher.RunAsync(async () => 
                {
                    if (this.BindingModel.SelectedItems.Count == 0)
                    {
                        if (this.BindingModel.Songs != null && args.CurrentSong != null)
                        {
                            var currentSong = this.BindingModel.Songs.FirstOrDefault(x => string.Equals(x.Metadata.ProviderSongId, args.CurrentSong.ProviderSongId, StringComparison.Ordinal));
                            if (currentSong != null)
                            {
                                await this.View.ScrollIntoCurrentSongAsync(currentSong);
                            }
                        }
                    }
                });
        }

        public DelegateCommand AddToPlaylistCommand { get; private set; }

        public DelegateCommand SaveAsPlaylistCommand { get; private set; }

        public DelegateCommand RemoveSelectedSongCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public DelegateCommand DownloadCommand { get; set; }

        public DelegateCommand UnPinCommand { get; set; }

        public DelegateCommand StartRadioCommand { get; set; }

        public SongsBindingModel BindingModel { get; set; }

        public void SelectPlayingSong()
        {
            this.BindingModel.ClearSelectedItems();
            this.BindingModel.SelectSongByIndex(this.playQueueService.GetCurrentSongIndex());
        }

        public void PlaySong(SongBindingModel songBindingModel)
        {
            int songIndex = this.BindingModel.Songs.IndexOf(songBindingModel);
            this.Logger.LogTask(this.playQueueService.PlayAsync(songIndex));
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.SelectedItems.CollectionChanged += this.SelectedSongChanged;
        }

        //protected override IEnumerable<CommandMetadata> GetViewCommands()
        //{
        //    yield return new CommandMetadata(CommandIcon.Page, "Save", this.SaveAsPlaylistCommand);
        //}

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            await this.Dispatcher.RunAsync(
                () =>
                    {
                        this.UpdateSongs();

                        if (navigatedToEventArgs.Parameter is bool && (bool)navigatedToEventArgs.Parameter)
                        {
                            this.SelectPlayingSong();
                        }
                    });
        }

        private void AddToPlaylist()
        {
            var selectedSongs = this.BindingModel.GetSelectedSongs().ToList();
            if (selectedSongs != null)
            {
                this.MainFrame.ShowPopup<IAddToPlaylistPopupView>(
                    PopupRegion.AppToolBarLeft, 
                    selectedSongs).Closed += this.AddToPlaylist_Closed;
            }
        }

        private void AddToPlaylist_Closed(object sender, EventArgs eventArgs)
        {
            ((IPopupView)sender).Closed -= this.AddToPlaylist_Closed;
            if (eventArgs is AddToPlaylistCompletedEventArgs)
            {
                this.BindingModel.ClearSelectedItems();
            }
        }

        private async void Download()
        {
            try
            {
                await this.cachingService.QueueForDownloadAsync(this.BindingModel.GetSelectedSongs());
                this.BindingModel.ClearSelectedItems();
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
                await this.cachingService.ClearCachedAsync(this.BindingModel.GetSelectedSongs());
                this.BindingModel.ClearSelectedItems();
            }
            catch (Exception e)
            {
                this.Logger.Error(e, "Cannot clear cache for selected songs");
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

        private async void RemoveSelectedSong()
        {
            var collection = this.BindingModel.GetSelectedIndexes().ToList();
            if (collection.Count > 0)
            {
                await this.playQueueService.RemoveAsync(collection);

                await this.Dispatcher.RunAsync(
                    () =>
                        {
                            if (collection.Count == 1)
                            {
                                int selectedSongIndex = collection.First();
                                if (selectedSongIndex < this.BindingModel.Songs.Count)
                                {
                                    this.BindingModel.SelectSongByIndex(selectedSongIndex);
                                }
                                else if (this.BindingModel.Songs.Count > 0)
                                {
                                    this.BindingModel.SelectSongByIndex(selectedSongIndex - 1);
                                }
                            }
                        });
            }
        }

        private void UpdateSongs()
        {
            this.BindingModel.SetCollection(this.playQueueService.GetQueue());
        }

        private void SelectedSongChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            this.AddToPlaylistCommand.RaiseCanExecuteChanged();
            this.RemoveSelectedSongCommand.RaiseCanExecuteChanged();
            this.StartRadioCommand.RaiseCanExecuteChanged();

            if (this.BindingModel.SelectedItems.Count > 0)
            {
                this.MainFrame.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.MainFrame.ClearContextCommands();
            }

            this.AddToPlaylistCommand.RaiseCanExecuteChanged();
            // this.SaveAsPlaylistCommand.RaiseCanExecuteChanged();
            this.DownloadCommand.RaiseCanExecuteChanged();
            this.RemoveSelectedSongCommand.RaiseCanExecuteChanged();
        }

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            if (this.stateService.IsOnline())
            {
                yield return new CommandMetadata(CommandIcon.Add, this.resources.GetString("Toolbar_PlaylistButton"), this.AddToPlaylistCommand);
                yield return new CommandMetadata(CommandIcon.MusicInfo, this.resources.GetString("Toolbar_StartRadio"), this.StartRadioCommand);
            }

            yield return new CommandMetadata(CommandIcon.Remove, this.resources.GetString("Toolbar_QueueButton"), this.RemoveSelectedSongCommand);

            if (!this.playQueueService.IsRadio)
            {
                if (this.BindingModel.SelectedItems.Any(x => !x.IsCached))
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
        }

        private async void StartRadio()
        {
            if (this.StartRadioCommand.CanExecute())
            {
                var songBindingModel = this.BindingModel.SelectedItems.FirstOrDefault();
                if (songBindingModel != null)
                {
                    try
                    {
                        this.IsDataLoading = true;

                        var radioResp = await this.radioWebService.CreateStationAsync(
                                songBindingModel.Metadata.ProviderSongId,
                                songBindingModel.Metadata.IsLibrary ? "TRACK_LOCKER_ID" : "TRACK_MATCHED_ID",
                                songBindingModel.Metadata.Title);

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
    }
}