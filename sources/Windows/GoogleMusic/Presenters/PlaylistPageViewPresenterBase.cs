// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
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
        where TPlaylist : Playlist 
        where TView : IDataPageView
    {
        private readonly ICurrentPlaylistService currentPlaylistService;
        private readonly ISongMetadataEditService metadataEditService;

        public PlaylistPageViewPresenterBase(IDependencyResolverContainer container)
            : base(container)
        {
            this.currentPlaylistService = container.Resolve<ICurrentPlaylistService>();
            this.metadataEditService = container.Resolve<ISongMetadataEditService>();

            this.PlaySongCommand = new DelegateCommand(this.Play, () => this.BindingModel != null && this.BindingModel.SelectedSong != null);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist, () => this.BindingModel != null && this.BindingModel.SelectedSong != null);
            this.RateSongCommand = new DelegateCommand(this.RateSong);
        }

        public DelegateCommand PlaySongCommand { get; set; }

        public DelegateCommand AddToPlaylistCommand { get; set; }

        public DelegateCommand RateSongCommand { get; set; }

        public override void OnNavigatedTo(NavigatedToEventArgs parameter)
        {
            this.BindingModel.Playlist = null;

            base.OnNavigatedTo(parameter);
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.Subscribe(() => this.BindingModel.SelectedSong, this.SelectedSongChanged);
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            await Task.Run(
                () =>
                    {
                        var playlist = navigatedToEventArgs.Parameter as TPlaylist;
                        if (playlist == null)
                        {
                            throw new NotSupportedException(
                                string.Format("Current view supports only {0}-playlists.", typeof(TPlaylist)));
                        }

                        // TODO: We need to refresh playlist before set it here.
                        this.BindingModel.Playlist = playlist;
                        this.BindingModel.SelectedSongIndex = -1;
                    });
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
                this.currentPlaylistService.ClearPlaylist();
                this.currentPlaylistService.SetPlaylist(this.BindingModel.Playlist);
                this.currentPlaylistService.PlayAsync(this.BindingModel.Playlist.Songs.IndexOf(selectedSong));
            }
        }

        private void AddToPlaylist()
        {
            var selectedSong = this.BindingModel.SelectedSong;
            if (selectedSong != null)
            {
                this.Toolbar.ShowPopup<IAddToPlaylistPopupView>(new List<Song> { selectedSong });
            }
        }

        private void RateSong(object parameter)
        {
            var ratingEventArgs = parameter as RatingEventArgs;
            Debug.Assert(ratingEventArgs != null, "ratingEventArgs != null");
            if (ratingEventArgs != null)
            {
                this.Logger.LogTask(this.metadataEditService.UpdateRatingAsync(
                        (Song)ratingEventArgs.CommandParameter, (byte)ratingEventArgs.Value));
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