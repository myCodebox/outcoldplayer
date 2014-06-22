// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Presenters
{
    using System;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.BindingModels;
    using OutcoldSolutions.GoogleMusic.Diagnostics;
    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    public class SearchPageViewPresenter : PagePresenterBase<ISearchPageView, SearchPageViewBindingModel>
    {
        private readonly IApplicationStateService applicationStateService;
        private readonly ISongsRepository songsRepository;
        private readonly IPlaylistsService playlistsService;
        private readonly INavigationService navigationService;
        private readonly IAllAccessService allAccessService;
        private readonly ISettingsService settingsService;

        private readonly IAnalyticsService analyticsService;

        private readonly SemaphoreSlim mutex = new SemaphoreSlim(1);
        private CancellationTokenSource cancellationTokenSource;

        private bool isSearching;

        public SearchPageViewPresenter(
            IApplicationResources resources,
            IApplicationStateService applicationStateService,
            ISongsRepository songsRepository,
            IPlaylistsService playlistsService,
            INavigationService navigationService,
            IAllAccessService allAccessService,
            ISettingsService settingsService,
            IAnalyticsService analyticsService)
        {
            this.applicationStateService = applicationStateService;
            this.songsRepository = songsRepository;
            this.playlistsService = playlistsService;
            this.navigationService = navigationService;
            this.allAccessService = allAccessService;
            this.settingsService = settingsService;
            this.analyticsService = analyticsService;

            this.NavigateToSongs = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("Search", "Navigate", "Songs");
                    this.navigationService.NavigateTo<IPlaylistPageView>(
                        new PlaylistNavigationRequest(
                            string.Format(
                                resources.GetString("SearchPageView_SubtitleFormat"),
                                this.BindingModel.SearchText),
                            "Songs",
                            this.BindingModel.Songs));
                });

            this.NavigateToArtists = new DelegateCommand(
               () =>
               {
                   this.analyticsService.SendEvent("Search", "Navigate", "Artists");
                   this.navigationService.NavigateTo<IPlaylistsPageView>(
                       new PlaylistNavigationRequest(
                           string.Format(
                               resources.GetString("SearchPageView_SubtitleFormat"),
                               this.BindingModel.SearchText),
                           "Artists",
                           this.BindingModel.Artists));
               });

            this.NavigateToAlbums = new DelegateCommand(
                () => 
                {
                    this.analyticsService.SendEvent("Search", "Navigate", "Albums");
                    this.navigationService.NavigateTo<IPlaylistsPageView>(
                        new PlaylistNavigationRequest(
                            string.Format(
                                resources.GetString("SearchPageView_SubtitleFormat"),
                                this.BindingModel.SearchText),
                            "Albums",
                            this.BindingModel.Albums));
                });

            this.NavigateToRadios = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("Search", "Navigate", "Radio Stations");
                    this.navigationService.NavigateTo<IPlaylistsPageView>(
                        new PlaylistNavigationRequest(
                            string.Format(
                                resources.GetString("SearchPageView_SubtitleFormat"),
                                this.BindingModel.SearchText),
                            "Radio Stations",
                            this.BindingModel.RadioStations));
                });


            this.NavigateToGenres = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("Search", "Navigate", "Genres");
                    this.navigationService.NavigateTo<IPlaylistsPageView>(
                        new PlaylistNavigationRequest(
                            string.Format(
                                resources.GetString("SearchPageView_SubtitleFormat"),
                                this.BindingModel.SearchText),
                            "Genres",
                            this.BindingModel.Genres));
                });

            this.NavigateToUserPlaylists = new DelegateCommand(
                () =>
                {
                    this.analyticsService.SendEvent("Search", "Navigate", "Playlists");
                    this.navigationService.NavigateTo<IPlaylistsPageView>(
                        new PlaylistNavigationRequest(
                            string.Format(
                                resources.GetString("SearchPageView_SubtitleFormat"),
                                this.BindingModel.SearchText),
                            "Playlists",
                            this.BindingModel.UserPlaylists));
                });
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

            this.BindingModel.Subscribe(() => this.BindingModel.SearchText, this.OnSearchChanged);
        }

        public override void OnNavigatingFrom(NavigatingFromEventArgs eventArgs)
        {
            eventArgs.State["SearchText"] = this.BindingModel.SearchText;
        }

        protected override Task LoadDataAsync(NavigatedToEventArgs eventArgs, CancellationToken cancellationToken)
        {
            return this.Dispatcher.RunAsync(
                () =>
                {
                    this.BindingModel.IsOnline = this.applicationStateService.IsOnline();

                    if (eventArgs.State.ContainsKey("SearchText"))
                    {
                        this.BindingModel.SearchText = eventArgs.State["SearchText"] as string;
                    }
                    else
                    {
                        this.BindingModel.SearchText = string.Empty;
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

            SearchResult searchResult = null;

            if ((string.IsNullOrEmpty(searchText) || searchText.Length < 2))
            {
                searchResult = new SearchResult(searchText);
            }
            else
            {
                searchResult = await this.SearchAsync(searchText, source.Token);
            }

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

                            this.BindingModel.Artists = searchResult.Artists == null ? null : searchResult.Artists.Select(x => x.Playlist).ToList();
                            this.BindingModel.Albums = searchResult.Albums == null ? null : searchResult.Albums.Select(x => x.Playlist).ToList();
                            this.BindingModel.UserPlaylists = searchResult.UserPlaylists == null ? null : searchResult.UserPlaylists.Select(x => x.Playlist).ToList();
                            this.BindingModel.RadioStations = searchResult.RadioStations == null ? null : searchResult.RadioStations.Select(x => x.Playlist).ToList();
                            this.BindingModel.Songs = searchResult.Songs == null ? null : searchResult.Songs.Select(x => x.Song).ToList();

                            this.BindingModel.UnfreezeNotifications();

                            this.IsSearching = false;
                        }
                    });
            }
        }

        private async Task<SearchResult> SearchAsync(string query, CancellationToken cancellationToken)
        {
            var taskLocal = this.SearchLocalAsync(query, cancellationToken);
            var taskCloud = this.SearchCloudAsync(query, cancellationToken);

            await Task.WhenAll(taskLocal, taskCloud);

            var localResult = await taskLocal;
            var cloudResult = await taskCloud;

            if (!cancellationToken.IsCancellationRequested && cloudResult.Songs != null)
            {
                if (localResult.Songs == null)
                {
                    localResult.Songs = cloudResult.Songs;
                }
                else
                {
                    localResult.Songs = localResult.Songs.Union(cloudResult.Songs
                        .Where(cloud => localResult.Songs.All(local => !string.Equals(local.Song.SongId, cloud.Song.SongId, StringComparison.OrdinalIgnoreCase))))
                        .OrderByDescending(x => x.Score).ToList();
                }
            }

            if (!cancellationToken.IsCancellationRequested && cloudResult.Artists != null)
            {
                if (localResult.Artists == null)
                {
                    localResult.Artists = cloudResult.Artists;
                }
                else
                {
                    localResult.Artists = localResult.Artists.Union(cloudResult.Artists
                        .Where(cloud => localResult.Artists.All(local => !string.Equals(((Artist)local.Playlist).GoogleArtistId, ((Artist)cloud.Playlist).GoogleArtistId, StringComparison.OrdinalIgnoreCase))))
                        .OrderByDescending(x => x.Score).ToList();
                }
            }

            if (!cancellationToken.IsCancellationRequested && cloudResult.Albums != null)
            {
                if (localResult.Albums == null)
                {
                    localResult.Albums = cloudResult.Albums;
                }
                else
                {
                    localResult.Albums = localResult.Albums.Union(cloudResult.Albums
                        .Where(cloud => localResult.Albums.All(local => !string.Equals(((Album)local.Playlist).GoogleAlbumId, ((Album)cloud.Playlist).GoogleAlbumId, StringComparison.OrdinalIgnoreCase))))
                        .OrderByDescending(x => x.Score).ToList();
                }
            }

            return localResult;
        }

        private async Task<SearchResult> SearchCloudAsync(string query, CancellationToken cancellationToken)
        {
            if (this.applicationStateService.IsOnline())
            {
                return await this.allAccessService.SearchAsync(query, cancellationToken);
            }
            else
            {
                return new SearchResult(query);
            }
        }

        private async Task<SearchResult> SearchLocalAsync(string query, CancellationToken cancellationToken)
        {
            var searchResult = new SearchResult(query);

            var normalizedQuery = query.Normalize();

            searchResult.Songs = (await this.songsRepository.SearchAsync(query))
                .Select(x => new SearchResultEntity() { Song = x, Score = CalculateScore(x.TitleNorm, normalizedQuery) })
                .ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.Artists = (await this.playlistsService.SearchAsync(PlaylistType.Artist, query))
                .Select(x => new SearchResultEntity() { Playlist = x, Score = CalculateScore(x.TitleNorm, normalizedQuery)})
                .ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.Albums = (await this.playlistsService.SearchAsync(PlaylistType.Album, query))
                .Select(x => new SearchResultEntity() { Playlist = x, Score = CalculateScore(x.TitleNorm, normalizedQuery) })
                .ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.Genres = (await this.playlistsService.SearchAsync(PlaylistType.Genre, query))
                .Select(x => new SearchResultEntity() { Playlist = x, Score = CalculateScore(x.TitleNorm, normalizedQuery) })
                .ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.UserPlaylists = (await this.playlistsService.SearchAsync(PlaylistType.UserPlaylist, query))
                .Select(x => new SearchResultEntity() { Playlist = x, Score = CalculateScore(x.TitleNorm, normalizedQuery) })
                .ToList();

            if (cancellationToken.IsCancellationRequested)
            {
                return searchResult;
            }

            searchResult.RadioStations = (await this.playlistsService.SearchAsync(PlaylistType.Radio, query))
                .Select(x => new SearchResultEntity() { Playlist = x, Score = CalculateScore(x.TitleNorm, normalizedQuery) })
                .ToList();

            return searchResult;
        }

        private static double CalculateScore(string text, string query)
        {
            return 1000d - Math.Max(50d, 100 * text.IndexOf(query, StringComparison.OrdinalIgnoreCase));
        }
    }
}