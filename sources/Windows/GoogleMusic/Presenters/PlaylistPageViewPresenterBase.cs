// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Specialized;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Controls;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.InversionOfControl;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class PlaylistPageViewPresenterBase<TView, TPlaylist> 
        : PagePresenterBase<TView, PlaylistPageViewBindingModel<TPlaylist>>
        where TPlaylist : class, IPlaylist 
        where TView : IPageView
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongsService metadataEditService;
        private readonly IPlaylistsService playlistsService;
        private readonly IApplicationResources resources;
        private readonly IApplicationStateService stateService;
        private readonly ISelectedObjectsService selectedObjectsService;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
        {
            this.playQueueService = container.Resolve<IPlayQueueService>();
            this.metadataEditService = container.Resolve<ISongsService>();
            this.playlistsService = container.Resolve<IPlaylistsService>();
            this.resources = container.Resolve<IApplicationResources>();
            this.stateService = container.Resolve<IApplicationStateService>();
            this.selectedObjectsService = container.Resolve<ISelectedObjectsService>();

            this.RateSongCommand = new DelegateCommand(this.RateSong);
        }

        public DelegateCommand RateSongCommand { get; set; }

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

            this.EventAggregator.GetEvent<SelectionClearedEvent>()
               .Subscribe<SelectionClearedEvent>(async (e) => await this.Dispatcher.RunAsync(() => this.BindingModel.SongsBindingModel.ClearSelectedItems()));
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

        private void SelectedItemsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.selectedObjectsService.Update(
                e.NewItems == null ? null : e.NewItems.Cast<SongBindingModel>().Select(x => x.Metadata),
                e.OldItems == null ? null: e.OldItems.Cast<SongBindingModel>().Select(x => x.Metadata));
        }
    }
}