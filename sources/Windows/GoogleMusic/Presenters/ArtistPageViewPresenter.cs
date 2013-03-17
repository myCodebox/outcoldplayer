// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;
    using OutcoldSolutions.Presenters;

    public class ArtistPageViewPresenter : DataPagePresenterBase<IArtistPageView, ArtistPageViewBindingModel>
    {
        private readonly IPlayQueueService playQueueService;
        private readonly INavigationService navigationService;

        public ArtistPageViewPresenter(
            IPlayQueueService playQueueService,
            INavigationService navigationService)
        {
            this.playQueueService = playQueueService;
            this.navigationService = navigationService;
            this.PlayCommand = new DelegateCommand(this.Play);
            this.ShowAllCommand = new DelegateCommand(this.ShowAll);
        }
        
        public DelegateCommand PlayCommand { get; set; }

        public DelegateCommand ShowAllCommand { get; set; }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs)
        {
            var artist = navigatedToEventArgs.Parameter as ArtistBindingModel;
            if (artist == null)
            {
                throw new NotSupportedException("Current view cannot show not-artists playlists.");
            }

            await Task.Run(
                () =>
                    {
                        this.BindingModel.Artist = artist;
                        this.BindingModel.Albums =
                            SongsGrouping.GroupByAlbums(artist.Songs)
                                         .Select(x => new PlaylistBindingModel(x) { PlayCommand = this.PlayCommand })
                                         .OrderBy(x => x.Playlist.Title, StringComparer.CurrentCultureIgnoreCase)
                                         .ToList();
                    });
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
                PlaylistBaseBindingModel playlist = commandParameter as PlaylistBaseBindingModel;
                if (playlist == null)
                {
                    playlist = this.BindingModel.Artist;
                    this.navigationService.NavigateTo<IPlaylistPageView>(this.BindingModel.Artist);
                }
                else
                {
                    this.navigationService.ResolveAndNavigateTo<PlaylistViewResolver>(playlist);
                }

                this.playQueueService.PlayAsync(playlist);
                this.playQueueService.PlayAsync();
                this.Toolbar.IsBottomAppBarOpen = true;
            }
        }
    }
}