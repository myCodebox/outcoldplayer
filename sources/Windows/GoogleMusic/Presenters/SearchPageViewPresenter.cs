// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class SearchPageViewPresenter : PagePresenterBase<ISearchPageView, SearchPageViewBindingModel>
    {
        private readonly ISongsRepository songsRepository;
        private readonly IPlaylistsService playlistsService;
        private readonly INavigationService navigationService;

        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1);
        private CancellationTokenSource cancellationTokenSource;

        private bool isSearching;

        public SearchPageViewPresenter(
            IApplicationResources resources,
            ISongsRepository songsRepository,
            IPlaylistsService playlistsService,
            INavigationService navigationService)
        {
            this.songsRepository = songsRepository;
            this.playlistsService = playlistsService;
            this.navigationService = navigationService;

            this.NavigateToSongs = new DelegateCommand(
               () => this.navigationService.NavigateTo<IPlaylistPageView>(
                   new PlaylistNavigationRequest(
                       string.Format(resources.GetString("SearchPageView_SubtitleFormat"), this.BindingModel.SearchText),
                       "Songs",
                       this.BindingModel.Songs)));

            this.NavigateToArtists = new DelegateCommand(
               () => this.navigationService.NavigateTo<IPlaylistsPageView>(
                   new PlaylistNavigationRequest(
                       string.Format(resources.GetString("SearchPageView_SubtitleFormat"), this.BindingModel.SearchText),
                       "Artists",
                       this.BindingModel.Artists)));

            this.NavigateToAlbums = new DelegateCommand(
                () => this.navigationService.NavigateTo<IPlaylistsPageView>(
                    new PlaylistNavigationRequest(
                        string.Format(resources.GetString("SearchPageView_SubtitleFormat"), this.BindingModel.SearchText),
                        "Albums",
                        this.BindingModel.Albums)));

            this.NavigateToRadios = new DelegateCommand(
                () => this.navigationService.NavigateTo<IPlaylistsPageView>(
                    new PlaylistNavigationRequest(
                        string.Format(resources.GetString("SearchPageView_SubtitleFormat"), this.BindingModel.SearchText),
                        "Radio Stations",
                        this.BindingModel.RadioStations)));


            this.NavigateToGenres = new DelegateCommand(
                () => this.navigationService.NavigateTo<IPlaylistsPageView>(
                    new PlaylistNavigationRequest(
                        string.Format(resources.GetString("SearchPageView_SubtitleFormat"), this.BindingModel.SearchText),
                        "Genres",
                        this.BindingModel.Genres)));

            this.NavigateToUserPlaylists = new DelegateCommand(
                () => this.navigationService.NavigateTo<IPlaylistsPageView>(
                    new PlaylistNavigationRequest(
                        string.Format(resources.GetString("SearchPageView_SubtitleFormat"), this.BindingModel.SearchText),
                        "Playlists",
                        this.BindingModel.UserPlaylists)));
        }

        public DelegateCommand NavigateToSongs { get; set; }

        public DelegateCommand NavigateToArtists { get; set; }

        public DelegateCommand NavigateToAlbums { get; set; }

        public DelegateCommand NavigateToGenres { get; set; }

        public DelegateCommand NavigateToUserPlaylists { get; set; }

        public DelegateCommand NavigateToRadios { get; set; }

        public bool IsSearching
        {
            get
            {
                return this.isSearching;
            }
            set
            {
                this.SetValue(ref this.isSearching, value);
            }
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.BindingModel.Subscribe(() => this.BindingModel.SearchText, OnSearchChanged);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            eventArgs.State["SearchText"] = this.BindingModel.SearchText;
        }

        protected override Task LoadDataAsync(NavigatedToEventArgs eventArgs)
        {
            return this.Dispatcher.RunAsync(
                () =>
                {
                    if (eventArgs.State.ContainsKey("SearchText"))
                    {
                        this.BindingModel.SearchText = eventArgs.State["SearchText"] as string;
                    }
                });
        }

        private async void OnSearchChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

            if (this.cancellationTokenSource != null)
            {
                this.cancellationTokenSource.Cancel();
            }

            CancellationTokenSource source = this.cancellationTokenSource = new CancellationTokenSource();
            string searchText = this.BindingModel.SearchText;

            await this.Dispatcher.RunAsync(
                () =>
                {
                    this.IsSearching = true;
                });

            this.mutex.Release(1);

            var result = (string.IsNullOrEmpty(searchText) || searchText.Length < 2) ? new SearchResult(searchText) : await this.Search(searchText, source.Token);

            if (!source.IsCancellationRequested)
            {
                await this.mutex.WaitAsync().ConfigureAwait(continueOnCapturedContext: false);

                if (this.cancellationTokenSource == source)
                {
                    this.cancellationTokenSource = null;
                }

                this.mutex.Release(1);

                await this.Dispatcher.RunAsync(
                    () =>
                    {
                        if (!source.IsCancellationRequested)
                        {
                            this.BindingModel.FreezeNotifications();

                            this.BindingModel.Artists = result.Artists;
                            this.BindingModel.Albums = result.Albums;
                            this.BindingModel.UserPlaylists = result.UserPlaylists;
                            this.BindingModel.RadioStations = result.RadioStations;
                            this.BindingModel.Songs = result.Songs;

                            this.BindingModel.UnfreezeNotifications();

                            this.IsSearching = false;
                        }
                    });
            }
        }

        private async Task<SearchResult> Search(string query, CancellationToken cancellationToken)
        {
            var searchResult = new SearchResult(query);

            searchResult.Songs = await this.songsRepository.SearchAsync(query);

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.Artists = (await this.playlistsService.SearchAsync(PlaylistType.Artist, query)).ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.Albums = (await this.playlistsService.SearchAsync(PlaylistType.Album, query)).ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.Genres = (await this.playlistsService.SearchAsync(PlaylistType.Genre, query)).ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.UserPlaylists = (await this.playlistsService.SearchAsync(PlaylistType.UserPlaylist, query)).ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.RadioStations = (await this.playlistsService.SearchAsync(PlaylistType.Radio, query)).ToList();

            return searchResult;
        }
    }
}