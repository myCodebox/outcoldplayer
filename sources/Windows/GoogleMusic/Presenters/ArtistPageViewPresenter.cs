// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class ArtistPageViewPresenter : PagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly ICurrentPlaylistService currentPlaylistService;
        private readonly INavigationService navigationService;

        public ArtistPageViewPresenter(
            IDependencyResolverContainer container, 
            ICurrentPlaylistService currentPlaylistService,
            INavigationService navigationService)
            : base(container)
        {
            this.currentPlaylistService = currentPlaylistService;
            this.navigationService = navigationService;
            this.PlayCommand = new DelegateCommand(this.Play);
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);
        }
        
        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand ShowAllCommand { get; set; }

        protected override void LoadData(NavigatedToEventArgs navigatedToEventArgs)
        {
            var artist = navigatedToEventArgs.Parameter as Artist;
            if (artist == null)
            {
                throw new NotSupportedException("Current view cannot show not-artists playlists.");
            }

            this.BindingModel.Artist = artist;
            this.BindingModel.Albums = SongsGrouping.GroupByAlbums(artist.Songs)
                .Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand })
                .ToList();
        }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.Play, "Play All", this.PlayCommand);
            yield return new CommandMetadata(CommandIcon.List, "Show All", this.ShowAllCommand);

            // TODO: Will be good to have shuffle all 
        }

        private void ShowAll()
        {
            if (this.BindingModel.Artist != null)
            {
                this.navigationService.NavigateTo<IPlaylistPageView>(this.BindingModel.Artist);
            }
        }

        private void Play(object commandParameter)
        {
            if (this.BindingModel.Artist != null)
            {
                Playlist playlist = commandParameter as Playlist;
                if (playlist == null)
                {
                    playlist = this.BindingModel.Artist;
                    this.navigationService.NavigateTo<IPlaylistPageView>(this.BindingModel.Artist);
                }
                else
                {
                    this.navigationService.NavigateToView<PlaylistViewResolver>(playlist);
                }

                this.currentPlaylistService.SetPlaylist(playlist);
                this.currentPlaylistService.PlayAsync();
            }
        }
    }
}