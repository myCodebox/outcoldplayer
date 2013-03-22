// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
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
        : DataPagePresenterBase<TView, PlaylistPageViewBindingModel<TPlaylist>>
        where TPlaylist : class, IPlaylist 
        where TView : IDataPageView
    {
        private readonly IPlayQueueService playQueueService;
        private readonly ISongMetadataEditService metadataEditService;
        private readonly IPlaylistsService playlistsService;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
        {
            this.playQueueService = container.Resolve<IPlayQueueService>();
            this.metadataEditService = container.Resolve<ISongMetadataEditService>();
            this.playlistsService = container.Resolve<IPlaylistsService>();

            this.PlaySongCommand = new DelegateCommand(this.Play, () => this.BindingModel != null && this.BindingModel.SelectedSong != null);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist, () => this.BindingModel != null && this.BindingModel.SelectedSong != null);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
        }

        public DelegateCommand PlaySongCommand { get; set; }

        public DelegateCommand AddToPlaylistCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.BindingModel.Songs = null;
            this.BindingModel.Playlist = default(TPlaylist);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.Subscribe(() => this.BindingModel.SelectedSong, this.SelectedSongChanged);
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

            this.BindingModel.Songs = songs.Select(x => new SongBindingModel(x)).ToList();
            this.BindingModel.Playlist = (TPlaylist)playlist;
            this.BindingModel.SelectedSongIndex = -1;
        }

        protected virtual IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.Play, "Play", this.PlaySongCommand);
            yield return new CommandMetadata(CommandIcon.Add, "Playlist", this.AddToPlaylistCommand);
        }

        private void Play()
        {
            var selectedSong = this.BindingModel.SelectedSong;
            if (selectedSong != null)
            {
                this.playQueueService.PlayAsync(this.BindingModel.Playlist, this.BindingModel.Songs.Select(s => s.Metadata), this.BindingModel.Songs.IndexOf(selectedSong));
                this.Toolbar.IsBottomAppBarOpen = true;
            }
        }

        private void AddToPlaylist()
        {
            var selectedSong = this.BindingModel.SelectedSong;
            if (selectedSong != null)
            {
                this.Toolbar.ShowPopup<IAddToPlaylistPopupView>(new List<SongBindingModel> { selectedSong });
            }
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null)
            {
                this.Logger.LogTask(this.metadataEditService.UpdateRatingAsync(
                        (SongBindingModel)ratingEventArgs.CommandParameter, (byte)ratingEventArgs.Value));
            }
        }

        private void SelectedSongChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.PlaySongCommand.RaiseCanExecuteChanged();
            this.AddToPlaylistCommand.RaiseCanExecuteChanged();

            if (this.BindingModel.SelectedSong != null)
            {
                this.Toolbar.SetContextCommands(this.GetContextCommands());
            }
            else
            {
                this.Toolbar.ClearContextCommands();
            }
        }
    }
}