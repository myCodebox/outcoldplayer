// --------------------------------------------------------------------------------------------------------------------
// Outcold Solutions (http://outcoldman.com)
// --------------------------------------------------------------------------------------------------------------------
namespace OutcoldSolutions.GoogleMusic.Shell
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using OutcoldSolutions.GoogleMusic.Models;
    using OutcoldSolutions.GoogleMusic.Repositories;
    using OutcoldSolutions.GoogleMusic.Services;
    using OutcoldSolutions.GoogleMusic.Views;

    using Windows.ApplicationModel.Search;
    using Windows.Storage.Streams;

    public class SearchService : ISearchService
    {
        private const int MaxResults = 5;

        private const string Artists = "Artists";
        private const string Albums = "Albums";
        private const string Genres = "Genres";
        private const string Playlists = "Playlists";
        private const string Songs = "Songs";

        private readonly INavigationService navigationService;
        private readonly IDispatcher dispatcher;
        private readonly IPlaylistCollectionsService playlistCollectionsService;
        private readonly ISongsRepository songsRepository;

        private bool isRegistered;

        public SearchService(
            INavigationService navigationService, 
            IDispatcher dispatcher,
            IPlaylistCollectionsService playlistCollectionsService,
            ISongsRepository songsRepository)
        {
            this.navigationService = navigationService;
            this.dispatcher = dispatcher;
            this.playlistCollectionsService = playlistCollectionsService;
            this.songsRepository = songsRepository;

            this.navigationService.NavigatedTo += this.OnNavigatedTo;
        }

        public event EventHandler IsRegisteredChanged;

        public bool IsRegistered
        {
            get
            {
                return this.isRegistered;
            }

            private set
            {
                this.isRegistered = value;
                this.RaiseIsRegisteredChanged();
            }
        }

        public void Activate()
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.Show();
        }

        public void SetShowOnKeyboardInput(bool value)
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = value;
        }

        private void OnNavigatedTo(object sender, NavigatedToEventArgs navigatedToEventArgs)
        {
            if (navigatedToEventArgs.View is IAuthentificationPageView
                || navigatedToEventArgs.View is IProgressLoadingView
                || navigatedToEventArgs.View is IReleasesHistoryPageView
                || navigatedToEventArgs.View is IInitPageView)
            {
                if (this.IsRegistered)
                {
                    this.Unregister();
                    this.IsRegistered = false;
                }
            }
            else
            {
                if (!this.IsRegistered)
                {
                    this.Register();
                    this.IsRegistered = true;
                }
            }
        }

        private void RaiseIsRegisteredChanged()
        {
            var handler = this.IsRegisteredChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void Register()
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = true;
            searchPane.SuggestionsRequested += this.OnSuggestionsRequested;
            searchPane.ResultSuggestionChosen += this.SearchPaneOnResultSuggestionChosen;
            searchPane.QuerySubmitted += this.SearchPaneOnQuerySubmitted;
        }

        private void Unregister()
        {
            var searchPane = SearchPane.GetForCurrentView();
            searchPane.ShowOnKeyboardInput = false;
            searchPane.SuggestionsRequested -= this.OnSuggestionsRequested;
            searchPane.ResultSuggestionChosen -= this.SearchPaneOnResultSuggestionChosen;
            searchPane.QuerySubmitted -= this.SearchPaneOnQuerySubmitted;
        }

        private async void SearchPaneOnQuerySubmitted(SearchPane sender, SearchPaneQuerySubmittedEventArgs args)
        {
            await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<ISearchPageView>(args.QueryText));
        }

        private void OnSuggestionsRequested(SearchPane sender, SearchPaneSuggestionsRequestedEventArgs args)
        {
            var searchPaneSuggestionsRequestDeferral = args.Request.GetDeferral();
            this.SearchAsync(args)
                .ContinueWith(
                x =>
                {
                    foreach (var item in x.Result)
                    {
                        if (args.Request.IsCanceled)
                        {
                            break;
                        }

                        if (item.ItemType == SearchItemType.Title)
                        {
                            args.Request.SearchSuggestionCollection.AppendSearchSeparator(((SearchTitle)item).Title);
                        }
                        else
                        {
                            var searchResult = (SearchResult)item;

                            IRandomAccessStreamReference randomAccessStreamReference = null;
                            if (searchResult.AlbumArtUrl != null)
                            {
                                randomAccessStreamReference =
                                    RandomAccessStreamReference.CreateFromUri(searchResult.AlbumArtUrl);
                            }
                            else
                            {
                                randomAccessStreamReference =
                                    RandomAccessStreamReference.CreateFromUri(new Uri("ms-appx:///Assets/SmallLogo.png"));
                            }

                            args.Request.SearchSuggestionCollection.AppendResultSuggestion(
                                searchResult.Title, searchResult.Details, searchResult.Tag, randomAccessStreamReference, "gMusic");
                        }
                    }

                    searchPaneSuggestionsRequestDeferral.Complete();
                },
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void SearchPaneOnResultSuggestionChosen(SearchPane sender, SearchPaneResultSuggestionChosenEventArgs args)
        {
            this.NavigateToTagAsync(args.Tag);
        }

        private async void NavigateToTagAsync(string tag)
        {
            var separator = tag.IndexOf(":", StringComparison.CurrentCulture);
            if (separator > 0)
            {
                var strings = tag.Split(new[] { ':' }, 2);
                if (strings.Length == 2)
                {
                    IEnumerable<Playlist> playlists = null;
                    switch (strings[0])
                    {
                        case Artists:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<Artist>().SearchAsync(strings[1]);
                                break;
                            }

                        case Albums:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<Album>().SearchAsync(strings[1]);
                                break; 
                            }

                        case Genres:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<Genre>().SearchAsync(strings[1]);
                                break;
                            }

                        case Playlists:
                            {
                                playlists = await this.playlistCollectionsService.GetCollection<MusicPlaylist>().SearchAsync(strings[1]);
                                break;
                            }

                        case Songs:
                            {
                                var songs = this.songsRepository.GetAll();
                                var song = songs.Where(
                                    x =>
                                    {
                                        if (x.Title == null)
                                        {
                                            return false;
                                        }

                                        return string.Equals(strings[1], string.Format(CultureInfo.CurrentCulture, "{0} - {1}", x.Artist, x.Title), StringComparison.CurrentCultureIgnoreCase);
                                    }).FirstOrDefault();

                                if (song != null)
                                {
                                    await this.dispatcher.RunAsync(() => this.navigationService.NavigateTo<IPlaylistPageView>(song));
                                }

                                return;
                            }
                    }

                    if (playlists != null)
                    {
                        var playlist = playlists.FirstOrDefault(x => string.Equals(x.Title, strings[1], StringComparison.CurrentCultureIgnoreCase));
                        if (playlist != null)
                        {
                            await this.dispatcher.RunAsync(() => this.navigationService.ResolveAndNavigateTo<PlaylistViewResolver>(playlist));
                        }
                    }
                }
            }
        }

        private async Task<List<ISearchItem>> SearchAsync(SearchPaneSuggestionsRequestedEventArgs args)
        {
            var result = new List<ISearchItem>();

            var artistsSearch = (await this.playlistCollectionsService.GetCollection<Artist>().SearchAsync(args.QueryText, MaxResults)).ToList();
            if (artistsSearch.Count > 0)
            {
                this.AddResults(result, artistsSearch, Artists);
            }

            if (result.Count < MaxResults)
            {
                var albumsSearch = (await this.playlistCollectionsService.GetCollection<Album>().SearchAsync(args.QueryText, MaxResults - result.Count)).ToList();
                if (albumsSearch.Count > 0)
                {
                    this.AddResults(result, albumsSearch, Albums);
                }
            }

            if (result.Count < MaxResults)
            {
                var genresSearch = (await this.playlistCollectionsService.GetCollection<Genre>().SearchAsync(args.QueryText, MaxResults - result.Count)).ToList();
                if (genresSearch.Count > 0)
                {
                    this.AddResults(result, genresSearch, Genres);
                }
            }

            if (result.Count < MaxResults)
            {
                var songs = this.songsRepository.GetAll();
                var songsSearch = songs.Where(x => Search.Contains(x.Title, args.QueryText)).Take(MaxResults - result.Count).ToList();

                if (songsSearch.Count > 0)
                {
                    result.Add(new SearchTitle(Songs));
                    foreach (var song in songsSearch)
                    {
                        result.Add(new SearchResult(
                            song.Title,
                            song.Artist,
                            string.Format(CultureInfo.CurrentCulture, "{0}:{1} - {2}", Songs, song.Artist, song.Title),
                            song.Metadata.AlbumArtUrl));
                    }
                }
            }

            if (result.Count < MaxResults)
            {
                var playlistsSearch = (await this.playlistCollectionsService.GetCollection<MusicPlaylist>().SearchAsync(args.QueryText, MaxResults - result.Count)).ToList();
                if (playlistsSearch.Count > 0)
                {
                    this.AddResults(result, playlistsSearch, Playlists);
                }
            }

            return result;
        }

        private void AddResults(List<ISearchItem> result, IEnumerable<Playlist> playlists, string title)
        {
            bool titleAdded = false;

            foreach (var playlist in playlists)
            {
                if (!titleAdded)
                {
                    result.Add(new SearchTitle(title));
                    titleAdded = true;
                }

                result.Add(new SearchResult(
                    playlist.Title,
                    string.Format(CultureInfo.CurrentCulture, "{0} songs", playlist.Songs.Count),
                    string.Format(CultureInfo.CurrentCulture, "{0}:{1}", title, playlist.Title),
                    playlist.AlbumArtUrl));
            }
        }

        private enum SearchItemType
        {
            Title = 1,

            Result = 2
        }

        private interface ISearchItem
        {
            SearchItemType ItemType { get; }
        }

        private class SearchTitle : ISearchItem
        {
            public SearchTitle(string title)
            {
                this.Title = title;
            }

            public string Title { get; private set; }

            public SearchItemType ItemType
            {
                get
                {
                    return SearchItemType.Title;
                }
            }
        }

        private class SearchResult : ISearchItem
        {
            public SearchResult(string title, string details, string tag, Uri albumArtUrl)
            {
                this.Title = title;
                this.Details = details;
                this.Tag = tag;
                this.AlbumArtUrl = albumArtUrl;
            }

            public string Title { get; private set; }

            public string Details { get; private set; }

            public string Tag { get; private set; }

            public Uri AlbumArtUrl { get; private set; }

            public SearchItemType ItemType
            {
                get
                {
                    return SearchItemType.Result;
                }
            }
        }
    }
}