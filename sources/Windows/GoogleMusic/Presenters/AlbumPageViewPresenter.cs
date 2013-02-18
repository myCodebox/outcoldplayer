// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class AlbumPageViewPresenter : PagePresenterBase<IAlbumPageView, AlbumPageViewBindingModel>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;

        public AlbumPageViewPresenter(
            IDependencyResolverContainer container, 
            ICurrentPlaylistService currentPlaylistService)
            : base(container)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.PlayCommand = new DelegateCommand(this.Play, () => this.BindingModel != null && this.BindingModel.SelectedSong != null);
            this.AddToPlaylistCommand = new DelegateCommand(this.AddToPlaylist, () => this.BindingModel != null && this.BindingModel.SelectedSong != null);
        }

        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand AddToPlaylistCommand { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.Subscribe(() => this.BindingModel.SelectedSong, this.SelectedSongChanged);
        }

        protected override void LoadData(NavigatedToEventArgs navigatedToEventArgs)
        {
            var album = navigatedToEventArgs.Parameter as Album;
            if (album == null)
            {
                throw new NotSupportedException("Current view supports only album-playlists.");
            }

            this.BindingModel.Album = album;
            this.BindingModel.SelectedSong = null;
        }

        private void Play()
        {
            var selectedSong = this.BindingModel.SelectedSong;
            if (selectedSong != null)
            {
                this.currentPlaylistService.ClearPlaylist();
                this.currentPlaylistService.SetPlaylist(this.BindingModel.Album);
                this.currentPlaylistService.PlayAsync(this.BindingModel.Album.Songs.IndexOf(selectedSong));
            }
        }

        private void AddToPlaylist()
        {
        }

        private void SelectedSongChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.PlayCommand.RaiseCanExecuteChanged();
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

        private IEnumerable<CommandMetadata> GetContextCommands()
        {
            yield return new CommandMetadata(CommandIcon.Play, "Play", this.PlayCommand);
            yield return new CommandMetadata(CommandIcon.Add, "Add To Playlist", this.AddToPlaylistCommand);
        }
    }
}