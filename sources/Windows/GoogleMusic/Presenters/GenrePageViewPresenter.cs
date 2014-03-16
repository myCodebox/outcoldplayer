// --------------------------------------------------------------------------------------------------------------------
// OutcoldSolutions (http://outcoldsolutions.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class GenrePageViewPresenter: PlaylistsPageViewPresenterBase<IGenrePageView, PlaylistsPageViewBindingModel>
    {
        private readonly IApplicationResources resources;

        private readonly IAlbumsRepository albumsRepository;

        private readonly INavigationService navigationService;

        private Genre genre;

        public GenrePageViewPresenter(
            IApplicationResources resources, 
            IPlaylistsService playlistsService,
            IAlbumsRepository albumsRepository,
            INavigationService navigationService)
            : base(resources, playlistsService)
        {
            this.resources = resources;
            this.albumsRepository = albumsRepository;
            this.navigationService = navigationService;

            this.ShowAllCommand = new DelegateCommand(
                () => this.navigationService.NavigateTo<IPlaylistPageView>(new PlaylistNavigationRequest(this.genre)));
        }

        public DelegateCommand ShowAllCommand { get; set; }

        protected override IEnumerable<CommandMetadata> GetViewCommands()
        {
            yield return new CommandMetadata(CommandIcon.List, this.resources.GetString("Toolbar_ShowAllButton"), this.ShowAllCommand);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            base.OnNavigatingFrom(eventArgs);

            this.genre = null;
        }

        protected override async Task LoadDataAsync(NavigatedToEventArgs navigatedToEventArgs, CancellationToken cancellationToken)
        {
            var playlistNavigationRequest = navigatedToEventArgs.Parameter as PlaylistNavigationRequest;

            if (playlistNavigationRequest != null)
            {
                this.genre = playlistNavigationRequest.Playlist as Genre;
                if (this.genre != null)
                {
                    var artists = await this.albumsRepository.FindGenreAlbumsAsync(this.genre.TitleNorm);
                    await this.Dispatcher.RunAsync(
                        () =>
                        {
                            this.BindingModel.Title = this.genre.Title;
                            this.BindingModel.Subtitle = "Genre - Albums";
                            this.BindingModel.PlaylistType = PlaylistType.Album;
                            this.BindingModel.Playlists = artists.Cast<IPlaylist>().ToList();
                        });
                }
            }
        }
    }
}
